# DECISIONS.md — Barkfest

This file records every significant architectural and technical decision made
during the design of Barkfest, along with the reasoning behind each choice.
When a future decision revisits one of these topics, read this file first to
understand the original intent before changing anything.

---

## Architecture

### Decision: Clean Architecture
**Choice:** Clean Architecture with four distinct layers — Domain, Application,
Persistence, Infrastructure — plus an API entry point.

**Reason:** Enforces strict separation of concerns. Business logic lives in
Domain and Application and has zero knowledge of databases, frameworks, or
external services. Infrastructure concerns (EF Core, Azure Blob Storage) are
swappable without touching business logic. Highly testable at every layer.

---

### Decision: Separate `Barkfest.Persistence` project
**Choice:** EF Core and all database concerns live in `Barkfest.Persistence`,
not inside `Barkfest.Infrastructure`.

**Reason:** `Infrastructure` was becoming a catch-all. EF Core is a substantial
concern that deserves its own project. Separating them means swapping the ORM
(e.g. adding Dapper for read-heavy queries) only touches `Persistence` and
never affects blob storage or email code in `Infrastructure`. Clearer ownership
for developers — database work goes in one place, external service integrations
go in another.

---

### Decision: Repository interfaces in Domain, implementations in Persistence
**Choice:** `IOwnerRepository` and `IPetRepository` are defined in
`Barkfest.Domain`. `OwnerRepository` and `PetRepository` are implemented in
`Barkfest.Persistence`.

**Reason:** Dependency Inversion Principle. The Domain defines the contract.
Infrastructure fulfils it. Application layer depends only on the abstraction —
never on EF Core directly. Repositories can be swapped or mocked in tests
without touching business logic.

---

### Decision: `IBlobStorageService` in Application, implementation in Infrastructure
**Choice:** `IBlobStorageService` interface lives in `Barkfest.Application`.
`AzureBlobStorageService` lives in `Barkfest.Infrastructure`.

**Reason:** Keeps Application layer testable without the Azure SDK. If blob
storage provider changes from Azure to S3 or similar, only
`Barkfest.Infrastructure` changes. Application handlers never know which
provider is in use.

---

## Primary Keys

### Decision: `Guid` primary keys using `Guid.CreateVersion7()`
**Choice:** All primary keys are `Guid`, initialised application-side using
`Guid.CreateVersion7()`. SQL Server column default set to `newsequentialid()`.

**Reason:** `int` auto-increment keys are sequential and guessable
(`GET /api/owners/1`, `GET /api/owners/2`) — a security risk. GUIDs are
non-guessable. Random GUIDs (`Guid.NewGuid()`) cause index fragmentation in
SQL Server because inserts are non-sequential. `Guid.CreateVersion7()` is
time-ordered so inserts remain sequential, eliminating fragmentation while
retaining the security benefits of GUIDs. Application-side generation means
the ID is known before hitting the database — useful for CQRS and event
sourcing patterns.

---

## Database

### Decision: Prefixed primary key column names
**Choice:** All primary key `Id` properties map to `{EntityName}Id` in the
database via EF Core `HasColumnName()`. For example `Owner.Id` → `OwnerId`,
`Pet.Id` → `PetId`.

**Reason:** Raw SQL and Dapper queries are self-describing in joins. `OwnerId`,
`PetId`, `PetImageId` are immediately clear without aliases. Plain `Id` is
ambiguous in any join with more than one table. Since Dapper may be introduced
later for read-heavy queries, clear column names are important from the start.

---

### Decision: Dapper compatibility considered from the start
**Choice:** Column naming and schema design takes Dapper into account even
though Dapper is not yet in use.

**Reason:** EF Core handles writes and complex queries well. Dapper may be
introduced later for performance-sensitive read queries. Prefixed column names
mean Dapper queries will be readable and maintainable without aliases.

---

### Decision: `Age` computed at runtime, never stored
**Choice:** `Pet.Age` is a computed property derived from `DateOfBirth`. It is
ignored by EF Core and never stored in the database.

**Reason:** Storing `Age` as an `int` goes stale immediately — a pet that is 2
today would still show as 2 next year. `DateOfBirth` is the source of truth.
Age is always accurate because it is calculated fresh from today's date.

---

### Decision: `DateOfBirth` as nullable `DateOnly`
**Choice:** `Pet.DateOfBirth` is `DateOnly?` rather than `int Age`.

**Reason:** Storing age as a plain `int` is simple but goes stale. `DateOnly`
is precise, never goes stale, and age is always computed correctly. Nullable
because not all owners will know their pet's exact date of birth.

---

### Decision: Migration applied at runtime via `MigrateAsync()`
**Choice:** `dotnet ef database update` is never used. The migration is applied
automatically at startup via `db.Database.MigrateAsync()` in `Program.cs`.

**Reason:** Ensures the database schema is always in sync with the application
regardless of deployment environment. No manual intervention required.

---

## Entity Design

