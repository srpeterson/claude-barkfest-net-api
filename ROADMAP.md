# ROADMAP.md — Barkfest

This file captures features that are desirable but out of scope for the initial
MVP. Items are listed in priority order. When starting a new phase, read this
file alongside `PROGRESS.md` to decide what to tackle next.

---

## 5. Email Verification & Password Reset

**Priority:** High
**Status:** Partially scaffolded — `IsEmailVerified` and `VerificationToken` on `Owner`, migration ready. `IEmailService` and all endpoints deferred until this phase is started.

### What
Two features built together in one phase because they share the same infrastructure
(`IEmailService`), the same token pattern, and the same chicken-and-egg problem with
existing unverified owners.

**Email Verification** — when an owner registers, send a verification email. The owner clicks
the link to confirm they control the address. Unverified owners can still log in until
enforcement is explicitly enabled.

**Password Reset** — an owner who has forgotten their password requests a reset link via their
registered email. Clicking the link takes them to a form where they set a new password. A
successful reset implicitly verifies the email address — no separate verification step needed
for owners who reset their password first.

### Why
- Ensures every registered owner controls the email on their account
- Prevents fake or mistyped emails from accumulating
- Gives owners a self-service recovery path when they forget their password
- The implicit-verification-on-reset approach elegantly handles existing owners who registered
  before verification was enforced — they prove ownership the first time they reset

### Domain scaffolding already done
- `IsEmailVerified` (`bool`, default `false`) and `VerificationToken` (`string?`) on `Owner`
- `SetVerificationToken(string token)` and `MarkEmailVerified()` methods in place
- Columns exist in the schema (included in `InitialCreate` migration)
- Login is **unenforced** — owners log in regardless of verification status

### Still to implement

**Shared infrastructure:**
- `IEmailService` interface in Application; implementation in Infrastructure (e.g. SendGrid, Mailgun, or SMTP)

**Email Verification:**
- New endpoint: `POST /v1/auth/verify-email?token={token}`
- New endpoint: `POST /v1/auth/resend-verification` (rate-limited)
- When ready to enforce: `LoginCommandHandler` checks `owner.IsEmailVerified`, throws `DomainException` if false

**Password Reset:**
- Domain: add `PasswordResetToken` (`string?`) and `PasswordResetTokenExpiry` (`DateTime?`) to `Owner`
  with `SetPasswordResetToken(string token, DateTime expiry)` and `ClearPasswordResetToken()` methods
- New migration: `AddOwnerPasswordResetToken`
- New endpoint: `POST /v1/auth/forgot-password` — accepts `{ email }`, generates a short-lived
  single-use token (30 minutes), persists it, sends reset email. Always returns `200 OK`
  regardless of whether the email exists — prevents user enumeration
- New endpoint: `POST /v1/auth/reset-password` — accepts `{ token, newPassword }`, validates
  token, updates `PasswordHash`, calls `MarkEmailVerified()`, invalidates token

**UI:**
- "Forgot password?" link on the Login modal → email entry form → "Check your email" confirmation
- `/reset-password?token=...` route → new page where the owner sets their new password
- Post-registration prompt nudging the owner to check their email for the verification link

### Local dev note
Do **not** enforce email verification in local development or test runs — use a feature flag or
environment check. In the Testing environment, reset and verification tokens should be returned
in the API response body so integration tests can complete the full flow without a real email
delivery service.

---

## 6. Two-Factor Authentication (2FA)

**Priority:** Medium
**Status:** Not started — depends on item 5 (Email Verification & Password Reset) being complete
**Depends on:** #5 Email Verification & Password Reset

### What
Add an optional second factor to the owner login flow. After a valid username and
password, owners who have enabled 2FA must supply a one-time code before receiving
a JWT. Two approaches are in scope:

- **Email OTP** — a short-lived numeric code sent to the owner's verified email address.
  Lower friction, no extra app required.
- **TOTP (Authenticator App)** — time-based one-time passwords compatible with Google
  Authenticator, Authy, and similar apps. More secure; preferred long-term.

### Why
Passwords alone can be compromised through phishing or credential stuffing. A second
factor significantly reduces the risk of unauthorised account access even when a
password is known.

