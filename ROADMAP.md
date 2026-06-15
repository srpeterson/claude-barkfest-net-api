# ROADMAP.md — Barkfest

This file captures features that are desirable but out of scope for the initial
MVP. Items are listed in priority order. When starting a new phase, read this
file alongside `PROGRESS.md` to decide what to tackle next.

---

## Email Delivery Infrastructure

**Priority:** High

**Status:** Not started — `IEmailService` interface not yet defined; no provider wired

### What
Implement `IEmailService` in Application and a concrete provider implementation in Infrastructure. Provider TBD pending cost/feature research — candidates are SendGrid and Azure Communication Services. The interface abstracts the provider completely; swapping later is a single implementation swap with no handler changes.

### Why
This is the gating dependency for email verification, password reset, and email OTP two-factor authentication. Nothing else in the auth roadmap can be built until this exists.

### Approach
- `IEmailService` in `Barkfest.Application/Common/Interfaces/` — `SendAsync(string to, string subject, string htmlBody, CancellationToken)`
- Concrete implementation in `Barkfest.Infrastructure/Email/`
- Configuration: provider API key via environment variable / Azure App Service config — never committed to source control
- `NoOpEmailService` for local dev and test environments — logs the email body without sending
- Provider selected and wired in `Infrastructure/DependencyInjection.cs`

---

## Email Verification

**Priority:** High

**Depends on:** Email Delivery Infrastructure

**Status:** Partially scaffolded — `IsEmailVerified` and `VerificationToken` on `Owner`, columns in `InitialCreate` migration. Login enforcement deliberately deferred.

### What
After registration, send the owner a verification email containing a single-use token link. The owner clicks it to confirm they control the address. Unverified owners can still log in — enforcement is a separate switch that can be enabled once the existing owner base has had the opportunity to verify.

### Why
Prevents fake or mistyped emails accumulating on the platform. Required before email OTP 2FA can be offered — you cannot send a one-time code to an address the owner hasn't proven they control.

### Domain scaffolding already done
- `Owner.IsEmailVerified` (`bool`, default `false`) and `Owner.VerificationToken` (`string?`)
- `SetVerificationToken(string token)` and `MarkEmailVerified()` methods
- Columns in schema

### Still to implement
- `POST /v1/auth/verify-email?token={token}` — validates token, calls `MarkEmailVerified()`
- `POST /v1/auth/resend-verification` — rate-limited; generates new token, sends new email
- Enforcement strategy: **new registrations only** — `LoginCommandHandler` blocks login if `IsEmailVerified == false` AND `CreatedAt` is after the enforcement date; owners registered before enforcement are grandfathered and never blocked
- UI: post-registration prompt nudging the owner to check their email

---

## Forgot Password / Self-Service Reset

**Priority:** High

**Depends on:** Email Delivery Infrastructure

**Status:** Interim solution in place — "Forgot password?" opens a modal directing users to email srpeterson@outlook.com. Replace with this flow when ready.

### What
A self-service password reset flow. The owner enters their registered email, receives a time-limited reset link, clicks it, and sets a new password. Completing the reset implicitly verifies the email address — no separate verification step needed for owners who reset first.

### Why
Without this, locked-out owners have no self-service path. The interim manual email process does not scale once real users are on the platform.

### Domain scaffolding needed
- `Owner.PasswordResetToken` (`string?`) and `Owner.PasswordResetTokenExpiry` (`DateTime?`)
- `SetPasswordResetToken(string token, DateTime expiry)` and `ClearPasswordResetToken()` methods
- Migration: `AddOwnerPasswordResetToken`

### Still to implement

**Backend:**
- `POST /v1/auth/forgot-password` — accepts `{ email }`, generates 30-minute single-use token, sends reset email. Always returns `200 OK` regardless of whether the email exists — prevents user enumeration
- `POST /v1/auth/reset-password` — accepts `{ token, newPassword }`, validates token and expiry, updates `PasswordHash`, calls `MarkEmailVerified()`, calls `ClearPasswordResetToken()`

