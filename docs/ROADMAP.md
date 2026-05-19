# ROADMAP.md — Barkfest

This file captures features that are desirable but out of scope for the initial
build. Items are listed in priority order. When starting a new phase, read this
file alongside `PROGRESS.md` to decide what to tackle next.

---

## 1. Email Verification

**Priority:** High
**Status:** Domain scaffolded — `IsEmailVerified` and `VerificationToken` on `Owner`, migration ready. Login enforcement and email sending deferred until `IEmailService` is implemented.

### What
When an owner registers, send a verification email containing a one-time token.
The owner must click the link to activate their account. Unverified owners cannot
log in.

### Why
Ensures every registered owner controls the email address on their account.
Prevents fake or mistyped emails from accumulating. Protects against basic abuse
at the registration endpoint.

### Approach

**Done:**
- `IsEmailVerified` (`bool`, default `false`) and `VerificationToken` (`string?`) added to `Owner`
- `SetVerificationToken(string token)` and `MarkEmailVerified()` methods in place
- Migration included in `InitialCreate` — columns exist in the schema
- Login is **unenforced** — owners log in regardless of verification status

**To implement:**
- New `IEmailService` interface in Application; implementation in Infrastructure
  (e.g. SendGrid, Mailgun, or SMTP)
- New endpoint: `POST /v1/auth/verify-email?token={token}`
- New endpoint: `POST /v1/auth/resend-verification` (rate-limited)
- When ready to enforce: `LoginCommandHandler` checks `owner.IsEmailVerified`,
  throws `DomainException` if false

### Local dev note
Do **not** enforce email verification during local development or in test runs.
Use a feature flag or environment check so tests and seeding are not blocked by
needing a real email delivery service.

---

## 2. Value Object Emails (and Other Validated Strings)

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

## 3. Image Moderation

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