### Prerequisites
- Item 5 (Email Verification & Password Reset) must be complete — 2FA via email requires a
  verified, working email address on the account
- For TOTP: a QR code enrolment flow and a shared secret stored (encrypted) on `Owner`

### Approach (high level)
1. Add `IsTwoFactorEnabled` (`bool`) and `TwoFactorSecret` (`string?`) to `Owner`
2. New endpoint: `POST /v1/auth/2fa/enable` — generates and returns a TOTP secret / QR code
3. New endpoint: `POST /v1/auth/2fa/verify` — confirms enrolment with a valid code
4. New endpoint: `POST /v1/auth/2fa/disable`
5. Modify `LoginCommandHandler` — when 2FA is enabled, return a short-lived challenge
   token instead of a full JWT; require the owner to complete `POST /v1/auth/2fa/challenge`
   with a valid code to receive the full JWT
6. Recovery codes — generate a set of single-use backup codes at enrolment

### Local dev note
2FA should be bypassable in the Testing environment so integration tests are not
blocked by the second-factor step.

---

## 7. Value Object Emails (and Other Validated Strings)

**Priority:** Low
**Status:** Not started — kept as plain `string` properties for now

### What
Introduce typed value objects — e.g. `ValidatedEmail`, `ValidatedUsername` — so the
type system enforces that these strings have passed validation rather than relying on
callers to always go through the entity setter.

### Why deferred
- Requires a cascade: if `Email` becomes a value object, `Username`, `FirstName`,
  `LastName`, `PhoneNumber`, and `Name` should follow for consistency.
- EF Core needs two construction paths: one that validates (new instances) and one
  that skips validation (DB reconstruction via `HasConversion()`). Every value object
  adds that boilerplate.
- The current setter pattern (`SetEmail()` with `private set`) already guarantees a
  string on the entity is valid — the type system benefit is incremental, not
  foundational.
- `ProfileImage` demonstrates the pattern (private constructor + `static Create()` +
  `OwnsOne()` mapping) and can serve as the template when the time is right.

### When to revisit
If a validated string type needs to travel across aggregate boundaries, appear in
domain events, or be compared across services — the value object pays for itself.
Until then, the setter guarantee is sufficient.

---

## 8. Upgrade Microsoft.OpenApi to 3.x

**Priority:** Low
**Status:** Blocked — pinned to 2.7.4 (latest 2.x)

### What
Upgrade `Microsoft.OpenApi` from 2.x to 3.x once `Microsoft.AspNetCore.OpenApi`
ships a compatible version.

### Why blocked
`Microsoft.OpenApi` 3.0 made `IOpenApiMediaType.Example` read-only. The
`Microsoft.AspNetCore.OpenApi` source generator (version 10.0.8) assigns to that
property in auto-generated code (`OpenApiXmlCommentSupport.generated.cs`), causing
a `CS0200` build error. A warning comment is in `Directory.Packages.props`.

### When to revisit
Check when a new `Microsoft.AspNetCore.OpenApi` release notes mention compatibility
with `Microsoft.OpenApi` 3.x. Once confirmed, remove the pin in
`Directory.Packages.props` and run `dotnet build` + `dotnet test` to verify.

---

## 9. Application Insights — Provision and Connect

**Priority:** High (before first Azure deployment)
**Status:** Code complete — `Azure.Monitor.OpenTelemetry.AspNetCore` wired into `ServiceDefaults`. Activates automatically when `APPLICATIONINSIGHTS_CONNECTION_STRING` is present.

### What
Provision an Azure Application Insights resource and connect it to the deployed API so logs, distributed traces, HTTP dependencies, and metrics flow into Azure Monitor.

### Steps
1. Create an **Application Insights** resource in your Azure subscription (or let Azure App Service create one during deployment)
2. Copy the **Connection String** from the Application Insights Overview blade
3. Set it as an environment variable in Azure App Service:
   **Settings → Environment variables → + Add** — name: `APPLICATIONINSIGHTS_CONNECTION_STRING`, value: the copied string
4. Redeploy / restart the app — telemetry will start flowing immediately

### What you get
- Live distributed traces for every HTTP request
- Structured log search via Log Analytics (`traces` table)
- Dependency tracking (SQL Server queries, Blob Storage calls)
- Custom metrics (ASP.NET Core + runtime)
- Alerting and availability tests via Azure Monitor

