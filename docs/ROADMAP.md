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

## 3. Upgrade Microsoft.OpenApi to 3.x

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

## 4. Application Insights — Provision and Connect

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

## 5. Image Moderation

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