**Frontend:**
- Replace interim modal in `LoginPage.tsx` with a link to `/forgot-password`
- `/forgot-password` — email entry form → "Check your email" confirmation screen
- `/reset-password?token=...` — new password + confirm fields; on success navigate to `/login` with success message

---

## Report Abuse

**Priority:** High

**Depends on:** Admin Area

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

## Admin Area

**Priority:** High

**Status:** Not started — admin login endpoint exists; admin UI not yet started

### What
A protected section of the existing React app (`/admin`) where authenticated administrators
can manage owners, content, and other administrator accounts. Administrators log in via the
existing admin login endpoint — the checkbox on `LoginPage` is currently disabled and will
be wired up when this phase starts.

### Routes
- `/admin` — admin home, redirects to login if not authenticated as admin
- `/admin/owners` — owner list with search, filter, and status management
- `/admin/owners/{id}` — owner detail: profile info, pet grid, image management
- `/admin/admins` — administrator account management

### Owner management
- View all owners (username, name, email, registration date, active/visible status)
- Toggle `IsActive` — setting `false` locks the owner out; their pets are excluded from the public gallery via `BrowseRepository`
- Edit an owner's profile details
- Reset an owner's password — depends on Forgot Password / Self-Service Reset being built first
- Delete an owner and all their pets

### Pet and image management
- View all pets for a given owner with their gallery images
- Delete individual pet images
- Delete a pet entirely

### Administrator account management
Backend is fully built and tested. This phase adds the frontend UI only.
- Create new administrator accounts
- Change another administrator's password
- Delete an administrator (cannot delete own account — enforced by API)

### Backend status
- `POST /v1/auth/admin/login` — exists and tested
- `GET /v1/owners` — exists (admin JWT required)
- `GET /v1/admin/admins` — exists (admin JWT required)
- `PATCH /v1/owners/{id}/visibility` — exists
- `PATCH /v1/admin/owners/{id}/active` — exists and tested
- Pet/image management endpoints — need to be built

### Add pagination to the admin list endpoints
`GET /v1/owners` and `GET /v1/admin/admins` currently return the **entire** table as a flat
list with no paging (`GetAllOwnersQuery` / `GetAllAdministratorsQuery` → `GetAllAsync`). This
is fine at current scale but degrades as the owner/admin count grows. Do this as part of the
admin UI work, not piecemeal, because it is a breaking change spanning backend + frontend:
- Change the response shape from `OwnerDto[]` / `AdministratorDto[]` to `PagedResult<T>`
  (the type already exists — see `Barkfest.Application/Common/Models/PagedResult.cs` and how
  `BrowseRepository` uses it), with `page` / `pageSize` query parameters.
- Update the admin owner/admin list screens to send page params and render paging controls.
- Add a stable secondary sort (e.g. by `CreatedAt` then `Id`) so pages don't shuffle.

### Note on BrowseRepository
`BrowseRepository` filters by `IsActive && IsVisible` for the public gallery. Admin queries bypass this filter — admins must be able to see all owners and pets regardless of visibility or active state.

---

## Allow Users to Comment on Pets

**Priority:** High

**Status:** Not started

### What
Allow authenticated owners to post comments on any pet's detail page, including their own. All visitors can read comments; only authenticated owners can post them. Only the comment author or an admin may delete a comment.

### Why
Comments give the community a way to engage with pets beyond likes — adding personality, asking questions, and building connections between owners. Allowing owners to comment on their own pets lets them reply to their community, which is natural and expected behaviour.

### Owner badge
Comments posted by the pet's owner are displayed with an **Owner** badge next to their display name — transparent to readers without restricting participation.

### Approach (high level)

**Domain / Database:**
- New `Comment` entity: `CommentId` (`Guid`), `PetId` (FK → `Pets`), `OwnerId` (FK → `Owners`),
  `Body` (string, max 500 characters), `CreatedAt`
- New migration: `CreateCommentsTable`
- New `ICommentRepository` + `CommentRepository`