### No code changes required
The exporter self-activates on the connection string presence check in `ServiceDefaults/Extensions.cs`.

---

## 11. Pet Image Descriptions and Batch Update

**Priority:** Low
**Status:** Not started

### What
Allow owners to add an optional text description to each pet image. Descriptions
could be displayed as a caption beneath the image or as a tooltip overlay in the UI.

### Why deferred
No immediate use case — deferred until UI design confirms how descriptions will
be surfaced (caption, tooltip, etc.).

### Approach (high level)
- Add `Description` (`string?`, nullable, max length TBD) to `PetImage`
- New migration: `AddPetImageDescription`
- New endpoint: `POST /v1/pets/{id}/images/batch-update` — accepts a list of
  `{ imageId, description }` pairs; atomic (all succeed or none are saved)
- Validator: description max length enforced; unknown `imageId`s rejected

### Notes
- Batch update is preferred over per-image update to match the edit images page
  UX — owner edits all captions at once and saves in a single action
- Atomic behaviour matches batch delete: if any `imageId` is invalid, reject the
  entire request

---

## 12. UI Component Tests — React Testing Library Setup

**Priority:** Medium
**Status:** Not started — pure utility functions are tested; component tests deferred

### What
Establish a full React Testing Library setup and write component-level tests for
the landing page components: `FilterBar`, `PetGrid`, `PetCard`, and `HomePage`.

### Why deferred
Setting up React Testing Library correctly (mocking `useQuery`, the API module,
and environment variables) is a one-time investment that should be done in a single
dedicated session rather than piecemeal alongside feature work. Doing it once
ensures a consistent pattern across all component tests.

### Scope when started
- Install and configure `@testing-library/react` and `@testing-library/user-event`
- Set up `msw` (Mock Service Worker) for API mocking, or use `vi.mock` for the `api` module
- Test `PetGrid` — loading state, empty state (no filters), empty state (filters active), renders cards
- Test `PetCard` — renders pet name, age badge, description, constructs image URL correctly
- Test `FilterBar` — renders pet type options from mocked API, breed dropdown appears/disappears
- Test `HomePage` — pagination buttons show/hide based on `hasMore` and `hasPrev`

---

## 13. Image Moderation

**Priority:** Medium
**Status:** Scaffolded — `IContentModerationService` is wired into all image upload
handlers; `NoOpContentModerationService` always returns `true`

### What
Screen every uploaded image through Azure AI Content Safety before saving it to
Blob Storage. Reject images flagged as unsafe with a `400 Bad Request`.

### Why
Owners upload profile images and pet gallery images. Without moderation, the
platform can be used to store inappropriate content.

### Approach (scaffolded)
The integration point is already in place. To activate:

1. Provision an **Azure AI Content Safety** resource in your Azure subscription
2. Add the connection string / key to `appsettings` (or Aspire secrets)
3. Install NuGet: `Azure.AI.ContentSafety`
4. Implement `AzureContentModerationService : IContentModerationService`
5. Swap the registration in `Infrastructure/DependencyInjection.cs`:
   replace `NoOpContentModerationService` with `AzureContentModerationService`

No handler changes are required — the interface is already called at every upload
site.

### Reference
- Azure AI Content Safety docs: https://learn.microsoft.com/en-us/azure/ai-services/content-safety/
- `NoOpContentModerationService` contains a detailed TODO with these same steps

---

## 14. Password-Protected Scalar in Production

**Priority:** Medium
**Status:** Not started — Scalar is currently disabled in production (`IsDevelopment()` gate)

### What
Gate Scalar and the OpenAPI spec behind HTTP Basic Auth in production, controlled by config.
In development the gate is bypassed entirely — no behaviour change locally.

### Why
Occasionally there is a legitimate need to access Scalar in production — for example, calling
an admin endpoint (like changing an admin password) without a full deployment cycle. Without
this, the only options are to temporarily remove the `IsDevelopment()` guard and deploy twice,
or connect directly to the database. A password-protected Scalar avoids both.