### Decision: `Breed` as Table Per Hierarchy (TPH) with abstract base class
**Choice:** `Breed` is an abstract base class. `DogBreedInfo` and `CatBreedInfo`
extend it. EF Core uses TPH with a `BreedType` discriminator column.

**Reason:** More scalable than storing two nullable columns on `Pet`
(`DogBreedValue` and `CatBreedValue`) where one is always null. TPH means one
`Breeds` table with a discriminator — adding `HorseBreedInfo` in the future
requires only a new class, a new SmartEnum, and a new discriminator value. No
table restructuring. The domain rule that breed must match pet type is enforced
in `Pet.SetBreed()`.

---

### Decision: `ProfileImage` as a Value Object (`sealed record`)
**Choice:** `ProfileImageBlobName` and `ProfileImageContentType` were two
separate nullable string properties on `Owner` and `Pet`. They were replaced
with a single `ProfileImage` value object.

**Reason:** The two properties always travel together — neither makes sense
without the other. A value object encapsulates them as a single domain concept.
Validation logic (`SupportedImageType` check, null checks) is defined once in
`ProfileImage.Create()` rather than duplicated on every entity that has a
profile image. Future entities (e.g. `Groomer`, `Vet`) can reuse the same
value object. EF Core maps it via `OwnsOne()` — no extra table, same columns.

---

### Decision: `PetImage` table for gallery images
**Choice:** A separate `PetImages` table with a one-to-many relationship on
`PetId`. Each row is a metadata record pointing to a blob.

**Reason:** Binary image files live in Azure Blob Storage — never in SQL Server.
SQL Server holds only lightweight metadata (`BlobName`, `ContentType`,
`DisplayOrder`). A separate table allows a pet to have multiple images with
ordering. Cascade delete ensures images are cleaned up when a pet is deleted.

---

### Decision: Maximum 6 gallery images per pet
**Choice:** `Pet.MaxImages = 6` enforced as a domain constant in `Pet.cs`.

**Reason:** Prevents unbounded image uploads. The limit is a constant rather
than a hardcoded magic number so it can be increased in one place without
touching validators, tests, or any other code. Initially set to 5, increased
to 6 before Phase 13 to give owners a bit more flexibility in showcasing their pets.

---

## C# Type Conventions

### Decision: `sealed record` for Value Objects
**Choice:** Value objects use `sealed record` instead of `class`.

**Reason:** Records provide structural equality, immutability, and `ToString()`
automatically — exactly what a value object needs. No manual `Equals()`,
`GetHashCode()`, or operator overloads required. `sealed` prevents inheritance
which is inappropriate for value objects.

---

### Decision: `record` for DTOs, Commands, and Queries
**Choice:** All DTOs, MediatR commands, and MediatR queries use `record`
instead of `class`.

**Reason:** These are immutable data carriers — once created they should not
change. Records enforce this naturally. Concise positional syntax reduces
boilerplate significantly (a DTO that was 10 lines becomes 1). Structural
equality is a bonus for testing.

---

### Decision: `class` for Domain Entities
**Choice:** Domain entities (`Owner`, `Pet`, `PetImage`, `Breed`) use `class`
not `record`.

**Reason:** Entities have identity-based equality — two `Pet` objects with the
same `Id` are the same pet regardless of their property values. Records use
structural equality which is the opposite of what entities need. Entities also
have controlled mutable state managed through domain methods — records fight
this pattern.

---

### Decision: `static Create()` factory methods on all domain entities
**Choice:** Every domain entity exposes a `static Create(...)` factory method
that constructs a fully valid instance in a single call. Handlers that create new
entities use `Create()` rather than calling `new Entity()` followed by multiple
setter calls. The setters remain on the entity for mutation (update operations).

**Reason:** Multiple setter calls leave a window where the entity is partially
constructed and invalid — a handler building an entity across five separate setter
calls could be interrupted, or a future developer extending the handler could miss
one required setter. `Create()` closes that window: the entity is either fully
valid or not created at all. `Create()` delegates to the existing setters, so all
validation logic has a single source of truth. The parameterless constructor is
retained (without modification) for EF Core to materialise entities from database
rows — this reconstruction path correctly bypasses `Create()` since the data was
already validated when it was first stored.

---

## SmartEnums

### Decision: `Ardalis.SmartEnum` for `PetType`, `DogBreed`, `CatBreed`
**Choice:** SmartEnum instead of plain C# `enum`.

**Reason:** Plain enums cannot have methods, are fragile to serialization, and
provide no built-in lookup by name or value. SmartEnum provides lookup by name
(`PetType.FromName("Dog")`), lookup by value (`PetType.FromValue(1)`), and a
full list (`PetType.List`). Behaviour can be added to the enum class itself.
Switching from SmartEnum to plain enum later is easy — the reverse is hard.

---

### Decision: `DogBreed` — top 25 AKC breeds plus designer crossbreeds
**Choice:** Top 25 AKC registered breeds (2025 rankings) plus `Labradoodle`,
`Goldendoodle`, `Cockapoo`, `Mixed`, and `Other`. 30 values total.

