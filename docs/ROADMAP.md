# ROADMAP.md — Barkfest

This file captures features that are desirable but out of scope for the initial
MVP. Items are listed in priority order. When starting a new phase, read this
file alongside `PROGRESS.md` to decide what to tackle next.

---

## 1. Add Pet Dialog

**Priority:** High
**Status:** Complete — merged via PR #9

### What
A modal dialog that allows an authenticated owner to add a new pet, covering both
the pet details and image upload in a single guided flow:
- Pet info form (name, pet type, breed, date of birth, description)
- Breed dropdown updates dynamically based on selected pet type
- Image upload step — minimum 1 image enforced by the UI
- Designate a featured image before saving

### Why
The Owner Pet Management Page (item 3) depends on this dialog. Building it as a
standalone component first keeps the work focused before it is embedded in the
full management page.

### Approach (high level)
- Multi-step dialog: step 1 — pet details form; step 2 — image upload with preview
- On submit: `POST /v1/pets` → `POST /v1/pets/{id}/images`
- On success: close dialog and refresh the pet list

---

## 2. Owner Profile Dialog

**Priority:** High
**Status:** Complete — smoke tested, pending PR

### What
A two-step modal dialog (`UpdateOwnerProfileDialog`) that lets an authenticated owner
manage their own account without leaving the current page:
- Step 1 — Update personal info: first name, last name, email, display name
  (username is shown as a read-only info line — it cannot be changed)
- Step 2 — Upload or remove their profile photo
- Profile photo appears as a round avatar in the Navbar immediately after save or login

### Why
Owners currently have no self-service way to update their account after registration.
This is a basic expectation of any account-based application.

### Approach (as built)
- Two-step dialog opened from the Navbar avatar button — no dedicated route
- Step 1 pre-fills all fields from `GET /v1/owners/{id}` on open
- Display name availability check fires only when the value has changed from the saved value
- Step 2 pre-loads the existing profile photo if one is set
- On save: `PUT /v1/owners/{id}` always; then upload/remove image if changed
- Profile image blob name stored in `AuthContext` + `sessionStorage` for instant Navbar avatar
- Dialog closes immediately on success — no intermediate success screen
- Password change excluded — see item 18

---

## 21. Pet Details Page

**Priority:** High
**Status:** Backend complete (branch `infrastructure/add-likes`, pending PR) — UI not started

### What
A public-facing page that opens when a user clicks a pet image on the landing page:
- Displays all gallery images for the pet
- Shows pet info: name, type, breed, age, description, like count
- Shows an owner actions menu (Edit, Delete) if the logged-in owner's ID matches `OwnerId`
- Like/Unlike button visible to all users; liked state tracked in localStorage by the UI

### Why
Clicking a pet card on the landing page currently has no destination. This page closes
the loop between browsing and viewing full pet details.

### Backend work completed
- `Likes` column added to `Pets` (`int NOT NULL DEFAULT 0`) — migration `AddPetLikes`
- `POST /v1/pets/{id}/likes` — increment likes (public, returns new count)
- `DELETE /v1/pets/{id}/likes` — decrement likes, floors at zero (public, returns new count)
- `GET /v1/pets/{id}` made `[AllowAnonymous]` — returns full `PetDto` including all images and `OwnerId`
- `OwnerId` added to `BrowseImageDto` — available at navigation time before full pet data loads

### UI approach (high level)
- Route: `/pets/{id}` — public, no auth required
- On load: `GET /v1/pets/{id}` for full pet data
- Image gallery with all pet images; featured image shown first
- Like button: compare localStorage for liked state; call POST/DELETE `/v1/pets/{id}/likes`
- Owner menu: visible only when `jwtOwnerId === pet.ownerId`; contains Edit and Delete actions
- Navigate from landing page: pass `OwnerId` (from `BrowseImageDto`) via router state for instant owner-menu decision before the full fetch completes

---

## 3. Owner Pet Management Page

**Priority:** High
**Status:** Not started

### What
A dedicated page where an authenticated owner can manage their pets:
- Add a new pet (name, type, breed, date of birth, description)
- Upload and manage pet gallery images (up to `Pet.MaxImages`)
- Designate a featured image
- Edit existing pet info
- Delete a pet