### Approach
- Move `MapOpenApi()` and `MapScalarApiReference()` outside the `IsDevelopment()` check
- Add a middleware or endpoint filter that checks for a valid HTTP Basic Auth header on
  `/scalar/*` and `/openapi/*` routes
- Credentials stored in config: `Scalar:Username` and `Scalar:Password`
- If either config value is absent (local dev), the gate is skipped — Scalar is open as today
- If both are present (production), requests without valid Basic Auth credentials receive `401`
  with a `WWW-Authenticate: Basic` header, triggering the browser's native credential prompt
- Add `SCALAR_USERNAME` and `SCALAR_PASSWORD` to GitHub Secrets and pass them as environment
  variables in `api.yml`

---

---

## 16. Aspire Container Host-Port Pinning

**Priority:** Low
**Status:** Investigated and deferred — not feasible with Aspire 13.x (package 9.x)

### What
Pin the Docker host-side port mappings for the SQL Server and Azurite containers to
fixed values so that local SSMS and Azure Storage Explorer connection strings remain
stable across container restarts and recreations.

Target mappings investigated:
- SQL Server: host `62905` → container `1433`
- Azurite Blob: host `62902` → container `10000`
- Azurite Queue: host `62903` → container `10001`
- Azurite Table: host `62904` → container `10002`

### Why deferred

All approaches tried produced random host ports when containers were deleted and
recreated from scratch. The ports were only stable while the original containers
persisted from first creation; fresh containers always received new random bindings.

**Approaches tried:**

1. **Hardcoded `port:` parameters** — `AddSqlServer("barkfest-sql", port: 62905)` and
   `WithBlobPort(62902).WithQueuePort(62903).WithTablePort(62904)` on the Azurite emulator.
   Compiled and ran cleanly but port mappings remained random on new container creation.

2. **`appsettings.json` / `appsettings.Development.json`** — Considered but rejected.
   These files are under source control. Per-developer overrides would risk reaching `main`.

3. **User Secrets with fallback defaults** — Added SQL and Azurite port keys to the
   AppHost's User Secrets file (`d33f7c7c-6286-410c-a3dc-1393eb108232`) and read them in
   `AppHost.cs` via `builder.Configuration.GetValue<int>("SqlPort", 62905)`. Two issues
   arose: (a) `Microsoft.Extensions.Configuration.Binder` is not transitively available in
   the Aspire AppHost, causing `GetValue<int>` to silently return `0` instead of the
   supplied default; (b) even after working around that with `int.TryParse` fallback,
   port mappings were still random on fresh container creation.

**Root cause:** Aspire 13.x DCP (Developer Control Plane) does not honour the `port:`
parameter of `AddSqlServer()` or the `WithBlobPort/QueuePort/TablePort` extension
methods when creating containers for the first time. The values are accepted by the API
surface without error but are not applied to the Docker host binding.

### When to revisit

Check the Aspire release notes for a fix to host-port pinning for `AddSqlServer` and
Azurite emulator ports. The GitHub issue tracker for `dotnet/aspire` is the right place
to watch. Once the DCP honours the port parameter, the simplest fix would be to restore
hardcoded defaults in `AppHost.cs` (no User Secrets needed for a purely local-dev
convenience feature).

---

## 17. Consolidate Migrations into a Single InitialCreate

**Priority:** High — must be done before the app goes public
**Status:** Not started — to be done once all pre-launch features are complete

### What
Replace all accumulated EF Core migrations with a single clean `InitialCreate` migration
that represents the final production schema. This gives the codebase a clean baseline
with no incremental history of schema experiments and refactors.

### Why
The current migration history includes scaffolding artefacts and design changes (e.g. the
Breeds table refactor) that have no value once the schema is stable. A single `InitialCreate`
is easier to reason about, faster to apply on a fresh database, and removes noise from the
migration history that new developers would otherwise have to read through.

### Local dev steps
1. Stop Aspire and delete the persistent SQL Server volume (`barkfest-sql-data`) so the
   local container starts fresh
2. Delete all files under `src/Barkfest.Persistence/Migrations/`
3. Run `dotnet ef migrations add InitialCreate --project src/Barkfest.Persistence --startup-project src/Barkfest.Persistence`
4. Start Aspire — `MigrateAsync()` applies the single migration to the fresh database
5. Run the full test suite to confirm everything still works