**Backend:**
- `POST /v1/pets/{id}/comments` — authenticated owner only; body `{ body }`; returns `201 Created`
- `GET /v1/pets/{id}/comments` — public (`[AllowAnonymous]`); returns paginated list of comments
  with commenter display name, avatar blob name, `CreatedAt`, and `isOwner` flag
- `DELETE /v1/pets/{id}/comments/{commentId}` — authenticated; only the comment author or an
  admin may delete

**Frontend:**
- Comments section below the pet gallery on `PetDetailPage`
- Public visitors see the comment list; unauthenticated users see a "Sign in to comment" prompt
- Owner badge displayed on comments made by the pet's owner
- Paginate or lazy-load if comment count grows large

---

## Application Insights — Provision and Connect

**Priority:** Medium

**Status:** Code complete and deployed — pending Azure Application Insights resource provisioning. App is live but telemetry is not flowing yet.

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

## Two-Factor Authentication

**Priority:** Medium

**Depends on:** Email Delivery Infrastructure, Email Verification

**Status:** Not started

### What
Optional second factor for owner login. After valid username and password, owners who have enabled 2FA must supply a one-time code before receiving their session cookie. Three delivery methods supported:

- **TOTP (Authenticator App)** — Google Authenticator, Authy, etc. No external service, no cost, most secure. Primary recommended option.
- **Email OTP** — short-lived numeric code sent to the verified email address. Reuses `IEmailService`. Lower friction, no extra app required.
- **SMS OTP** — code sent via text message. Provider TBD (candidates: Twilio, Azure Communication Services). Provides an independent channel from email — important if the owner's email account is compromised. Requires `ISmsService` implementation alongside `IEmailService`.

### Why
Passwords alone can be compromised via phishing or credential stuffing. A second factor significantly reduces the risk of unauthorised access. Offering all three delivery methods ensures both tech-savvy users (TOTP) and less technical users (SMS) have a path.

### Approach (high level)

**Shared infrastructure:**
- `ISmsService` in Application; concrete provider implementation in Infrastructure (provider TBD)
- `NoOpSmsService` for local dev and test

**Domain:**
- `Owner.IsTwoFactorEnabled` (`bool`, default `false`)
- `Owner.TwoFactorMethod` (enum: `Totp`, `EmailOtp`, `Sms`)
- `Owner.TotpSecret` (`string?`, encrypted at rest)
- Backup/recovery codes — set of single-use codes generated at enrolment

**Flow:**
- Login returns a short-lived challenge token (not a full session cookie) when 2FA is enabled
- New endpoint: `POST /v1/auth/2fa/challenge` — accepts `{ challengeToken, code }`, validates code, issues full session cookie on success
- Enrolment endpoints: enable, verify enrolment, disable, regenerate backup codes

**Frontend:**
- Post-login 2FA code entry screen
- Account settings section for enabling/disabling 2FA and choosing delivery method
- Recovery code display at enrolment (shown once, owner must save them)

### Local dev note
2FA should be bypassable in the Testing environment so integration tests are not blocked by the second-factor step.

---

## Image Moderation

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

The interface is already called at every upload site. Two implementation notes
for when the real moderator lands:

- **Stream contract.** A real moderator must read the image bytes, which consumes
  the upload `Stream`'s cursor. The same stream is then handed to the blob upload,
  which would read zero bytes and silently write a 0-byte blob. Before activating,
  change `IContentModerationService.IsImageSafeAsync` to take the image as
  `ReadOnlyMemory<byte>` (or `byte[]`) instead of `Stream`, buffer each upload once
  in the handler, and wrap the buffer in a fresh `MemoryStream` for the blob upload.
  This removes the cursor-ordering hazard at the type level. Applies to **both**
  upload handlers: `AddPetImagesCommandHandler` and `UploadOwnerProfileImageCommandHandler`.
- **Moderation ordering.** Today `AddPetImagesCommandHandler` moderates then uploads
  per-image inside the loop, so a later image failing moderation leaves earlier images
  already in blob storage (the partial-success result model handles this). Once
  moderation calls cost money/latency, reconsider moderating all images up front
  before uploading any.

