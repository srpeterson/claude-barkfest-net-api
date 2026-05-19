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

## 2. Image Moderation

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