### Azure steps (existing deployed database)
The Azure database already has the full schema applied via the accumulated migrations.
Since the schema itself is not changing — only the migration history — the approach is
to swap out the history record rather than drop and recreate the database:

1. **Deploy the app with the new single migration** — `MigrateAsync()` will fail on startup
   because the existing `__EFMigrationsHistory` table contains the old migration names and
   does not contain `InitialCreate`
2. **Before deploying:** manually clear and repopulate `__EFMigrationsHistory` on the Azure
   database via a one-time SQL script run in the Azure portal query editor:
   ```sql
   DELETE FROM [__EFMigrationsHistory];
   INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
   VALUES ('<timestamp>_InitialCreate', '<ef-version>');
   ```
   where `<timestamp>` matches the generated migration file name and `<ef-version>` matches
   the version in the existing history rows
3. Deploy — `MigrateAsync()` sees `InitialCreate` already applied and skips it; the app
   starts normally with the existing data intact

### When to do this
After all planned pre-launch features are merged to `main` and the schema is considered
stable. Do not do this while active feature branches exist that depend on the current
migration history.

---

## 22. Add Links to Footer

**Priority:** Low
**Status:** Not started

### What
Wire up the footer link columns (Company: About, Blog, Careers; Legal: Privacy Policy, Terms of Use, Contact) to real pages or external URLs. Currently these are plain text placeholders.

### Why
The footer looks complete visually but the links do nothing. Once the relevant content pages or policies exist, they should be linked.

### Approach (high level)
- Decide which items get internal routes vs external URLs (e.g. Privacy Policy and Terms of Use may be separate static pages)
- Replace the plain `<p>` elements in the `Footer` component in `barkfest-ui/src/pages/HomePage.tsx` with `<Link>` or `<a href>` as appropriate
- Add any new routes to `App.tsx`

---

## 26. Forgot Password — Full Self-Service Reset Flow

**Priority:** High
**Depends on:** #5 Email Verification & Password Reset
**Status:** Interim solution in place — "Forgot password?" link opens a modal directing
users to email srpeterson@outlook.com. Replace with this automated flow when ready.

### What
A self-service password reset flow: the owner enters their registered email, receives a
time-limited reset link, clicks it to land on a page where they set a new password.

### Current interim behaviour
Clicking "Forgot password?" on the Sign In page opens a modal that says:
*"Woof! Automated reset is on its way. Until then, shoot us an email and we'll get your
paws back on the keys. Don't forget to include your username: srpeterson@outlook.com"* When this feature ships, replace the modal with the real flow and
update the support email to the official address.

### Domain scaffolding already done (from Roadmap item 5)
- `Owner.PasswordResetToken` (`string?`) — add via `SetPasswordResetToken()` method
- `Owner.PasswordResetTokenExpiry` (`DateTime?`)
- Migration: `AddOwnerPasswordResetToken`

### Still to implement

**Backend:**
- `POST /v1/auth/forgot-password` — accepts `{ email }`, generates a short-lived
  single-use token (30 min), persists it, sends reset email via `IEmailService`.
  Always returns `200 OK` regardless of whether the email exists (prevents enumeration)
- `POST /v1/auth/reset-password` — accepts `{ token, newPassword }`, validates token,
  updates `PasswordHash`, calls `ClearPasswordResetToken()`
- Requires `IEmailService` (Roadmap item 5) to be implemented first

**Frontend:**
- Replace the interim modal in `LoginPage.tsx` with a link to `/forgot-password`
- `/forgot-password` — email entry form → "Check your email" confirmation screen
- `/reset-password?token=...` — new password + confirm fields; on success navigate
  to `/login` with a success message

---

## 25. Third-Party Authentication Providers (Google, Apple)

**Priority:** Medium
**Status:** Not started — UI placeholder hidden pending implementation

### What
Allow owners to register and sign in using their Google or Apple account instead of a
username and password.

### Current state
The "or continue with" divider and the Google and Apple buttons exist in `LoginPage.tsx`
but are commented out (search for `TODO (Roadmap #25)`). They will be restored once the
backend OAuth flow is implemented.

### Why
Social sign-in reduces friction at registration — no password to create or remember.
Google and Apple are the two providers expected on desktop and iOS respectively.