### Relationship to Report Abuse
Image Moderation is proactive — catches inappropriate content at upload time. Report Abuse is reactive — the community flags content that slipped through. The two are complementary and together form a complete content moderation strategy.

### Reference
- Azure AI Content Safety docs: https://learn.microsoft.com/en-us/azure/ai-services/content-safety/
- `NoOpContentModerationService` contains a detailed TODO with these same steps

---

## Google Authentication

**Priority:** Medium

**Depends on:** Email Verification

**Status:** Not started — UI placeholder hidden pending implementation

### What
Allow owners to register and sign in using their Google account instead of a username and password.

### Current state
The "or continue with" divider and the Google button exist in `LoginPage.tsx` but are commented out (search for `TODO (Roadmap: Google Authentication)`). They will be restored once the backend OAuth flow is implemented.

### Why
Social sign-in reduces friction at registration — no password to create or remember. Google is the most widely used OAuth provider and covers the majority of users on both desktop and mobile.

### Schema change
`Owner.PasswordHash` becomes nullable — Google-authenticated accounts have no password. This requires a migration.

### Approach (high level)

**Backend:**
- Add OAuth 2.0 / OpenID Connect support via `Microsoft.AspNetCore.Authentication.Google`
- New endpoints: `GET /v1/auth/google` (redirect), `GET /v1/auth/google/callback`
- On callback: look up or create an `Owner` by the verified Google email; set `IsEmailVerified = true` automatically — Google has already verified the address; issue the `barkfest_auth` HttpOnly cookie
- `Owner.PasswordHash` — make nullable; migration required

**Frontend:**
- Restore the hidden Google button in `LoginPage.tsx` (search `TODO (Roadmap: Google Authentication)`)
- Wire button to `window.location.href = '/v1/auth/google'` (full redirect, not fetch)
- On return the cookie is set server-side; `AuthContext` reads `accountId` from the response body as usual

**Infrastructure:**
- Register OAuth app credentials in Google Cloud Console
- Store client ID + secret in GitHub Secrets; inject as environment variables

---

## Enforce Unique Likes Per User

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
- New `PetLike` entity: `PetLikeId` (`Guid`), `PetId` (FK → `Pets`), `OwnerId` (FK → `Owners`), `CreatedAt`
- Unique index on `(PetId, OwnerId)` to enforce the constraint at the DB level
- New migration: `CreatePetLikesTable`
- The `Pets.Likes` counter column can either be removed (derive the count from `PetLikes` rows)
  or kept as a denormalised cache updated transactionally on each like/unlike

**Backend:**
- `POST /v1/pets/{id}/likes` — check for an existing `PetLike` row before inserting;
  return `409 Conflict` (or silently ignore) if one already exists
- `DELETE /v1/pets/{id}/likes` — delete the `PetLike` row; no-op if it does not exist
- `OwnerId` from the JWT claim as the unique identifier

**Frontend:**
- Replace the current `localStorage` liked-state tracking with server-authoritative state:
  on page load, call a new `GET /v1/pets/{id}/liked` endpoint (or include `isLikedByCurrentUser`
  in the `PetDto`) so the heart icon reflects real state rather than browser memory