### Why
Owners register and log in but currently have no UI to actually add or manage their pets. This is the core value proposition of Barkfest.

### Approach (high level)
- New route: `/pets` — protected, owner only (stub exists)
- List of owner's pets with featured image thumbnails
- Add pet form → image upload step (minimum 1 image enforced by UI)
- Edit pet form with gallery management (reorder, add, remove images)
- Delete pet with confirmation prompt

---

## 4. Landing Page — Full Wire-Up

**Priority:** High
**Status:** Complete — merged via PR #10

### What
Ensure the landing page works correctly end-to-end once real owners and pets exist in the database:
- Latest pets shown on load (sorted by `CreatedAt` descending)
- Pet type dropdown filters correctly
- Breed dropdown updates based on selected pet type and filters correctly
- Pagination works correctly as pet count grows
- Pet cards show featured image, pet name, breed, age
- Empty states handled gracefully (no pets, no results for filter combination)

### Why
The browse API and filter UI exist but have only been tested with seeded/test data. Once real owners are posting real pets this needs to be verified end-to-end and any rough edges smoothed out.

### Approach (high level)
- Smoke test with real owner accounts and real pet data
- Verify featured image displays correctly on each card
- Verify filter combinations return correct results
- Verify pagination thresholds and "no results" empty state
- Polish any UI rough edges discovered during testing

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

## 10. Require Minimum One Image on Pet Creation (API-level enforcement)

**Priority:** Low — only relevant if the API is opened to third-party clients
**Status:** Deferred — UI enforces this rule; API intentionally does not

### Decision
The minimum 1 image rule is enforced at the UI layer only. The two-step flow
(create pet → upload images) is better UX and keeping them separate avoids
significant API and test complexity for no practical benefit while the only
consumer is the controlled `barkfest-ui` client.

### When to revisit
If the API is ever opened to third-party clients (mobile apps, integrations),
enforce this at the API layer by combining `POST /v1/pets` into a single
multipart request that requires at least 1 image file.

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

## 15. Rolling JWT Expiry

**Priority:** Low
**Status:** Not started — current token is fixed 8-hour (480-minute) expiry with 401 interception handling expiry gracefully in the UI

### What
Replace the fixed 8-hour JWT expiry with a sliding window using refresh tokens. A short-lived
access token (e.g. 15 minutes) is paired with a longer-lived refresh token (e.g. 7 days). Each
successful API call silently refreshes the access token, keeping active users logged in
indefinitely without re-prompting.

### Why deferred
The current approach (fixed 8-hour token + 401 interception that signs out and opens the
login modal) is sufficient for real-world Barkfest usage. Public browsing never triggers a 401.
Only authenticated actions (posting pets, profile management) are affected by expiry, and those
are infrequent enough that re-login after 8 hours is acceptable for the current user base.

### When to revisit
If user feedback indicates the 8-hour timeout is disruptive — particularly once owner pet
management UI is fully built and users are doing longer authenticated sessions.

### Approach (high level)
- Issue a short-lived access token + long-lived refresh token (HttpOnly cookie) at login
- New endpoint: `POST /v1/auth/refresh` — validates refresh token, issues new access token
- `api.ts` intercepts `401`, attempts silent refresh, retries the original request
- On refresh failure (expired or revoked refresh token), fall back to current 401 handler (sign out + login modal)
- Refresh token rotation: each use issues a new refresh token and invalidates the old one

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

## 18. Change Owner Password

**Priority:** Medium
**Status:** Not started

### What
Allow an authenticated owner to change their own password from within the app:
- Requires the current password to confirm identity
- New password must meet the minimum strength requirements (`AccountConstraints.PasswordMinLength`)
- Confirm new password field (client-side match check)

### Why
Owners currently have no self-service way to change their password after registration.
This is a basic account security expectation.

### Approach (high level)
- New command: `ChangeOwnerPasswordCommand(Guid Id, string CurrentPassword, string NewPassword)`
- New endpoint: `POST /v1/owners/{id}/change-password` → 204 NoContent
- Handler verifies current password via `IPasswordHasher.Verify` before accepting the new one
- UI: a small standalone "Change Password" dialog or section, separate from the main profile update flow

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