### Approach (high level)

**Backend:**
- Add OAuth 2.0 / OpenID Connect support to the API (e.g. via a library such as
  `Microsoft.AspNetCore.Authentication.Google` / `.Apple`)
- New endpoints: `GET /v1/auth/google` (redirect), `GET /v1/auth/google/callback`,
  same pattern for Apple
- On callback: look up or create an `Owner` by the verified email; issue the same
  `barkfest_auth` JWT as the password flow; set the HttpOnly cookie
- `Owner.PasswordHash` remains nullable — social-only accounts have no password

**Frontend:**
- Restore the hidden block in `LoginPage.tsx` (search `TODO (Roadmap #25)`)
- Wire each button to `window.location.href = '/v1/auth/google'` (full redirect, not fetch)
- On return the cookie is set server-side; the `AuthContext` reads the `accountId` from
  the login response body as usual

**Infrastructure:**
- Register OAuth app credentials in Google Cloud Console and Apple Developer portal
- Store client ID + secret in GitHub Secrets; inject as environment variables

---

## 24. Dynamic Sign-In Brand Panel Mosaic

**Priority:** Low
**Status:** Not started — currently using static local images

### What
Replace the hardcoded pet photos in the Sign In page brand panel mosaic with real images
pulled dynamically from the live browse API, so the mosaic always shows actual pets from
the community rather than static placeholders.

### Why
Static images feel disconnected from the live platform. Showing real community pets makes
the sign-in page feel alive and gives new visitors an immediate sense of what Barkfest
contains.

### Approach (high level)
- Call `GET /v1/browse/images?page=1&pageSize=4` on page load (no auth required)
- Use the first 4 results' `blobName` values to build image URLs via the existing
  `/v1/images/...` proxy route
- Fall back to the current static images if the fetch fails or returns fewer than 4 results
- `staleTime` can be short (60s) — the mosaic is decorative, not data-critical
- The 2×2 grid layout and `tall` height variants stay the same; just swap the `src`

---

## 23. Report Abuse

**Priority:** High
**Status:** Not started

### What
Allow any user (authenticated or public) to flag a pet image or a pet profile as abusive — inappropriate content, pornographic images, or anything that violates community standards. A report should be reviewed by an administrator before any action is taken.

### Why
Once real user content is publicly browsable, the platform needs a moderation pathway. Without a reporting mechanism, there is no way for the community to surface harmful content to administrators for review.

### Approach (high level)

**UI:**
- "Report" option on the pet card (accessible via a small flag icon or a kebab menu item on the Pet Detail page, visible to all users including guests)
- A lightweight modal: reason dropdown (Inappropriate image, Pornography, Spam, Other) + optional free-text box + Submit
- On submit: confirmation message ("Thanks — we'll review this shortly."); no further UI action

**API (new endpoints):**
- `POST /v1/pets/{id}/report` — body `{ reason, details? }`; open to all (no auth required); returns 204
- `GET /v1/admin/reports` — admin-only; lists all reports with pet ID, reason, reporter IP/account, timestamp
- `DELETE /v1/admin/reports/{id}` — admin dismisses a report
- `DELETE /v1/admin/reports/{id}/pet` — admin dismisses the report and removes the pet

**Domain/Persistence:**
- New `Report` entity: `ReportId`, `PetId` (FK), `Reason` (enum or string), `Details` (nullable), `ReportedAt`, `ReporterAccountId` (nullable — guests can also report)
- New `IReportRepository` + `ReportRepository`
- Migration: `CreateReportsTable`

**Admin UI:**
- Reports list in the admin area showing flagged pets with thumbnail, reason, and date
- One-click dismiss or remove-pet actions

**Rate limiting / abuse prevention:**
- Consider limiting reports to N per IP per hour to prevent report-bombing

---

## 30. Enforce Unique Likes Per User

**Priority:** Medium
**Status:** Not started — likes are currently a simple counter with no uniqueness enforcement

### What
Prevent a user from liking the same pet more than once. Currently the `POST /v1/pets/{id}/likes`
endpoint increments the counter unconditionally — a user (or bot) can call it repeatedly to
artificially inflate the count.