### Decisions
- **Guest liking:** Already restricted to authenticated owners only on the UI side (PR #16). The API endpoints remain `[AllowAnonymous]` but will move to `[Authorize]` as part of this item.
- **Counter vs. derived:** Keep the denormalised `Likes` counter on `Pet`, updated transactionally on each like/unlike. Deriving the count from `PetLikes` rows on every page load does not scale as like counts grow.

---

## Rolling Expiry — Refresh Tokens

**Priority:** Medium — address before the app handles sensitive user data or scales beyond a hobby audience

**Status:** Partially complete — HttpOnly cookie storage shipped in Phase 17; refresh token flow not yet implemented

### What's done
The access token is already stored in a `barkfest_auth` HttpOnly cookie (`Secure`, `SameSite=Strict`) — it is not accessible to JavaScript. All API calls use `credentials: 'include'`. This is the OWASP-recommended token storage approach and it is in place.

### What remains — Rolling Expiry (Refresh Tokens)
Replace the fixed 8-hour access token with a short-lived access token (e.g. 15 minutes) paired with a long-lived refresh token (e.g. 7 days) stored in a second HttpOnly cookie. Active users are kept logged in silently; inactive users are signed out after the refresh token expires.

### Why
The current 8-hour fixed expiry is acceptable today but will feel disruptive as owners spend longer authenticated sessions managing pets. A refresh token flow keeps active users logged in without requiring them to re-authenticate.

### When to revisit
When user feedback indicates the 8-hour timeout is disruptive, or before the app handles sensitive user data at scale.

### Approach (high level)

**Backend:**
- Login issues two cookies: a short-lived access token and a long-lived refresh token (both `HttpOnly; Secure; SameSite=Strict`)
- New `OwnerRefreshToken` entity — stores refresh tokens server-side to support revocation; logout and password change invalidate all tokens for that owner
- New endpoint: `POST /v1/auth/refresh` — validates refresh token cookie against stored token, issues a new access token cookie; refresh token rotation (each use issues a new refresh token, invalidates the old one)
- `POST /v1/auth/logout` clears both cookies with `Max-Age=0` and invalidates the refresh token in the database

**Frontend:**
- `api.ts` intercepts `401`, attempts a silent `POST /v1/auth/refresh`, retries the original request; on refresh failure falls back to current sign-out + login modal

---

## Result Railway — Remaining Follow-ups

**Priority:** Low

**Status:** Core migration complete (branch `enhancement/code-refactor`) — error handling moved from exceptions to `Result<T, Error>` (CSharpFunctionalExtensions) across all fallible handlers. The items below were deliberately deferred.

### Context (done)
All fallible handlers return `Result<T, Error>`; a closed `Error` DU lives in `Barkfest.Domain/Errors`; `DomainResult.Try` bridges the still-throwing domain (Depth A); `ResultExtensions` translates results to HTTP; `ValidationBehavior` is dual-mode; `ExceptionHandlingMiddleware` is now a backstop. See CLAUDE.md → **Error Handling**. Infallible queries (`CheckUsernameQuery`, `CheckDisplayNameQuery`, Browse) intentionally stay plain (don't wrap what can't fail).

### Deferred follow-ups

**1. Depth B — exception-free domain (optional purity upgrade)**
Convert entity smart constructors / setters (`Owner.Create`, `SetEmail`, etc.) from throwing `DomainException` to returning `Result`, then delete `DomainResult.Try` and `DomainException`. This makes the domain functionally pure end-to-end (the *Domain Modeling Made Functional* end-state). High churn — every setter signature plus the ~193 domain tests — but localized, because `DomainResult.Try` is the only coupling. Tackle as a dedicated effort only if the "exception-free domain" story is explicitly wanted; Depth A already captured all the robustness/performance value.

**2. Orphan-blob sweeper (robustness backstop for the #6 ordering fix)**
The blob/DB ordering fix fails toward orphaned (reclaimable) blobs and uses best-effort compensation, which can still leak a blob if the process dies mid-operation. Add a periodic reconciliation job: list blobs under `pets/{petId}/` and `owners/{ownerId}/`, compare against DB `BlobName`s, and delete unreferenced blobs older than a grace window. Safe by construction — the railway guarantees blobs are only ever *extra*, never *missing*.

**3. Optional `ConflictError` (409) for uniqueness violations**
Duplicate username/email currently surface as `DomainRuleError` → 400 (behavior-preserving). If 409 is preferred, add a `ConflictError` case to the `Error` DU, map it in `ResultExtensions` (+ its exhaustiveness test), and switch the duplicate checks in Register / UpdateOwner / CreateAdministrator. This is a deliberate external behavior change, hence deferred.

---

## UI Component Tests — React Testing Library Setup

**Priority:** Low

**Status:** Not started — pure utility functions are tested; component tests deferred. `msw` is already installed.

### What
Establish the React Testing Library component test pattern and write tests for the key UI components across the full app.

### Why deferred
A one-time setup investment best done in a dedicated session rather than piecemeal alongside feature work. The MVP test plan (`docs/test-plans/MVP-TEST-PLAN.md`) covers manual testing in the interim.

### Scope when started
- Configure `@testing-library/react`, `@testing-library/user-event`, and `msw` for API mocking
- `HomePage` — pagination, filter state, loading/empty states
- `FilterBar` — pet type options from mocked API, breed dropdown behaviour
- `PetCard` — renders name, age badge, image URL, navigates to detail page
- `PetDetailPage` — hero layout, like button, owner kebab menu visibility
- `LoginPage` / `RegisterPage` — form validation, submission, error states
- `ManagePetsPage` — pet list, bulk delete bar, hide toggle
- `EditPetModal` — pre-filled form, two-step flow, image management

---

## Password-Protected Scalar in Production

**Priority:** Low

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

## Upgrade Microsoft.OpenApi to 3.x

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
Check when a new `Microsoft.AspNetCore.OpenApi` release notes mention compatibility with `Microsoft.OpenApi` 3.x. The most reliable signal is watching the GitHub issue tracker for `dotnet/aspnetcore` rather than checking release notes periodically. Once confirmed, remove the pin in `Directory.Packages.props` and run `dotnet build` + `dotnet test` to verify.

---

## Pet Image Captions

**Priority:** Low

**Status:** Not started

### What
Allow owners to add an optional short caption to each pet image. Captions are displayed beneath the image in the gallery or as an overlay in the lightbox.

### Why deferred
No immediate use case — deferred until UI design confirms how captions will be surfaced.

### Approach (high level)
- Add `Caption` (`string?`, nullable, max 300 characters) to `PetImage`
- New migration: `AddPetImageCaption`
- New endpoint: `POST /v1/pets/{id}/images/batch-update` — accepts a list of `{ imageId, caption }` pairs; atomic (all succeed or none are saved)
- Caption max length enforced at UI level only (consistent with pet description)
- Unknown `imageId`s in batch update rejected with 400

### Notes
- Batch update is preferred over per-image update — owner edits all captions at once and saves in a single action
- Atomic behaviour matches batch delete: if any `imageId` is invalid, the entire request is rejected

---

## Aspire Container Host-Port Pinning

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

Checked on 2026-06-05 — Aspire 13.4.2 (latest) has no fix for this. Watch the GitHub issue tracker for `dotnet/aspire` for DCP port binding fixes for `AddSqlServer` and Azurite emulator ports. Once the DCP honours the port parameter, the simplest fix would be to restore hardcoded defaults in `AppHost.cs` (no User Secrets needed for a purely local-dev convenience feature).

---

## Footer Links

**Priority:** Low

**Status:** Partially complete — footer restructured to a single row of four links (About · Privacy Policy · Terms of Use · Contact); items are styled but not yet wired to real destinations.

### What
Wire up the four footer links to real pages or destinations:
- **About** — internal route `/about`; a short static page describing Barkfest
- **Privacy Policy** — internal route `/privacy-policy`; legally required once the platform has real users handling personal data
- **Terms of Use** — internal route `/terms-of-use`; legally required
- **Contact** — `mailto:` link to the official support email address

### Why
Privacy Policy and Terms of Use are legal requirements in most jurisdictions (GDPR, CCPA) once the platform collects personal data from real users. About and Contact complete the professional appearance of the site.

### Approach (high level)
- Create static page components for `/about`, `/privacy-policy`, `/terms-of-use`
- Add routes to `App.tsx`
- Replace `<span>` placeholders in `Footer.tsx` with `<Link>` for internal routes and `<a href="mailto:...">` for Contact
- Write basic content for each page

---

## Dynamic Sign-In Brand Panel Mosaic

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

## Profile Image Access Control (currently unlisted, not private)

**Priority:** Low

**Status:** Not started — profile images are served anonymously

### What
Today `GET /v1/images/owner-profile-images/{blobName}` is `[AllowAnonymous]` with no auth
check. Profile images are therefore **unlisted, not private**: the GUID-based blob name is
unguessable, but anyone who obtains the URL (referrer header, shared link, logged request,
the JSON response that exposes the blob name) can fetch the image with no token. This is a
normal MVP choice for avatars, but it is not enforced privacy.

### Why this note exists
So future "private profile image" work does not start from the false assumption that the
retrieval endpoint already enforces access control. It does not. Real privacy means adding a
JWT check to `ImagesController.GetImage` for the `owner-profile-images` container (return 403
unless the caller is authorized to view that owner's image).

### Related — caching
The image endpoint serves pet images with `Cache-Control: public, ...` and profile images
with `private, ...` precisely because profile images are unlisted — `private` keeps them out
of shared caches/CDNs. If real access control is added, the per-user browser cache remains
fine (per unguessable URL), but confirm the auth check runs on every fetch regardless of
cache state. See `ImagesController.CacheControlFor`.

---

## Order Landing Page by Pet ModifiedAt

**Priority:** High

**Status:** Not started

### What
Add a `Pet.ModifiedAt` timestamp and order the public landing/browse gallery by it
(most-recently-modified first) instead of `CreatedAt`. A pet resurfaces to the top of the
gallery when its owner meaningfully updates it.

### Why
`CreatedAt` ordering means a pet sinks permanently as newer pets are added, even after its
owner refreshes it with new photos or details. Ordering by `ModifiedAt` keeps actively
maintained pets visible and rewards owners for keeping listings fresh.

### Behaviour decisions (confirmed)
- **What bumps `ModifiedAt`:** editing pet details (name, description, breed, date of birth,
  pet type) **and** image changes (add, remove, re-feature). 
- **What does NOT:** likes. Likes use atomic `ExecuteUpdateAsync` that bypasses the change
  tracker by design, so they naturally never touch `ModifiedAt` — the landing page is "recently
  updated", not "recently liked".
- **Maintenance:** automatic. `AppDbContext.SaveChanges`/`SaveChangesAsync` stamps
  `ModifiedAt = DateTime.UtcNow` on every `Pet` entry in `Modified` state. This lines up exactly
  with the rules above: every current Pet mutation (UpdatePet, AddPetImages, RemovePetImage,
  BatchDeletePetImages, SetFeaturedImage) marks the Pet `Modified` via `context.Pets.Update(pet)`,
  while likes do not. **Watch-out:** if a future handler ever marks a Pet `Modified` for a reason
  that should *not* resurface it, this auto-stamp would still fire — revisit then.
- **Scope:** public browse/landing only (`BrowseRepository`). Owner's "My Pets" and other pet
  listings keep their current ordering.

### Approach (high level)
- Add `Pet.ModifiedAt` (initialise to `CreatedAt` in the entity so a brand-new pet sorts by
  its creation time until first modified).
- Override `AppDbContext.SaveChanges`/`SaveChangesAsync` to stamp `ModifiedAt` on `Modified`
  `Pet` entries. New (`Added`) pets keep the constructor value (= `CreatedAt`).
- EF config: map `ModifiedAt`; add an index to back the browse sort.
- Migration: add the `ModifiedAt` column; backfill existing rows `ModifiedAt = CreatedAt`.
- `BrowseRepository`: change `OrderByDescending(pi => pi.Pet.CreatedAt)` to
  `OrderByDescending(pi => pi.Pet.ModifiedAt)`, keeping the `ThenByDescending(pi => pi.Id)`
  tiebreaker added for stable pagination.
- Tests: retarget the BrowseRepository ordering/tied-timestamp tests from `CreatedAt` to
  `ModifiedAt`; add coverage that an edit and an image change each bump `ModifiedAt` and
  resurface the pet, and that a like does not.

---

