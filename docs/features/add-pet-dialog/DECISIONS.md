# Add Pet Dialog — Decisions

Decisions made during the Add Pet Dialog feature.

---

## Image format support — JPG and PNG only

**Decision:** The dialog (and the API) accept only `image/jpeg`, `image/jpg`, and `image/png`.

**Reasoning:**
- On iOS Safari, the browser automatically converts HEIC photos to JPEG before passing
  them to the web app. iPhone users selecting from their photo library are unaffected.
- The primary mobile use case (iPhone + Safari) works without any changes.

**Known limitations:**
- Desktop users with HEIC files synced from iCloud or AirDrop would be blocked. HEIC
  files are not auto-converted outside of a mobile browser context.
- WebP images (increasingly common on Android and in browsers) are not accepted.

**When to revisit:**
- If user feedback indicates upload failures on desktop with HEIC files.
- If WebP becomes a common source format for users.
- Adding a new format requires updating `SupportedImageType` in `Barkfest.Domain`
  and the `accept` filter in both `AddPetDialog.tsx` and `AddPetImagesCommandValidator`.

---

## File size limits — 10 MB per file, 65 MB total request

**Decision:** Each uploaded image is capped at 10 MB. The API endpoint accepts a maximum
request body of 65 MB.

**Reasoning:**
- 10 MB per file is generous for a JPEG or PNG photo from a phone camera, while
  preventing runaway uploads from raw or uncompressed files.
- 65 MB total gives comfortable headroom for the maximum of 6 images (6 × 10 MB = 60 MB)
  with a small buffer for multipart framing overhead.
- Enforced at two layers:
  - **Frontend** — react-dropzone `maxSize` rejects oversized files before upload with
    a user-facing error message. Constant `MAX_IMAGE_SIZE_BYTES` in `AddPetDialog.tsx`
    mirrors `PetImage.MaxImageSizeBytes`.
  - **Backend** — `AddPetImagesCommandValidator` validates `PetImageUpload.Length` against
    `PetImage.MaxImageSizeBytes`. `[RequestSizeLimit]` and `[RequestFormLimits]` on the
    `AddImages` action in `PetController` enforce the 65 MB ceiling at the HTTP layer
    before the request even reaches MediatR.

**When to revisit:**
- If average photo sizes increase (e.g. higher-resolution phone cameras become the norm).
- Changing the per-file limit requires updating `PetImage.MaxImageSizeBytes` in
  `Barkfest.Domain` and `MAX_IMAGE_SIZE_BYTES` in `AddPetDialog.tsx`.
- Changing the request limit requires updating both attributes on the `AddImages` action.

---

## Auth — switched from HttpOnly cookie to Bearer token

**Decision:** Replace the HttpOnly cookie (`barkfest_auth`) with an `Authorization: Bearer`
header. The JWT is returned in the login response body (`accessToken`) and stored in
`sessionStorage`. Every authenticated request adds the header explicitly.

**Reasoning:**
- Discovered during smoke-testing of the Add Pet dialog: `POST /v1/pets` was returning 401
  even though the user was logged in. Root cause: the cookie was set with `SameSite=Strict`
  by `https://localhost:7001` and the frontend runs on `http://localhost:5173`. Modern Chrome
  treats different schemes as cross-site, so `SameSite=Strict` prevented the cookie from being
  sent on fetch requests — silently.
- The same cross-origin mismatch exists in production on Azure (frontend and API on different
  domains). `SameSite=None` would have fixed it but trades CSRF protection for convenience.
- Bearer tokens sidestep the entire SameSite problem. The `Authorization` header is explicit,
  works identically on any origin combination, and is immune to CSRF (a foreign site's
  JavaScript cannot forge an `Authorization` header).
- Standard pattern for SPAs per OAuth 2.0 BCP and OWASP guidance.

**Trade-off accepted:**
- `sessionStorage` is readable by JavaScript (unlike HttpOnly cookies). XSS can steal the
  token. Mitigated by: this is not a high-value-target application; `sessionStorage` is already
  used for other auth state; Content Security Policy can be added later if needed.
- `sessionStorage` is tab-scoped and cleared on browser close. Users must log in each new
  session (no persistent "remember me"). Acceptable for this application.

**What changed:**
- `AuthController.cs` — login/admin login return `accessToken` in response body; no cookies set or deleted
- `ServiceRegistration.cs` — removed `OnMessageReceived` cookie extraction event
- `AuthContext.tsx` — `signIn()` accepts and stores `token`; `token` persisted to `sessionStorage`
- `api.ts` — `setAuthToken()` + `Authorization: Bearer` header on all requests; no `credentials: 'include'`
- `App.tsx` — syncs `token` from context to `api.ts` via `useEffect`
- `LoginModal.tsx`, `RegisterModal.tsx` — pass `result.accessToken` to `signIn()`