### Why
Without uniqueness enforcement, like counts are meaningless. A single user can pump up any
pet's count without limit, which undermines the social proof value of the feature entirely.

### Approach (high level)

**Domain / Database:**
- New `PetLike` entity: `PetLikeId` (`Guid`), `PetId` (FK → `Pets`), `OwnerId` (FK → `Owners`,
  nullable — guests tracked separately), `CreatedAt`
- For guest users (unauthenticated): track by a browser fingerprint stored in `localStorage`
  or a server-issued anonymous session cookie, depending on the chosen approach
- Unique index on `(PetId, OwnerId)` for authenticated likes to enforce the constraint at the DB level
- New migration: `CreatePetLikesTable`
- The `Pets.Likes` counter column can either be removed (derive the count from `PetLikes` rows)
  or kept as a denormalised cache updated transactionally on each like/unlike

**Backend:**
- `POST /v1/pets/{id}/likes` — check for an existing `PetLike` row before inserting;
  return `409 Conflict` (or silently ignore) if one already exists
- `DELETE /v1/pets/{id}/likes` — delete the `PetLike` row; no-op if it does not exist
- For authenticated owners: use `OwnerId` from the JWT claim as the unique identifier
- For guests: decision needed — options are anonymous session cookie, localStorage token
  passed in the request body, or simply restrict liking to authenticated users only

**Frontend:**
- Replace the current `localStorage` liked-state tracking with server-authoritative state:
  on page load, call a new `GET /v1/pets/{id}/liked` endpoint (or include `isLikedByCurrentUser`
  in the `PetDto`) so the heart icon reflects real state rather than browser memory
- Remove the `liked` local state optimism for guests if guest liking is removed

### Open questions
- Should guest (unauthenticated) users be allowed to like at all? Restricting to authenticated
  owners only is simpler and more trustworthy; allowing guests requires fingerprinting which is
  easily circumvented.
- Keep the denormalised `Likes` counter or always derive the count from `PetLikes` rows?
  The counter is faster to read but adds update complexity and drift risk.

---

## 28. Administrator Panel

**Priority:** High — required before the app goes public
**Status:** Not started — admin login endpoint exists; admin UI not started

### What
A dedicated admin area where authenticated administrators can manage owners and their
content. Administrators are a separate account type from owners (different table, different
JWT claims, different login endpoint).

**Owner management:**
- View all owners (name, username, email, registration date, active/visible status)
- Toggle `IsActive` on an owner — setting `false` locks them out of the platform
  (their pets are excluded from the public gallery via `BrowseRepository`)
- Edit an owner's profile details
- Reset an owner's password (for lost-password support requests)
- Delete an owner and all their pets

**Pet/image management:**
- View all pets for a given owner with their gallery images
- Delete individual pet images
- Delete a pet entirely

**Administrator account management:**
- Create new administrator accounts
- Change another administrator's password
- Delete an administrator (cannot delete own account — enforced by API)

### Why
Once real users are on the platform, administrators need a way to manage accounts,
handle support requests (e.g. lost password), and remove harmful content. Without this,
the only option is direct database access.

### Backend status
- `POST /v1/auth/admin/login` — exists and tested
- `GET /v1/owners` — exists (admin JWT required)
- `GET /v1/admin/admins` — exists (admin JWT required)
- `PATCH /v1/owners/{id}/visibility` — exists
- Owner `IsActive` toggle endpoint — needs to be built
- Admin pet/image management endpoints — need to be built

### Frontend approach (high level)
- Protected route: `/admin` — redirects to login if not authenticated as admin
- The existing `LoginDialog` and `LoginPage` support admin login (checkbox is currently
  disabled in `LoginDialog` — wire it up when this phase starts)
- Admin layout separate from the owner shell: no public navbar, minimal chrome
- Owner list with search/filter, status badges, action menus
- Owner detail view: profile info + pet grid with image management

### Note on BrowseRepository
`BrowseRepository` intentionally filters by `IsActive && IsVisible` for the public gallery.
Admin endpoints will use separate queries/repositories with no such filter applied —
admins must be able to see all owners and pets regardless of visibility or active state.

---

## 31. Allow Users to Comment on Pets

**Priority:** High
**Status:** Not started

