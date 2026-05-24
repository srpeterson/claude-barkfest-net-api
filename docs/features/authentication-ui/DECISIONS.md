# Authentication UI — Decisions

Decisions made during the Authentication UI feature.

---

## HttpOnly cookie for JWT storage

**Decision:** The JWT is set as an HttpOnly cookie on login rather than returned
in the response body.

**Reason:** `sessionStorage` and `localStorage` are accessible to any JavaScript running
on the page — an XSS vulnerability anywhere in the frontend can silently exfiltrate
a token stored there. An HttpOnly cookie cannot be read by JavaScript at all; the
browser attaches it to requests automatically. Combined with `Secure` (HTTPS only)
and `SameSite=Strict` this is the correct default for any application handling
authenticated sessions. This decision was established in `docs/DECISIONS.md` during
Phase 12 and is implemented here.

**Impact on `LoginResponse`:** The response body contains only `accountId` and
`expiresAt` — no token. The token lives exclusively in the HttpOnly cookie.

---

## `sessionStorage` for client-side auth state (not the token)

**Decision:** `AuthContext.tsx` stores `barkfest_authenticated`, `barkfest_account_id`,
and `barkfest_account_type` in `sessionStorage`. The JWT itself is never stored
client-side — it lives only in the HttpOnly cookie.

**Reason:** Persisting auth state in `sessionStorage` means opening the app in a new
tab starts a fresh, unauthenticated session, which is the safest default. The user
re-authenticates each browser session. `localStorage` was considered but rejected:
data persisting across browser restarts without a matching cookie would leave the UI
in a permanently-logged-in state after the cookie (and thus the token) has expired.
`sessionStorage` lifetime matches the browser tab — it resets naturally when the tab
is closed. The stored values are identity metadata (`accountId`, `accountType`), never
the security credential itself.

---

## `credentials: 'include'` on all API requests

**Decision:** All `fetch` calls in `api.ts` include `credentials: 'include'`.

**Reason:** The HttpOnly auth cookie is a cross-origin credential (the frontend dev
server runs on a different port than the API). `credentials: 'include'` is required
for the browser to send cross-origin cookies. Without it, the auth cookie would be
silently omitted from every request and every protected endpoint would return 401.

---

## 401 interception via `setUnauthorizedHandler`

**Decision:** `api.ts` exposes a `setUnauthorizedHandler(fn)` callback. `App.tsx`
registers a handler that calls `signOut()` + `openLoginModal()` on any 401 response.

**Reason:** When a JWT expires the API returns 401. Without interception, the user
would see a raw error or a broken UI state. The handler automatically signs the user
out and re-prompts login — a smooth recovery with no manual refresh required.
`setUnauthorizedHandler` keeps `api.ts` framework-agnostic (no React imports) while
still allowing the auth context to respond to expired tokens.

---

## Admin login checkbox — disabled in Phase 17, enabled in admin UI phase

**Decision:** The "I am an Administrator" checkbox is present in `LoginModal.tsx` but
is rendered with `disabled` and styled `opacity-40 cursor-not-allowed`. The admin login
code path (`adminLogin()` + `signIn(..., 'admin')`) is wired but unreachable from the UI
until the checkbox is re-enabled.

**Reason:** Admin login via the same modal as owner login is a temporary measure. The
correct long-term UI will be a dedicated admin area with its own route. For now, admin
login is available via Scalar (`POST /v1/auth/admin/login`) without needing the UI.
The checkbox is hidden from practical use but the code is tested and ready — enabling
it requires removing the `disabled` attribute only.

**Admin checkbox state persistence:** Unlike username, password, and error (which reset
when the modal closes), `isAdmin` state persists across modal open/close cycles. This is
intentional — if an admin opens the modal, closes it, then reopens it, the checkbox
should still be checked. Resetting it on close would be more surprising than preserving it.

---

## zxcvbn for password strength — score ≥ 2 required

**Decision:** `RegisterModal.tsx` uses the `zxcvbn` library to estimate password strength.
A score of 0 or 1 (Very weak / Weak) blocks form submission. Score ≥ 2 is required.

**Reason:** Enforcing character-class rules (uppercase + number + symbol) produces
predictable substitutions (`Password1!`) without improving security. NIST SP 800-63B
explicitly recommends against complexity requirements. `zxcvbn` evaluates actual
strength based on dictionary patterns and entropy — `Correct Horse Battery Staple`
scores higher than `P@ssw0rd!`. Score ≥ 2 ("Fair") is a practical minimum: it blocks
trivially weak passwords while allowing strong passphrases without symbol requirements.
The strength meter and contextual hint guide the user rather than hard-blocking them
with cryptic rules.

---

## Generic error messages for auth failures

**Decision:** Registration failures show "Woof! Something went wrong! Check your details
and try again." Login failures show "Invalid username or password." Neither message
reveals whether the username exists, what specifically failed, or any server-side detail.

**Reason:** Specific errors ("username already taken", "email already registered") enable
user enumeration — an attacker can probe the API to discover which usernames or emails
are registered. A generic message gives the user enough to act on (re-check your details,
try a different username) without leaking membership information.

---

## `ProtectedRoute` checks `accountType === 'owner'`

**Decision:** `ProtectedRoute.tsx` requires `isAuthenticated && accountType === 'owner'`.
Admin users who land on a protected route are redirected to `/`.

**Reason:** Owner pages (e.g. `/pets`, `/profile`) use `ICurrentUserService.OwnerId` from
the JWT `sub` claim. An admin JWT has a different `sub` (the admin's GUID) — the handler
would fetch the wrong record or find nothing. Admin users need purpose-built moderation
views, not owner views. Redirecting admins to the home page is the safest default until
an admin UI is built.

---

## Password minimum length bumped to 10

**Decision:** `AccountConstraints.PasswordMinLength` was raised from 8 to 10 during
UI development.

**Reason:** The zxcvbn strength meter was configured to require score ≥ 2. An 8-character
password of all the same digit (e.g. `11111111`) passes the minimum length check but
scores 0 on zxcvbn — the UI would block it. Raising the backend minimum to 10 makes the
backend enforcement more consistent with the intent of the frontend guidance: a password
that short is weak by any measure. The zxcvbn score is the primary gate; the length
minimum is the fallback for non-UI callers.