**Reason:** AKC breeds cover registered pedigree dogs. Labradoodle, Goldendoodle,
and Cockapoo are the three most popular designer crossbreeds in the US — owners
will look for them by name and not find them if missing. `Mixed` and `Other`
are catch-alls for everything else. Adding new breeds when users request them
is a one-line change.

---

### Decision: `CatBreed` — top 25 CFA breeds plus common owner-identified types
**Choice:** Top 25 CFA registered breeds (2025 rankings) plus `DomesticShorthair`,
`Tabby`, `Mixed`, and `Other`. 29 values total.

**Reason:** Unlike dogs there is no AKC equivalent for cats — CFA (Cat Fanciers'
Association) is the governing body. `Tabby` is not an official breed — it is a
coat pattern — but cat owners very commonly identify their cat simply as "a
Tabby". Including it improves UX. `DomesticShorthair` covers the most common
mixed-breed cat type. `Mixed` and `Other` handle everything else.

---

## Libraries and Tooling

### Decision: Scalar instead of Swagger/Swashbuckle
**Choice:** `Scalar.AspNetCore` for API documentation UI. OpenAPI spec
generation uses the built-in `AddOpenApi()` from .NET 10.

**Reason:** Scalar has a significantly better UI than Swagger UI. .NET 10 has
built-in OpenAPI spec generation so Swashbuckle is no longer needed for the
spec itself. Scalar is a drop-in replacement for the UI layer only.

---

### Decision: Manual mapping extension methods instead of AutoMapper
**Choice:** Static extension methods in `*Mappings.cs` files co-located with
each feature. No AutoMapper.

**Reason:** AutoMapper errors are runtime errors — a missing mapping silently
returns null or throws at runtime rather than failing at compile time. Manual
mapping is explicit, debuggable (F12 goes straight to the mapping code), and
safe to refactor (the compiler catches broken mappings immediately). No NuGet
dependency. Slightly more code but significantly safer and more readable.

---

### Decision: NSubstitute instead of Moq
**Choice:** NSubstitute for all mocking in tests.

**Reason:** Moq fell out of favour partly due to the SponsorLink controversy
and partly because NSubstitute has cleaner, more readable syntax. NSubstitute
requires no `Mock<T>` wrapper — `Substitute.For<T>()` returns the type directly.
Setup and verification syntax is more natural and less ceremonial than Moq.

---

### Decision: Shouldly instead of FluentAssertions
**Choice:** Shouldly for all assertions in tests.

**Reason:** Shouldly has excellent failure messages that tell you exactly what
went wrong and what value was received. Syntax is clean and reads naturally
(`result.ShouldBe("Buddy")`). FluentAssertions introduced licensing changes
that caused concern in the community. Shouldly is a straightforward replacement
with no such concerns.

---

### Decision: .NET Aspire for local dev orchestration
**Choice:** `Barkfest.AppHost` and `Barkfest.ServiceDefaults` projects added to the solution.
Aspire orchestrates SQL Server and Azurite containers with persistent named volumes. No `azd`
or Azure deployment is included.

**Reason:** Any developer can clone the repo and run `dotnet run --project src/Barkfest.AppHost`
to have a fully working local environment in under 2 minutes. Aspire handles container creation,
connection string injection, telemetry, and health checks automatically. Persistent containers
(`ContainerLifetime.Persistent`) with explicit named volumes (`barkfest-sql-data`,
`barkfest-blobs-data`) mean data survives AppHost restarts and `docker rm`. Names are
project-scoped to prevent collisions when a developer has multiple Aspire solutions running
simultaneously. User Secrets are not used — Aspire injects all connection strings at runtime.

---

### Decision: Testcontainers for all integration tests
**Choice:** `Testcontainers.MsSql` for SQL Server and `Testcontainers.Azurite`
for Azure Blob Storage in all integration and API tests.

**Reason:** No manual setup, no shared test databases, no dependency on a local
SQL Server or Azure Storage instance. Each test run gets a clean isolated
container spun up automatically. Tests are fully repeatable on any machine and
in CI/CD pipelines. `DatabaseFixture` and `AzuriteFixture` handle container
lifecycle via `IAsyncLifetime`.

---

### Decision: React Router v7 in library mode
**Choice:** React Router v7 (`react-router-dom@^7`) used in library mode — `<BrowserRouter>`,
`<Routes>`, `<Route>`, `<Outlet>`. Framework mode (file-based routing, SSR, loaders/actions)
is not used.

**Reason:** v7 in library mode is API-compatible with v6 for the patterns we use.
The upgrade was made before any routing logic was written, keeping the cost near zero.
Starting on v7 avoids a future migration once screens are built. Framework mode was
explicitly rejected — it brings Remix-style complexity (file-based routing, server
functions, adapters) that is not appropriate for a single-page app backed by a
separate .NET API.

---

### Decision: Vitest + React Testing Library for frontend unit tests
**Choice:** Vitest as the test runner and React Testing Library (RTL) for component
testing in `barkfest-ui`. No Playwright or Cypress at this stage.