### What
Allow authenticated owners to post comments on any pet's detail page. Owners cannot
comment on their own pets. All visitors can read comments; only authenticated owners
can post them.

### Why
Comments give the community a way to engage with pets beyond likes — adding personality,
asking questions, and building connections between owners. It deepens the social aspect
of the platform.

### Approach (high level)

**Domain / Database:**
- New `Comment` entity: `CommentId` (`Guid`), `PetId` (FK → `Pets`), `OwnerId` (FK → `Owners`),
  `Body` (string, max length TBD), `CreatedAt`
- New migration: `CreateCommentsTable`
- New `ICommentRepository` + `CommentRepository`

**Backend:**
- `POST /v1/pets/{id}/comments` — authenticated owner only; body `{ body }`; returns `201 Created`
  with the new comment; handler enforces that the commenter is not the pet's owner
- `GET /v1/pets/{id}/comments` — public (`[AllowAnonymous]`); returns paginated list of comments
  with commenter display name, avatar blob name, and `CreatedAt`
- `DELETE /v1/pets/{id}/comments/{commentId}` — authenticated; only the comment author or an
  admin may delete

**Frontend:**
- Comments section below the pet gallery on `PetDetailPage`
- Public visitors see the comment list; unauthenticated users see a "Sign in to comment" prompt
  in place of the input box
- Owners viewing their own pet see the comment list but no input box (same read-only treatment
  as the like button)
- Paginate or lazy-load if comment count grows large

---

## 27. Secure JWT Storage — HttpOnly Cookies and Rolling Expiry

**Priority:** Medium — address before the app handles sensitive user data or scales beyond a hobby audience
**Status:** Not started — JWT currently stored in `sessionStorage` with a fixed 8-hour expiry

### What
Two auth improvements implemented together because they touch the same backend and frontend
plumbing. Doing them in a single phase avoids opening the login endpoint, logout endpoint,
and `api.ts` request layer twice.

**HttpOnly Cookie Storage** — move the access token out of `sessionStorage` and into an
HttpOnly cookie so it is never accessible to JavaScript. This is the OWASP-recommended
approach for token storage in browser-based applications.

**Rolling Expiry (Refresh Tokens)** — replace the fixed 8-hour access token with a
short-lived access token (e.g. 15 minutes) paired with a long-lived refresh token
(e.g. 7 days) stored in a second HttpOnly cookie. Active users are kept logged in
silently; inactive users are signed out after the refresh token expires.

### Why
`sessionStorage` is readable by any JavaScript on the page. A successful XSS attack can
exfiltrate the token and fully impersonate the user. An HttpOnly cookie has no JavaScript
path to its value. The current 8-hour fixed expiry is acceptable today but will feel
disruptive as owners spend longer authenticated sessions managing pets.

### When to revisit
When user feedback indicates the 8-hour timeout is disruptive, or before the app handles
sensitive user data at scale.

### Approach (high level)

**Backend:**
- Login issues two cookies: a short-lived access token and a long-lived refresh token
  (both `HttpOnly; Secure; SameSite=Strict`)
- Response body still returns `accountId` and `expiresAt` for `AuthContext` initialisation —
  just not the token itself
- New endpoint: `POST /v1/auth/refresh` — validates refresh token cookie, issues new access
  token cookie; refresh token rotation (each use issues a new refresh token, invalidates the old one)
- `POST /v1/auth/logout` clears both cookies with `Max-Age=0`
- All authenticated endpoints continue to accept Bearer tokens for API clients (Scalar,
  integration tests) — cookies take precedence for browser sessions

**Frontend:**
- Remove `barkfest_token` from `sessionStorage` and from `AuthContext` state
- Remove `Authorization` header injection from `api.ts` — the browser sends cookies
  automatically; fetch calls need `credentials: 'include'`
- `AuthContext` retains `accountId`, `accountType`, and `profileImageBlobName` in
  `sessionStorage` (non-sensitive — UI only)
- `api.ts` intercepts `401`, attempts a silent `POST /v1/auth/refresh`, retries the
  original request; on refresh failure falls back to current sign-out + login modal

**CSRF mitigation:**
- `SameSite=Strict` is the primary CSRF defence for same-origin SPAs
- If cross-origin requests are ever needed, add a CSRF token header check