**Important for Azure deployment:**
`Cors:AllowedOrigin` in Azure App Configuration must be set to the production frontend URL.
The CORS policy (`AllowCredentials()` + specific origin) is the layer that controls which
origins can call the API at all — it remains the primary CSRF defence even without SameSite.

---

## Session timeout — 8 hours (480 minutes)

**Decision:** Set `Jwt:ExpiryMinutes` to `480` (8 hours) in `appsettings.json`.

**Reasoning:**
- The original value of 60 minutes was too short for a social app where users browse,
  post pets, and manage their profile across a normal day.
- When a token expires, the next API call returns 401, the `unauthorizedHandler` fires,
  `signOut()` is called, and the login modal appears — interrupting the user mid-action.
  60 minutes makes this a common occurrence. 8 hours makes it a rare one.
- Reviewed three options:
  1. Increase `ExpiryMinutes` — one config change, solves the practical problem. ✅ Chosen.
  2. Proactive expiry warning — store `expiresAt`, run a timer, show a banner. Extra
     complexity for a problem that no longer occurs at 8-hour expiry.
  3. Refresh tokens — correct long-term solution but 2–3 days of work touching every
     layer. Deferred to ROADMAP item 15.
- 8 hours covers a full working day. The token is in `sessionStorage` so it is always
  cleared when the browser closes — there is no persistent stale-token risk.

**When to revisit:**
- If refresh tokens (ROADMAP item 15) are implemented, `ExpiryMinutes` should be shortened
  to 15 minutes (access token) to match the short-lived access token pattern.
- If compliance or security requirements demand shorter sessions.

---

## Birthday/age toggle — custom radio buttons

**Decision:** Use custom-styled radio buttons (`sr-only` native input + Tailwind circle) to
toggle between the date picker ("I know the date") and the age stepper ("Not sure").

**Journey:**
1. **Checkbox** — original implementation; had a `checked={false}` bug (never visually checked)
   and semantically wrong (checkbox = yes/no, not a choice between two modes).
2. **shadcn Switch** — installed `@base-ui/react/switch` via shadcn. Looked correct but the
   off-state `bg-input` colour looked disabled. Changing the off state to `bg-primary` (always
   orange) removed the visual ambiguity but a toggle switch still implies on/off rather than
   a mode choice. Users found it confusing.
3. **Custom radio buttons** ✅ Chosen — two options always visible, selected state driven by
   a custom orange ring + inner dot. `sr-only` native `<input type="radio">` preserved for
   keyboard accessibility. Consistent across desktop and mobile (native radio was grey on iOS).

**What changed:**
- `AddPetDialog.tsx` — Switch import removed; radio buttons render custom divs
- `barkfest-ui/src/components/ui/switch.tsx` — installed but not used in the dialog;
  retained as it will be needed for future toggle settings (e.g. owner profile preferences)

---

## Description — required in UI, optional in API

**Decision:** `Description` is marked required in the Add Pet dialog (red `*`, included in
`step1Valid`) but the API and domain layer continue to treat it as optional.

**Reasoning:**
- Richer descriptions improve the social experience — a pet with no description is less
  engaging for other users browsing the feed.
- Enforcing at the UI layer is the lowest-friction change and covers the primary creation
  path without breaking any existing API contracts or requiring a migration.
- Admin tools or future bulk-import paths may legitimately create pets without descriptions,
  so the backend staying optional preserves that flexibility.

**When to revisit:**
- If descriptions should be enforced at the API level, add a `NotEmpty()` rule to
  `CreatePetCommandValidator` in `Barkfest.Application`.

---

## Age selector — custom +/− stepper

**Decision:** Replace the `NativeSelect` dropdown (1–21 years) with a custom +/− stepper
rendered as a single pill-shaped container matching the other form inputs.

**Reasoning:**
- A dropdown for a 1–21 range requires opening a menu to change a number by one. A stepper
  is more tactile, especially on mobile where large tap targets matter.
- The stepper disables `−` at 1 and `+` at 21, making the valid range self-evident.
- Displays "X year old" / "X years old" inline — more readable than a plain number option.
- Empty state shows "Tap + to set age" as a clear prompt, keeping the required-field
  contract (`step1Valid` requires `age !== ''`) visible to the user.

**What changed:**
- `AddPetDialog.tsx` — `NativeSelect` import removed; stepper uses `Minus` and `Plus`
  Lucide icons with `Math.max`/`Math.min` guards.