**Reason:** Vitest is the natural companion to Vite — it reuses `vite.config.ts`
directly and shares the same transform pipeline, so TypeScript, path aliases, and
plugins all work without additional configuration. The Jest-compatible API means
documentation and community answers for Jest apply directly. React Testing Library
tests components through the DOM the way a user would interact with them, rather
than testing implementation details. End-to-end tests (Playwright) are not included
yet — they require a running API and add significant setup overhead that is not
justified until the UI screens are built. Vitest + RTL covers the high-value layer
(component behaviour, hooks, utility functions) with minimal friction.

**Scripts:**
- `pnpm test` — single run, exits 0 when all tests pass (CI-safe, used before commits)
- `pnpm test:watch` — interactive watch mode for development
- `pnpm test:ui` — Vitest browser UI for debugging test results

---

### Decision: `Microsoft.OpenApi` pinned to 2.x — do not upgrade to 3.x
**Choice:** `Microsoft.OpenApi` is pinned to the latest 2.x release (`2.7.4`) in
`Directory.Packages.props`. Upgrading to 3.x is blocked until `Microsoft.AspNetCore.OpenApi`
ships a compatible version.

**Reason:** `Microsoft.OpenApi` 3.0 made `IOpenApiMediaType.Example` read-only. The
`Microsoft.AspNetCore.OpenApi` source generator (10.0.8) assigns to that property in
auto-generated code (`OpenApiXmlCommentSupport.generated.cs`), producing a `CS0200`
build error. This is a Microsoft tooling gap — the two packages are published by the
same team but their versions are not yet aligned. A warning comment in
`Directory.Packages.props` documents this constraint. ROADMAP item 3 tracks when to
revisit.

---

### Decision: Azure Monitor OpenTelemetry exporter for Application Insights
**Choice:** `Azure.Monitor.OpenTelemetry.AspNetCore` added to `Barkfest.ServiceDefaults`.
The exporter is conditionally activated via `UseAzureMonitor()` when
`APPLICATIONINSIGHTS_CONNECTION_STRING` is present. When the key is absent (local dev),
the block is skipped and the Aspire dashboard receives telemetry via the OTLP exporter instead.

**Why the OpenTelemetry exporter, not the Serilog sink:**
Aspire already configures a full OpenTelemetry pipeline in `ServiceDefaults` — logs, traces,
and metrics are all flowing through it. Plugging `UseAzureMonitor()` into that pipeline sends
everything (structured logs, distributed traces, HTTP dependency calls, custom metrics) to
Application Insights with a single call. The Serilog `ApplicationInsights` sink only captures
log events — it has no visibility into traces or metrics. The OpenTelemetry approach is also the
direction Microsoft is standardising on for Azure Monitor going forward.

**Why conditional on connection string presence:**
Activating Azure Monitor unconditionally would break local dev (no connection string available)
and integration tests (containers have no Azure connectivity). Checking
`APPLICATIONINSIGHTS_CONNECTION_STRING` at startup means the exporter self-activates in any
environment where it is configured and stays silent everywhere else. No code changes required
when deploying to Azure — just set the environment variable.

**How to activate in production:**
Set `APPLICATIONINSIGHTS_CONNECTION_STRING` as an environment variable in Azure App Service
(Settings → Environment variables) or via Key Vault reference. Never commit this value to source
control or `appsettings.json`.

---

### Decision: pnpm as the frontend package manager
**Choice:** pnpm is used for all Node.js dependency management in `barkfest-ui`
instead of npm or yarn.

**Reason:** Four factors make pnpm the right fit for this project:

1. **Monorepo-ready** — pnpm has first-class workspace support. If `barkfest-ui`
   is later joined by additional frontend projects in the same repo, pnpm workspaces
   handle shared utilities and types without extra tooling.