## 19. Update Pet Dialog

**Priority:** High
**Status:** Not started

### What
A dialog that allows an authenticated owner to edit an existing pet's details and manage
its gallery images:
- Update pet info (name, breed, date of birth, description)
- Add new images to the gallery (up to `Pet.MaxImages` total)
- Remove existing images
- Change the featured image

### Why
Owners can currently add pets but have no way to correct mistakes or update information
after the fact. A clean edit flow is essential before the app goes public.

### Approach (high level)
- Pre-fill all fields with the pet's current data on open (same pattern as `UpdateOwnerProfileDialog`)
- Re-use `PetTypeBreedFormFields` for breed selection
- Image management: show existing images with remove buttons, allow adding new ones up to the limit
- `PUT /v1/pets/{id}` — update pet info
- `POST /v1/pets/{id}/images` — add new images
- `DELETE /v1/pets/{id}/images/{imageId}` — remove individual images
- `PUT /v1/pets/{id}/images/{imageId}/featured` — change featured image

---

## 20. Styled Authenticated Navbar

**Priority:** High
**Status:** Not started

### What
Re-style the Navbar for authenticated owners to give it a polished, on-brand look that
goes beyond the current minimal layout.

### Why
The authenticated state of the Navbar currently has the same basic layout as the public
state. As owner features grow (profile, pets, etc.) the Navbar needs a more intentional
design that reflects the owner's logged-in experience.

### Approach (high level)
- Design to be agreed during the feature session
- Consider: dropdown menu from the avatar, navigation links, notification indicators

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

## 27. Migrate JWT Storage from sessionStorage to HttpOnly Cookies

**Priority:** Medium — address before the app handles sensitive user data or scales beyond a hobby audience
**Status:** Not started — JWT currently stored in `sessionStorage`

### What
Replace `sessionStorage`-based JWT storage with a server-set HttpOnly cookie so the
access token is never accessible to JavaScript. This is the OWASP-recommended approach
for token storage in browser-based applications.

### Why
`sessionStorage` (and `localStorage`) are readable by any JavaScript running on the page.
A successful XSS attack — even a minor one via a third-party dependency — can exfiltrate
the token and fully impersonate the user. An HttpOnly cookie cannot be read by JavaScript
at all; the browser sends it automatically with every same-origin request and XSS has no
path to the token value.

### Approach (high level)

**Backend:**
- On successful login, set the JWT as an HttpOnly, Secure, SameSite=Strict cookie
  (`Set-Cookie: barkfest_auth=<jwt>; HttpOnly; Secure; SameSite=Strict; Path=/`)
- The response body can still return `accountId` and `expiresAt` for the UI to initialise
  `AuthContext` (account type, avatar, etc.) — just not the token itself
- `POST /v1/auth/logout` clears the cookie by setting it with `Max-Age=0`
- All authenticated endpoints continue to accept Bearer tokens for API clients (Scalar,
  integration tests) — the cookie takes precedence for browser sessions

**Frontend:**
- Remove `barkfest_token` from `sessionStorage` and from `AuthContext` state
- Remove `setAuthToken` / `Authorization` header injection from `api.ts` — the browser
  sends the cookie automatically; fetch calls need `credentials: 'include'`
- `AuthContext` retains `accountId`, `accountType`, and `profileImageBlobName` in
  `sessionStorage` (these are not sensitive — they drive UI only, not access)
- On reload, non-sensitive fields restore from `sessionStorage` as today; authenticated
  API calls work immediately because the cookie is present

**CSRF mitigation:**
- `SameSite=Strict` is the primary CSRF defence for same-origin SPAs
- If cross-origin requests are ever needed, add a CSRF token header check

### Note on Roadmap item 15
Item 15 (Rolling JWT Expiry) also proposes HttpOnly cookies for the refresh token. These
two items should be implemented together in a single phase — migrating the access token
to a cookie and introducing a refresh token cookie at the same time avoids doing the
backend auth plumbing twice.