2. **Strict dependency isolation** — pnpm's non-flat `node_modules` structure
   prevents phantom dependency access (using a package you haven't declared). The
   `package.json` always reflects what the project actually depends on.
3. **Disk efficiency** — pnpm uses a content-addressable global store with hard
   links rather than copying packages per project. On a machine with multiple Node
   projects, disk savings are significant and subsequent installs are faster.
4. **Tooling compatibility** — Vite, shadcn/ui, and Tailwind all support pnpm
   fully and their documentation includes pnpm examples.

Install once globally before working on the frontend:
```bash
npm install -g pnpm
```

---

### Decision: `Startup/` folder pattern for `Barkfest.API`
**Choice:** `Program.cs` is reduced to ~14 lines. All service registration, pipeline
configuration, and database initialisation are extracted into three static extension
method classes in `src/Barkfest.API/Startup/`:

| Class | Extension method | Responsibility |
|---|---|---|
| `ServiceRegistration` | `AddBarkfestServices(this WebApplicationBuilder)` | All `builder.*` wiring: Serilog, controllers, CORS, OpenAPI, Application/Persistence/Infrastructure DI, JWT auth, CurrentUserService |
| `DatabaseInitializer` | `InitialiseDatabaseAsync(this WebApplication)` | `MigrateAsync()` and `SeedAdminAsync` — skipped in Testing environment |
| `PipelineConfiguration` | `ConfigurePipeline(this WebApplication)` | Middleware order and all route/endpoint mapping |

**`WebApplicationBuilder` as the extension target (not `IServiceCollection`):**
`AddBarkfestServices` extends `WebApplicationBuilder` rather than `IServiceCollection`
because it needs access to both `.Services` and `.Configuration` in one place, and
because `IHostApplicationBuilder` (which `WebApplicationBuilder` implements) is required
for Aspire's `AddInfrastructure()` call. An `IServiceCollection` extension cannot satisfy
this requirement without being passed a second `IConfiguration` parameter — making the
signature unwieldy. `WebApplicationBuilder` covers both needs with a single clean extension.

**Reason:** `Program.cs` with ~160 lines of mixed service registration, middleware, and
initialisation becomes hard to scan and impossible to test in isolation. Each Startup class
has a single clearly-stated responsibility. A developer looking for middleware order goes to
`PipelineConfiguration`. A developer looking for where JWT is configured goes to
`ServiceRegistration`. Database startup logic is isolated in `DatabaseInitializer` and
explicitly skipped in the Testing environment. `Program.cs` itself becomes a readable
executive summary of the startup sequence.

---

### Decision: Serilog for logging
**Choice:** Serilog configured in `Startup/ServiceRegistration.cs` via `builder.Host.UseSerilog()`.

**Reason:** Serilog is the de facto standard for structured logging in .NET.
Structured logs are queryable in tools like Seq, Application Insights, and
Datadog. Plain `ILogger` text logging is not queryable at scale.

---

## Image Handling

### Decision: Images stored in Azure Blob Storage, metadata in SQL Server
**Choice:** Binary image files live exclusively in Azure Blob Storage. SQL Server
stores only `BlobName` and `ContentType`.

**Reason:** Storing binary files in SQL Server causes significant performance
degradation and database bloat. Blob Storage is purpose-built for binary files —
cheap, scalable, and CDN-friendly. `BlobName` is the key used to retrieve the
file. SQL Server never touches the binary data.

---

### Decision: Supported image types — jpeg, jpg, png only
**Choice:** `SupportedImageType` static class defines allowed content types
(`image/jpeg`, `image/jpg`, `image/png`) and extensions (`.jpeg`, `.jpg`,
`.png`). Applied to all image uploads across the entire application.

**Reason:** Restricting to common web-safe formats prevents uploads of
unsupported or potentially dangerous file types. The rule is defined once in
`SupportedImageType` in the Domain and reused by every entity and validator —
no duplication, no risk of the allowed list drifting between Owner and Pet
image uploads. When a new pet type (e.g. Horse) is added, the same rule applies
automatically.

---

### Decision: Image validation at two layers
**Choice:** Image type validation is enforced at both the Domain layer (entity
methods and `ProfileImage.Create()`) and the Application layer (FluentValidation
validators).

**Reason:** Application validation fast-fails at the boundary before hitting
the domain or Blob Storage, returning a clean 400 response. Domain validation
is the safety net — the entity can never be put into an invalid state regardless
of how it is called (e.g. directly in tests or a future CLI tool).

---

## Implementation Discoveries

### Decision: `PetRepository.UpdateAsync` — disable `AutoDetectChanges` around state snapshot
**Choice:** `AutoDetectChangesEnabled` is set to `false` before inspecting entity state and
restored to `true` in a `finally` block. New `PetImage` entities are identified as detached
before calling `context.Pets.Update(pet)`, then their state is explicitly set to `Added`.

**Reason:** EF Core's `AutoDetectChanges` fires automatically when `context.ChangeTracker.Entries<T>()`
or `context.Entry(entity)` is called. This caused new `PetImage` entities (created with
`Guid.CreateVersion7()`) to be snapshotted and tracked as `Modified` before the state check
could run. Because their GUID keys are non-empty, EF Core treated them as existing rows and
issued an `UPDATE` instead of an `INSERT`, throwing `DbUpdateConcurrencyException`. Disabling
`AutoDetectChanges` during the critical section prevents the premature snapshot, allowing the
`Detached` → `Added` state assignment to work correctly.

---

### Decision: EF Core configuration tests use a shared `ModelHelper`
**Choice:** A static `ModelHelper` class builds the EF Core model once using the SQL Server
provider and caches it in a `Lazy<IModel>`. All configuration test classes read from this
shared model.

**Reason:** Building the model is expensive and requires a valid provider (SQL Server, not
in-memory) to reflect real column names, types, and constraints. No live database connection
is needed — `OnModelCreating` runs entirely during model construction. A shared `Lazy<IModel>`
means the model is built once per test run regardless of how many test classes use it. Using
the in-memory provider would silently hide SQL Server-specific configuration (e.g. `nvarchar`
lengths, `newsequentialid()` defaults).

---

### Decision: Validator tests use concrete `AbstractValidator<T>` subclasses, not mocks
**Choice:** Test-specific validator classes that extend `AbstractValidator<T>` are defined
directly in the test file. `IValidator<T>` is never mocked with NSubstitute.

**Reason:** FluentValidation is strong-named. Castle DynamicProxy (used by NSubstitute
internally) cannot proxy `IValidator<T>` when `T` is a private or nested type against a
strong-named assembly — it throws a `TypeLoadException` at runtime. Using concrete
`AbstractValidator<T>` subclasses sidesteps this entirely and produces clearer tests since
the validation rules being tested are explicit in the test file.

---

### Decision: `ValidationBehavior` tests use real delegates, not NSubstitute substitutes
**Choice:** `RequestHandlerDelegate<TResponse>` is implemented as a real lambda with a
closure-based call counter rather than being mocked with NSubstitute.

**Reason:** NSubstitute cannot mock delegate types — it only proxies interfaces and virtual
class members. Attempting to `Substitute.For<RequestHandlerDelegate<TResponse>>()` throws
at runtime. A real lambda (`() => { callCount++; return Task.FromResult(response); }`)
is simpler, more readable, and does not require any workaround.

---

### Decision: FluentValidation `EmailAddress()` does not reject spaces in the local part
**Choice:** The test case `"space in@example.com"` is not included in the invalid email
theory data for validator tests.

**Reason:** FluentValidation's built-in `EmailAddress()` validator only checks for the
presence of `@` and a dot in the domain — it does not reject spaces in the local part of
the address. This is by design in FluentValidation. Adding a custom regex to reject spaces
was considered but rejected as over-engineering for a dev learning project. If stricter
email validation is needed in future, replace `EmailAddress()` with
`Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")` or a dedicated email validation library.

---

### Decision: Use `AddDbContext` (not `AddSqlServerDbContext`) in Persistence
**Choice:** `Barkfest.Persistence/DependencyInjection.cs` uses standard EF Core `AddDbContext<AppDbContext>` with
`configuration.GetConnectionString("barkfest-sql")`. The Aspire integration package
`Aspire.Microsoft.EntityFrameworkCore.SqlServer` is kept in the project file for future health-check use
but `AddSqlServerDbContext` is not called at startup.

**Reason:** Two interlocking problems made `AddSqlServerDbContext` untestable with `WebApplicationFactory`:

1. **`ConfigureAppConfiguration` priority order** — in .NET 10's minimal hosting model
(`WebApplicationBuilder`), callbacks registered via `IWebHostBuilder.ConfigureAppConfiguration` in
`WebApplicationFactory.ConfigureWebHost` are added *before* the default configuration sources
(including `appsettings.json`). Any key present in `appsettings.json` therefore silently overwrites
the in-memory test values. The injection appears to work but the test value never reaches `IConfiguration`.

2. **Environment variable race condition** — using `Environment.SetEnvironmentVariable` before host
build works in isolation but fails when xUnit runs test classes in parallel (the default): two
`BarkfestApiFactory` instances race to set the same global env var names and one factory's host is
built with the other factory's container connection strings.

Standard `AddDbContext` avoids both issues because the factory can replace
`DbContextOptions<AppDbContext>` directly via `services.RemoveAll<T>()` — this is entirely
in-process, per-instance, and unaffected by configuration source priority or global state.
In production, the Aspire AppHost injects `ConnectionStrings__barkfest-sql` as an environment
variable which ASP.NET Core's built-in env-var provider picks up at startup — no Aspire-specific
extension method is required for that to work.

---

## Documentation Structure

### Decision: Feature-scoped docs under `docs/features/` after the initial build
**Choice:** After `feature/initial-build` merges into `main`, each new feature gets its
own branch (`feature/<name>`) and its own docs folder (`docs/features/<name>/`) containing
three files: `PLAN.md`, `PROGRESS.md`, and `DECISIONS.md`.

The root-level `PLAN.md`, `PROGRESS.md`, and `DECISIONS.md` become historical records of
the initial build — they are not updated for new features. The root files that remain
actively maintained are `CLAUDE.md`, `README.md`, `ROADMAP.md`, and `SPEC.md`.

**Reason:** The root docs grew large during the initial build and primarily document
decisions and progress from phases 1–12. Continuing to append new feature work to them
would make them unwieldy and hard to navigate. Feature-scoped docs mean a developer
working on a feature can read just the three files in that feature's folder to get full
context, without wading through the entire initial build history. Each feature PR has a
clean, self-contained paper trail. The root docs serve as a stable foundation reference —
they do not grow indefinitely.

---

## Git and Workflow

### Decision: Branch protection on `main`, PRs only
**Choice:** GitHub ruleset prevents direct commits to `main`. All changes
must go through an approved PR with squash and merge.

**Reason:** Keeps `main` always releasable. Squash and merge produces a clean
linear history — one PR equals one commit on `main`. Code review enforced
for all changes regardless of author.

---

### Decision: Never use `git add .` or `git add -A`
**Choice:** All staging must name specific files explicitly. Claude Code must
show the user which files will be staged and wait for approval before staging.

**Reason:** `git add .` silently stages unintended files — secrets, build
artifacts, temporary files. Explicit staging with user approval ensures only
intended files enter the commit. This is a non-negotiable safety rule.

---

## Testing

### Decision: Shared test data builders in `Barkfest.Tests.Common`
**Choice:** Fluent builder classes (`OwnerBuilder`, `PetBuilder`, `PetImageBuilder`) live in a
shared `Barkfest.Tests.Common` project referenced by `Barkfest.Domain.Tests` and
`Barkfest.Application.Tests`. Builders are available globally via `GlobalUsings.cs` in each
test project. Private `BuildXxx()` helper methods in individual test classes are banned.

**Reason:** `Domain.Tests` and `Application.Tests` both build the same domain entities. Without
a shared project, `OwnerBuilder` and `PetBuilder` would be duplicated — any change to a domain
entity would require updating builders in two places. A single shared project eliminates that
duplication. Banning private helpers enforces consistency — every test file uses the same
defaults and the same fluent API, making tests easier to read and maintain. The one exception
is bare entity construction (`new Pet(Guid.NewGuid())`) in `Domain.Tests` setter tests where
builder defaults would mask the behaviour under test.

---

### Decision: Test naming convention — `[Method]_When_[Condition]_Returns/Throws_[Result]`
**Choice:** All test methods follow a structured naming pattern:
- Happy path / response tests: `[Method]_When_[Condition]_Returns_[Result]`
- Exception tests: `[Method]_When_[Condition]_Throws_[ExceptionType]`
- Validator failure tests: `Fails_For[Property]_When_[Condition]`
- HTTP status codes written as words: `Ok`, `Created`, `NoContent`, `BadRequest`, `NotFound`, `InternalServerError`
- Scenario lifecycle tests (e.g. `OwnerCrudLifecycle_*`) are exempt — they describe an end-to-end flow, not a single method

**Reason:** Consistent naming makes the intent of every test immediately clear without reading
the body. The `When_` clause is required — it forces the author to articulate the condition
being tested rather than writing vague names like `Should_ReturnOwner`. Writing HTTP status
codes as words (`NotFound` not `404`) keeps test names readable and decoupled from
implementation details. The pattern applies uniformly across all six test projects so any
developer can navigate the test suite without learning different conventions per layer.

---

### Decision: `dotnet test` must pass before any commit
**Choice:** Claude Code runs `dotnet test` before every `git commit` and waits
for all tests to pass. Staging does not require tests to pass.

**Reason:** Prevents broken code from ever landing in a commit — even locally.
Staging is a work-in-progress operation and does not need a test gate. Committing
is a statement that work is complete and verified — tests must confirm this.

---

## Authentication and Identity

### Decision: Administrators and Owners are completely separate identities
**Choice:** `Administrator` and `Owner` are distinct entity classes, stored in separate
tables (`Administrators` and `Owners`), with separate repositories (`IAdministratorRepository`,
`IOwnerRepository`), separate login endpoints (`/v1/auth/admin/login` and `/v1/auth/login`),
and separate JWT claims (`account_type: "admin"` vs `account_type: "owner"`).

**Reason:** Administrators manage the platform — they activate/deactivate owners and manage
other admin accounts. Owners register pets and manage their own data. Conflating these two
roles into a single identity (e.g. an `IsAdmin` flag on `Owner`) would create awkward coupling:
admin-specific properties would appear on owner DTOs, ownership checks would need to handle
admin edge cases everywhere, and an administrator logging in as an owner would have access to
owner-specific endpoints in unexpected ways. Separate entities mean each identity has exactly
the properties it needs, the JWT `sub` claim always refers to the correct entity, and the
authorization logic in every handler is unambiguous. `ICurrentUserService` exposes `OwnerId`,
`AdminId`, and `IsAdmin` — handlers read the correct property for their context.

---

### Decision: Administrator self-delete is forbidden
**Choice:** `DeleteAdministratorCommandHandler` throws `ForbiddenException` when
`request.Id == currentUserService.AdminId`. The check is unconditional — no override path exists.

**Reason:** Preventing self-deletion ensures there is always at least one administrator account
in the system. If an admin could delete themselves, a single-admin deployment could become
permanently locked out with no recovery path short of direct database manipulation. Trust is
the only other variable — any admin can delete any other admin — so the self-delete guard is
the one hard rule that protects against accidental or malicious lockout.

---

### Decision: Administrator management trust model — any admin can manage any other admin
**Choice:** Any authenticated administrator can create new administrators, update another
administrator's password, and delete another administrator (but not themselves).

**Reason:** A more restrictive model (e.g. super-admin role, owner hierarchy, approval workflow)
adds complexity that is not warranted for a small trusted team. The self-delete guard is the
only hard constraint. Production deployments should create a second administrator account
immediately after first login so that access is never dependent on a single set of credentials.
If an admin account is compromised, any other admin can delete it. This is an intentional
design choice — trust is a prerequisite for the team using this system.

---

### Decision: E.164 format enforced for all phone numbers
**Choice:** Phone numbers for `Owner` and `Administrator` must be in E.164 format
(e.g. `+15555550100`) validated by `E164PhoneNumber.IsValid()`. Free-form strings
(e.g. `555-0100`, `(555) 555-0100`) are rejected.

**Reason:** Free-form phone numbers are a persistent data quality problem — the same
number can be stored in dozens of different formats making deduplication, lookup, and
programmatic dialling impossible without normalisation. E.164 is the ITU-T international
standard: `+` followed by country code and up to 14 digits, no spaces, no dashes, no
parentheses. Enforcing the canonical format at the domain boundary means the stored value
is always dial-ready, always unambiguous, and never needs cleanup. Validation is a compile-time-
checked regex defined once in `E164PhoneNumber` — the same rule applies to every entity that
has a phone number field.

---

### Decision: Shared domain constants in `ValueObjects/` — `E164PhoneNumber` and `AccountConstraints`
**Choice:** E.164 pattern, max length, and `IsValid()` method live in
`Barkfest.Domain/ValueObjects/E164PhoneNumber.cs`. Email and username max lengths live in
`Barkfest.Domain/ValueObjects/AccountConstraints.cs`. Neither `Owner` nor `Administrator`
defines its own copies of these values.

**Reason:** Both `Owner` and `Administrator` independently defined the same E.164 regex,
the same max length constant, and the same `UsernameMaxLength`/`EmailMaxLength` values.
Duplication is a maintenance hazard — changing the regex or a limit requires finding and
updating every copy. The `ValueObjects/` folder already held `SupportedImageType` (shared
image validation rules) and `ProfileImage` (shared structure) — this pattern was already
established. Naming required care: `PhoneNumber` would conflict with the `PhoneNumber`
property inside setter methods (`PhoneNumber.IsValid()` would resolve to the string property,
not the static class). `E164PhoneNumber` is explicit about the format it represents and has
no naming conflicts. Similarly, `AccountConstraints` avoids the `Username` and `Email`
conflict while clearly signalling that these constants govern account-level field rules.

---

### Decision: Content moderation scaffolded as a NoOp — implement after Azure deployment
**Choice:** `IContentModerationService` is defined in the Application layer with a single
`IsImageSafeAsync` method. `NoOpContentModerationService` (Infrastructure) always returns
`true`. All image upload handlers call the service but production enforcement is deferred
until Azure AI Content Safety is provisioned.

**Reason:** Wiring the interface into every image upload handler now means the integration
point is tested, the dependency injection path is proven, and future activation requires only
swapping one registration in `DependencyInjection.cs`. The alternative — adding the service
later — risks forgetting injection points, requires touching handler code post-deployment, and
creates a gap where images are uploaded without any moderation path in the code at all. The
NoOp pattern makes the intent explicit: content moderation is a known requirement, not an
afterthought. The detailed TODO comment in `NoOpContentModerationService` documents the exact
steps to activate Azure AI Content Safety (the successor to the deprecated Azure Content
Moderator), including the NuGet package and configuration steps required.

---

### Decision: HttpOnly cookies for frontend auth token storage
**Choice:** The JWT issued on login is stored in an HttpOnly cookie set by the
server, not in `localStorage` or `sessionStorage` on the client.

**Reason:** `localStorage` and `sessionStorage` are accessible to any JavaScript
running on the page — an XSS vulnerability anywhere in the frontend can silently
exfiltrate the token. An HttpOnly cookie cannot be read by JavaScript at all;
the browser attaches it to requests automatically and it is invisible to client-side
code. Combined with `Secure` (HTTPS only) and `SameSite=Strict`, this is the
correct default for any application handling authenticated sessions.

**What this requires:**
- .NET API: login endpoint sets the cookie via `Response.Cookies.Append()` with
  `HttpOnly = true`, `Secure = true`, `SameSite = Strict` instead of returning
  the token in the response body
- .NET API: new `POST /v1/auth/logout` endpoint that clears the cookie
- .NET API: CORS configured with `AllowCredentials()` and an explicit allowed
  origin for the frontend dev server
- Frontend: all API calls include `credentials: 'include'` so the browser sends
  the cookie cross-origin
- Frontend auth context: tracks `isAuthenticated` (boolean) — no token ever
  touches client-side storage

**Implementation split:**
- Phase 12: CORS configuration (needed for any frontend → API communication)
- Phase 13: HttpOnly cookie login/logout + frontend auth context

---

### Decision: `GET /v1/owners` and `GET /v1/admin/admins` are admin-only
**Choice:** Both list endpoints are guarded by `ICurrentUserService.IsAdmin` in the handler,
throwing `ForbiddenException` for non-admin callers. Regular owners cannot enumerate all owner
accounts or any administrator accounts.

**Reason:** Listing all owners exposes PII (usernames, emails, phone numbers) across the entire
user base — a capability that belongs to platform administrators, not individual owners. Allowing
any authenticated owner to enumerate all accounts would be a significant privacy and security
concern. Similarly, listing all administrator accounts should only be visible to other
administrators. Enforcing the check in the handler (Application layer) rather than only at the
route level ensures the rule is testable in isolation and cannot be bypassed by adding a new
controller endpoint that accidentally omits the attribute.
