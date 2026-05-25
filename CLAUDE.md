# CLAUDE.md — Barkfest

This file is read by Claude Code at the start of every session. Follow every rule
defined here without exception. These conventions were deliberately chosen — do not
substitute alternatives, even if they seem equivalent.

---

## Solution Overview

**Barkfest** is a .NET 10 Clean Architecture pet management application.
Owners register themselves and their pets. All relational data lives in SQL Server
via EF Core. All images (binary files) are stored in Azure Blob Storage — SQL Server
holds only metadata (blob name + content type).

---

## Target Framework

- `.NET 10` for all projects — no exceptions

---

## Architecture

Clean Architecture with five layers, plus two Aspire orchestration projects (`AppHost`, `ServiceDefaults`):

```
Barkfest.API  →  Barkfest.Application  →  Barkfest.Domain
                         ↑                        ↑
              Barkfest.Persistence ───────────────┘
              Barkfest.Infrastructure ─────────────┘
```

- `Domain` has zero external dependencies
- `Application` references only `Domain`
- `Persistence` and `Infrastructure` reference `Domain` and `Application`
- `API` references `Application`, `Persistence`, `Infrastructure`, and `ServiceDefaults`
- `AppHost` references `API` only (as an Aspire IProjectResource)
- Dependency direction always points inward — never outward

---

## Project Structure

```
Barkfest.sln
├── src/
│   ├── Barkfest.AppHost
│   ├── Barkfest.ServiceDefaults
│   ├── Barkfest.Domain
│   ├── Barkfest.Application
│   ├── Barkfest.Persistence
│   ├── Barkfest.Infrastructure
│   └── Barkfest.API
└── tests/
    ├── Barkfest.Tests.Common
    ├── Barkfest.Domain.Tests
    ├── Barkfest.Application.Tests
    ├── Barkfest.Persistence.Tests
    ├── Barkfest.Infrastructure.Tests
    ├── Barkfest.API.Tests
    └── Barkfest.Integration.Tests
```

---

## C# Type Conventions

This is one of the most important rules in this file. The choice between `class`,
`record`, and `sealed record` is deliberate and must be followed consistently.

| What | Type | Reason |
|---|---|---|
| Domain Entities (`Owner`, `Pet`, `PetImage`, `Breed`) | `class` | Mutable state, identity-based equality |
| Value Objects (`ProfileImage`) | `sealed record` | Immutable, structural equality, no boilerplate |
| DTOs (`OwnerDto`, `PetDto`, `PetImageDto`, `ProfileImageDto`) | `record` | Immutable data carriers |
| MediatR Commands (`CreateOwnerCommand` etc.) | `record` | Immutable, concise syntax |
| MediatR Queries (`GetOwnerByIdQuery` etc.) | `record` | Immutable, concise syntax |
| Handlers, Validators, Repositories, Services | `class` | Behaviour, dependencies, mutable state |
| EF Core Configurations, DbContext | `class` | Infrastructure concerns |

### Examples

```csharp
// Value Object — sealed record with private constructor and static factory
public sealed record ProfileImage
{
    public string BlobName { get; }
    public string ContentType { get; }
    // No manual Equals/GetHashCode — record provides it
    private ProfileImage(string blobName, string contentType) { ... }
    public static ProfileImage Create(string blobName, string contentType) { ... }
}

// DTO — record
public record OwnerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    ProfileImageDto? ProfileImage,
    DateTime CreatedAt);

// Command — record
public record CreateOwnerCommand(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber) : IRequest<Guid>;

// Query — record
public record GetOwnerByIdQuery(Guid Id) : IRequest<OwnerDto>;

// Entity — class with static Create() factory
public class Owner
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    // ...

    // All domain entities expose a static Create() factory that constructs a
    // fully valid instance in one call. Handlers use Create() — never new Entity()
    // followed by individual setter calls.
    public static Owner Create(string username, string firstName, ...) { ... }
}
```

### Entity factory method rule

All domain entities expose a `static Create(...)` factory method. Handlers that
create new entity instances must use `Create()` — not `new Entity()` with separate
setter calls. The setters remain as the validation layer for *mutation* (update
handlers). `Create()` delegates to the setters, so all validation has a single
source of truth.

The one exception is EF Core reconstruction: the parameterless constructor stays
accessible so EF Core can materialise entities from database rows.

---

## Primary Keys

- All primary keys are `Guid`
- Initialised application-side using `Guid.CreateVersion7()` (sequential, no index fragmentation)
- SQL Server column default set to `newsequentialid()` as a safety net
- Never use `int` auto-increment keys

---

## Database Column Naming

All primary key `Id` properties **must** map to `{EntityName}Id` in the database
using `HasColumnName()` in EF Core configuration.

| Entity | C# Property | DB Column |
|---|---|---|
| `Owner` | `Id` | `OwnerId` |
| `Pet` | `Id` | `PetId` |
| `PetImage` | `Id` | `PetImageId` |
| `Breed` | `Id` | `BreedId` |

**Why:** Raw SQL queries and Dapper queries become self-describing in joins.

```sql
-- Good — immediately clear which Id belongs to which table
SELECT o.OwnerId, p.PetId, pi.PetImageId
FROM Owners o
INNER JOIN Pets p       ON o.OwnerId = p.OwnerId
LEFT JOIN  PetImages pi ON p.PetId   = pi.PetId

-- Bad — ambiguous without aliases
SELECT o.Id, p.Id, pi.Id
FROM Owners o
INNER JOIN Pets p       ON o.Id = p.OwnerId
LEFT JOIN  PetImages pi ON p.Id = pi.PetId
```

---

## Dependency Injection

Each `src` project has its own `DependencyInjection.cs` with a self-registering
extension method. The API wires everything together in `Program.cs` via two calls:

```csharp
builder.AddServiceDefaults();   // Aspire — telemetry, health checks
builder.AddBarkfestServices();  // all API-layer services — see Startup/ServiceRegistration.cs

await app.InitialiseDatabaseAsync();  // migration + admin seed — see Startup/DatabaseInitializer.cs
app.ConfigurePipeline();              // middleware + endpoints — see Startup/PipelineConfiguration.cs
```

`AddPersistence` takes `IServiceCollection` + `IConfiguration` (standard EF Core registration).
`AddInfrastructure` takes `IHostApplicationBuilder` (Aspire-aware `AddAzureBlobServiceClient`).
See DECISIONS.md — `AddSqlServerDbContext` was tried but dropped due to `WebApplicationFactory`
configuration injection limitations in .NET 10's minimal hosting model.

Never register services from one layer inside another layer's `DependencyInjection.cs`.

### API Startup Folder

`Barkfest.API/Startup/` contains three static classes that keep `Program.cs` minimal:

| File | Extension method | Responsibility |
|---|---|---|
| `ServiceRegistration.cs` | `AddBarkfestServices(this WebApplicationBuilder)` | Serilog, controllers, CORS, OpenAPI, Application/Persistence/Infrastructure DI, JWT auth |
| `DatabaseInitializer.cs` | `InitialiseDatabaseAsync(this WebApplication)` | `MigrateAsync()` + `SeedAdminAsync` (skipped in Testing environment) |
| `PipelineConfiguration.cs` | `ConfigurePipeline(this WebApplication)` | Middleware order, route mapping |

`AddBarkfestServices` extends `WebApplicationBuilder` (not `IServiceCollection`) because it
needs access to both `.Services` and `.Configuration`, and satisfies `IHostApplicationBuilder`
for Aspire's `AddInfrastructure()`. See DECISIONS.md for the full reasoning.

---

## Object Mapping

**No AutoMapper.** All mapping is done via manual static extension methods
co-located with the feature they serve.

```csharp
// Barkfest.Application/Features/Owners/OwnerMappings.cs
public static class OwnerMappings
{
    public static OwnerDto ToDto(this Owner owner) =>
        new(owner.Id, owner.FirstName, ...);

    public static IEnumerable<OwnerDto> ToDtoList(this IEnumerable<Owner> owners) =>
        owners.Select(o => o.ToDto());
}
```

---

## MediatR

- Commands and queries implement `IRequest<TResponse>`
- Handlers implement `IRequestHandler<TRequest, TResponse>`
- Pipeline behaviours: `ValidationBehavior` (runs first), `LoggingBehavior`
- Commands that return nothing use `IRequest` (not `IRequest<Unit>`)
- Commands that create a resource return `IRequest<Guid>` (the new entity Id)
- **The handler class is always defined in the same file as its command or query — never in a separate `*CommandHandler.cs` or `*QueryHandler.cs` file.** The record and its handler live together in `CreateOwnerCommand.cs`, `LoginCommand.cs`, etc.

---

## Validation

**No manual validation in handlers.** All validation uses FluentValidation
`AbstractValidator<T>` and is executed automatically by `ValidationBehavior`.

```csharp
public class CreateOwnerCommandValidator : AbstractValidator<CreateOwnerCommand>
{
    public CreateOwnerCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(Owner.EmailMaxLength)
            .EmailAddress().WithMessage("Email must be a valid email address.");
    }
}
```

**Always reference domain constants in validators — never hardcode numbers:**

```csharp
// Correct
.MaximumLength(Owner.FirstNameMaxLength)

// Wrong
.MaximumLength(50)
```

**Note:** Email validation uses `Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")` rather than
FluentValidation's built-in `EmailAddress()`. The built-in validator does not reject
spaces in the local part (e.g. `"space in@example.com"` would pass it). The regex
correctly rejects such addresses.

---

## Domain Constants

These constants are defined on the entity and must be used everywhere —
validators, tests, EF Core configuration.

| Constant | Value | Location |
|---|---|---|
| `AccountConstraints.UsernameMaxLength` | 50 | `ValueObjects/AccountConstraints.cs` |
| `AccountConstraints.EmailMaxLength` | 75 | `ValueObjects/AccountConstraints.cs` |
| `E164PhoneNumber.MaxLength` | 25 | `ValueObjects/E164PhoneNumber.cs` |
| `Owner.FirstNameMaxLength` | 50 | `Owner.cs` |
| `Owner.LastNameMaxLength` | 100 | `Owner.cs` |
| `Administrator.NameMaxLength` | 100 | `Administrator.cs` |
| `Pet.NameMaxLength` | 75 | `Pet.cs` |
| `Pet.MaxImages` | 6 | `Pet.cs` |
| `PetImage.BlobNameMaxLength` | 500 | `PetImage.cs` |
| `PetImage.ContentTypeMaxLength` | 100 | `PetImage.cs` |
| `PetImage.MaxImageSizeBytes` | 10 MB (10 × 1024 × 1024) | `PetImage.cs` |

---

## Business Rules

### Owner
- `Username` — required, max `Owner.UsernameMaxLength` chars, trimmed, case-sensitive, unique
- `FirstName` — required, max `Owner.FirstNameMaxLength` chars, trimmed
- `LastName` — required, max `Owner.LastNameMaxLength` chars, trimmed
- `Email` — required, valid email format, max `Owner.EmailMaxLength` chars, lowercased and trimmed, unique (contact only — not used for login)
- `PhoneNumber` — optional, E.164 format if provided, max `E164PhoneNumber.MaxLength` chars
- Login uses `Username` + password; `Email` is a contact field only

### Pet
- `Name` — required, max `Pet.NameMaxLength` chars, trimmed
- `Description` — optional, no max length, trimmed if provided
- `DateOfBirth` — optional `DateOnly`, cannot be in the future
- `Age` — computed from `DateOfBirth` at runtime, **never stored in the database**
- `PetType` — required SmartEnum; only `Dog` (1) and `Cat` (2) are valid values
- `Breed` — required; must match `PetType`: Dog → `DogBreedInfo`, Cat → `CatBreedInfo`; "Other" is a valid breed name within each species (same as any named breed)
- `Images` — maximum `Pet.MaxImages` (6) images total; any one can be designated `IsFeaturedImage = true`; only one may be featured at a time; the UI enforces a minimum of 1 image at creation — the API does not enforce this at the endpoint level

### Images (applies to all image uploads across the entire application)
- Allowed content types: `image/jpeg`, `image/jpg`, `image/png`
- Allowed extensions: `.jpeg`, `.jpg`, `.png`
- Maximum file size: `PetImage.MaxImageSizeBytes` (10 MB) per file — enforced in `AddPetImagesCommandValidator` and on the frontend via react-dropzone `maxSize`
- Maximum request body: 65 MB — enforced by `[RequestSizeLimit]` and `[RequestFormLimits]` on the `AddImages` action in `PetController`
- Validated by `SupportedImageType` static class in `Barkfest.Domain`
- Enforced at two layers: Domain (entity methods) and Application (FluentValidation)
- Binary files stored in Azure Blob Storage — SQL Server stores only `BlobName` and `ContentType`

### Administrator
- `Username` — required, max `AccountConstraints.UsernameMaxLength` chars, trimmed, case-sensitive, unique
- `Name` — required, max `Administrator.NameMaxLength` chars, trimmed
- `Email` — required, valid email format, max `AccountConstraints.EmailMaxLength` chars, lowercased and trimmed, unique
- `PhoneNumber` — required, E.164 format, max `E164PhoneNumber.MaxLength` chars
- `PasswordHash` — required, set via `SetPasswordHash(string hash)`
- Login uses `Username` + password
- Any administrator can create new administrators (username + name + email + phoneNumber + password)
- Any administrator can update another administrator's password
- Any administrator can delete another administrator but **never themselves** (self-delete throws `ForbiddenException`)
- Administrator accounts are fully separate from Owner accounts — different tables, different JWT claims, different identity

### Authorization
- `GET /v1/owners` — lists all owners; admin JWT required (throws `ForbiddenException` for non-admins)
- `GET /v1/admin/admins` — lists all administrators; admin JWT required (throws `ForbiddenException` for non-admins)
- All other owner and pet endpoints require a valid owner JWT; ownership enforced in handlers

### Profile Images
- `Owner` has an optional profile image represented as a `ProfileImage` value object (`sealed record`) with `BlobName` and `ContentType`
- Mapped to two nullable columns on `Owners` via EF Core `OwnsOne()`:
  `ProfileImageBlobName` nvarchar(500), `ProfileImageContentType` nvarchar(100)
- `Pet` has no separate profile image — any of its gallery images can be designated as featured via `IsFeaturedImage = true` on `PetImage`

---

## EF Core

- Use `IEntityTypeConfiguration<T>` for all entity configuration — no data annotations
- SmartEnums stored as `int` using `.HasConversion(pt => pt.Value, value => PetType.FromValue(value))`
- `Breed` uses Table Per Hierarchy (TPH) with discriminator column `BreedType` (`"Dog"` or `"Cat"`)
- `Owner.ProfileImage` value object mapped using `OwnsOne()` — no separate table
- `Pet.Age` must be ignored: `builder.Ignore(p => p.Age)`
- `Pet.FeaturedImage` must be ignored: `builder.Ignore(p => p.FeaturedImage)`
- All cascade deletes: `Owner` → `Pets`, `Pet` → `PetImages`, `Pet` → `Breeds`
- Migration applied at startup via `MigrateAsync()` — never run `dotnet ef database update`

### Migration naming

Format: `{Verb}{Subject}` in PascalCase. For compound changes use `And`.

| Verb | When to use |
|---|---|
| `Add` | New column, index, or constraint on an existing table |
| `Remove` | Drop a column, index, or constraint |
| `Create` | New table |
| `Drop` | Remove a table entirely |
| `Rename` | Rename a column or table |
| `Alter` | Change type, nullability, or length of an existing column |

```
✅ AddOwnerPasswordHash
✅ AddUniqueIndexOnOwnerEmail
✅ CreatePetImagesTable
✅ RemovePetProfileImageColumns
✅ AddPetImageIsFeaturedImage
✅ RemovePetProfileImageColumnsAndAddIsFeaturedImage

❌ UpdatePet          — says nothing about what changed
❌ FixSchema          — vague
❌ Misc / Changes     — never acceptable
```

---

## Connection Strings and Local Dev

- Connection strings are **never** committed to source control
- For local development, connection strings are injected automatically by .NET Aspire
  when running via `dotnet run --project src/Barkfest.AppHost`
- `appsettings.json` has **no** `ConnectionStrings` section — Aspire injects the connection
  strings at runtime and there are no placeholder empty values to override
- In production or CI, populate these via environment variables or a secrets manager
- User Secrets are **not used** — Aspire replaces them entirely

---

## .NET Aspire

- Run the solution locally via: `dotnet run --project src/Barkfest.AppHost`
- Aspire spins up SQL Server and Azurite containers automatically on first run
- Containers are persistent (`ContainerLifetime.Persistent`) with named volumes
  (`barkfest-sql-data`, `barkfest-blobs-data`) — data survives restarts
- Do not modify container or volume names — they are project-scoped to prevent
  collisions with other Aspire solutions on the same machine
- Docker container names will have a short hash suffix appended by Aspire (e.g. `barkfest-sql-090bc107`) — this is expected and stable per machine; volume names are not hashed
- `Barkfest.Domain.Tests`, `Barkfest.Application.Tests` — no Aspire dependency, no containers
- `Barkfest.Persistence.Tests`, `Barkfest.Infrastructure.Tests`, `Barkfest.API.Tests` — no Aspire dependency, manage their own containers via Testcontainers
- `Barkfest.Integration.Tests` — uses `WebApplicationFactory<Program>` with Testcontainers; fully self-contained, no running AppHost required

---

## API Documentation

**No Swagger/Swashbuckle.** Use Scalar.

```csharp
builder.Services.AddOpenApi();          // built-in .NET 10 OpenAPI spec generation
app.MapOpenApi();                       // serves /openapi/v1.json
app.MapScalarApiReference();            // serves /scalar/v1
```

---

## Logging

Use Serilog. Configured in `Startup/ServiceRegistration.cs` via `builder.Host.UseSerilog()`.

---

## Exception Handling

`ExceptionHandlingMiddleware` in `Barkfest.API/Middleware/` handles all exceptions:

| Exception | HTTP Response |
|---|---|
| `NotFoundException` | 404 Not Found |
| `DomainException` | 400 Bad Request |
| `ForbiddenException` | 403 Forbidden |
| Unhandled | 500 Internal Server Error |

Never add try/catch blocks in handlers or controllers — let middleware handle it.

---

## Testing

### Libraries
- **Test framework:** xUnit
- **Assertions:** Shouldly — never FluentAssertions
- **Mocking:** NSubstitute — never Moq
- **Containers (Persistence.Tests, Infrastructure.Tests, API.Tests):** Testcontainers.MsSql, Testcontainers.Azurite
- **Integration.Tests:** Testcontainers (SQL Server + Azurite) via `WebApplicationFactory<Program>` — fully self-contained

### Rules
- `Domain.Tests`, `Application.Tests` — unit tests, no I/O, no containers, no external dependencies
- `Persistence.Tests`, `Infrastructure.Tests`, `API.Tests` — Testcontainers only, no real external services
- `Barkfest.Integration.Tests` — references `Barkfest.API`; uses `WebApplicationFactory<Program>`
  with Testcontainers (SQL Server + Azurite); fully self-contained, no running AppHost required
- All image limit tests reference `Pet.MaxImages` — never hardcode `5`
- All length tests reference domain constants — never hardcode numbers
- Test names follow `[Method]_When_[Condition]_Returns_[Result]` (happy path) and
  `[Method]_When_[Condition]_Throws_[ExceptionType]` (exception path):
  - ✅ `SetFirstName_When_ExceedsMaxLength_Throws_DomainException`
  - ❌ `Should_Throw_When_FirstName_Exceeds_50_Characters`
  - ✅ `IsContentTypeSupported_When_TypeIsNotSupported_Returns_False`
  - ❌ `Should_Fail_When_ContentType_Is_Webp`
- Validator failure tests use `Fails_For[Property]` — e.g. `Fails_ForFirstName_When_Empty`
- HTTP status codes are written as words, never numbers:
  - `200` → `Ok`, `201` → `Created`, `204` → `NoContent`
  - `400` → `BadRequest`, `404` → `NotFound`, `500` → `InternalServerError`
- `When_` is always required — never skip the condition clause
- Scenario lifecycle tests (e.g. `OwnerCrudLifecycle_*`, `FullLifecycle_*`) are exempt from
  the naming pattern — they describe an end-to-end flow, not a single method

### Test Data Builders

`Barkfest.Domain.Tests` and `Barkfest.Application.Tests` use the shared builders from
`Barkfest.Tests.Common/Builders/`. These are referenced via `GlobalUsings.cs` so the
builder classes are available without an explicit `using` in every test file.

| Builder | Default state |
|---|---|
| `OwnerBuilder` | `FirstName="Test"`, `LastName="Owner"`, unique email |
| `PetBuilder` | `OwnerId=NewGuid`, `Name="Buddy"`, `PetType=Dog`, `Breed=DogBreedInfo(Beagle)` |
| `PetImageBuilder` | `BlobName="pets/test/gallery/photo.jpg"`, `ContentType="image/jpeg"`, `DisplayOrder=0` |

**Rules:**
- Never write private `BuildXxx()` helper methods in test classes — use the shared builders
- Override only the properties relevant to the test scenario:
  ```csharp
  // Good — only the property under test is non-default
  var owner = new OwnerBuilder().WithEmail("bad-email").Build();

  // Good — build a collection with specific names
  var pets = new[]
  {
      new PetBuilder().WithOwnerId(ownerId).WithName("Max").Build(),
      new PetBuilder().WithOwnerId(ownerId).WithName("Daisy").Build()
  };
  ```
- Exception: `Domain.Tests` setter tests that need a bare entity (no defaults applied)
  may still use `new Pet(Guid.NewGuid())` directly — builders set domain defaults which
  can mask failures in property-setter tests

### Validator Tests — NSubstitute Limitations
- Never mock `IValidator<T>` with NSubstitute — FluentValidation is strong-named and
  Castle DynamicProxy cannot proxy it for nested or private types. Use concrete
  `AbstractValidator<T>` subclasses instead.
- Never mock `RequestHandlerDelegate<TResponse>` — it is a delegate type and NSubstitute
  cannot mock delegates. Use a real lambda with a closure-based call counter instead.

### EF Core Configuration Tests
- Use the shared `ModelHelper` static class which builds the EF Core model once using
  the SQL Server provider and caches it. Never use the in-memory provider for
  configuration tests — it does not reflect real column names or SQL Server constraints.

### Test Class Member Naming

Private fields in test classes must be named after their concrete type — never `_sut` or
any other generic placeholder.

```csharp
// Correct — name reflects the concrete type
private readonly CreateOwnerCommandHandler _createOwnerCommandHandler;
private readonly CreateOwnerCommandValidator _createOwnerCommandValidator;
private readonly IOwnerRepository _ownerRepository = Substitute.For<IOwnerRepository>();

// Wrong — generic placeholder conveys nothing
private readonly CreateOwnerCommandHandler _sut;
```

The field name must match the class name in camelCase with a leading underscore,
regardless of whether the class is the primary subject under test or a dependency.

### NSubstitute Style
```csharp
var repo = Substitute.For<IOwnerRepository>();
repo.GetByIdAsync(id).Returns(owner);
await repo.Received(1).GetByIdAsync(id);
```

### Shouldly Style
```csharp
result.ShouldNotBeNull();
result.Name.ShouldBe("Buddy");
await act.ShouldThrowAsync<NotFoundException>();
```

---

## What NOT To Use

| Banned | Use Instead |
|---|---|
| AutoMapper | Manual static extension methods in `*Mappings.cs` |
| Moq | NSubstitute |
| FluentAssertions | Shouldly |
| Swagger / Swashbuckle | Scalar |
| `int` primary keys | `Guid` with `Guid.CreateVersion7()` |
| Hardcoded max lengths in tests | Domain constants (`Owner.FirstNameMaxLength` etc.) |
| Hardcoded image limits in tests | `Pet.MaxImages` |
| Data annotations for EF config | `IEntityTypeConfiguration<T>` |
| Try/catch in handlers or controllers | `ExceptionHandlingMiddleware` |
| `dotnet ef database update` | `MigrateAsync()` at startup |
| User Secrets | Aspire connection string injection |

---

## Git Workflow

1. At the start of every session and before any git operation run
   `git branch --show-current` and `git status` to confirm the
   current branch and whether there are any uncommitted changes.

2. Always create a branch before starting any work:
   `git checkout -b feature/<name>`

   Branch naming conventions:
   - `feature/<name>` — new features
   - `fix/<name>`     — bug fixes
   - `chore/<name>`   — maintenance tasks (dependency updates, config changes)
   - `test/<name>`    — adding or fixing tests only

3. Stage specific files by name as work progresses. Run both test suites before
   any `git commit` and confirm all tests pass. Never commit if any tests are failing.

   ```bash
   dotnet test
   npm test --prefix barkfest-ui
   ```

4. Commit after each logical milestone is complete and verified.

5. Always stage specific files by name — never use `git add .` or
   `git add -A`. Before staging, tell the user which files will be
   staged and why, and wait for approval.

6. Always ask the user for approval before running `git commit` —
   show the proposed commit message and wait for confirmation.

7. Never switch branches if there are uncommitted changes. Before any
   `git checkout`, run `git status` and if uncommitted changes exist,
   stop and ask the user how to proceed — either commit, stash, or
   discard them first.

8. Push to the remote branch whenever it makes sense — for example
   at the end of the day, when another developer would like to pull
   the branch for review, or when the feature is complete. Always
   ask the user for approval before pushing. Always push to the
   feature branch — never directly to `main`:
   `git push -u origin <branch-name>`

9. Open a PR on GitHub — squash and merge into `main`.

10. Always ask the user for approval before pulling `main` locally
    and deleting the local branch. Once approved:
    - `git checkout main && git pull`
    - `git branch -d <branch-name>`
    - Run `dotnet build && dotnet test` to verify main is clean

### Feature branch model (post-initial-build)

After `feature/initial-build` merged into `main`, all new work follows this model:

- Each feature gets its own branch: `feature/<name>` (e.g. `feature/landing-page`)
- Each feature gets its own docs folder: `docs/features/<name>/`
  - `PLAN.md` — implementation plan for this feature
  - `PROGRESS.md` — progress tracking for this feature
  - `DECISIONS.md` — decisions made during this feature
- The branch is PR'd into `main` when the feature is complete, then deleted

**Root-level docs after the initial build:**

| File | Status | Notes |
|---|---|---|
| `CLAUDE.md` | Always current | Updated whenever conventions change |
| `README.md` | Always current | Updated when user-facing behaviour changes |
| `docs/ROADMAP.md` | Always current | Updated as features are planned and shipped |
| `docs/SPEC.md` | Always current | Updated as features ship and the spec evolves |
| `PROGRESS.md` | Historical record | Initial build history — not updated for new features |
| `docs/PLAN.md` | Historical record | Initial build plan — not updated for new features |
| `docs/DECISIONS.md` | Historical record | Initial build decisions — not updated for new features |

---

## Progress Tracking

For active feature branches, update the feature's own `docs/features/<name>/PROGRESS.md`
immediately when a milestone is complete. If context is running low, stop at a clean
boundary, update `PROGRESS.md`, and the next session can resume by reading it first.

When starting a new feature, read `docs/ROADMAP.md` to select the next item.

### When to update each documentation file

At the end of every significant body of work, review the relevant files and update as needed:

| File | Update when... |
|---|---|
| `docs/features/<name>/PROGRESS.md` | A milestone within the feature completes |
| `docs/features/<name>/DECISIONS.md` | A decision is made that is specific to this feature |
| `docs/features/<name>/PLAN.md` | The feature plan changes — a step added, removed, or redesigned |
| `docs/ROADMAP.md` | A backlog item is started, completed, or reprioritised |
| `docs/SPEC.md` | User-visible behaviour changes — new endpoints, new business rules |
| `README.md` | Setup steps, environment config, or user-facing behaviour changes |
| `CLAUDE.md` | Session conventions change — new rules, new patterns, corrected guidance |

---

## Key Files

| File | Purpose |
|---|---|
| `CLAUDE.md` | This file — Claude Code session rules and conventions |
| `README.md` | Repo landing page — rendered by GitHub |
| `docs/ROADMAP.md` | Feature backlog — read when choosing the next feature to build |
| `docs/SPEC.md` | Functional specification — what the app does |
| `docs/PLAN.md` | Initial build plan — historical record, phases 1–12 |
| `docs/DECISIONS.md` | Initial build decisions — historical record |
| `PROGRESS.md` | Initial build progress — historical record |
| `docs/features/<name>/PLAN.md` | Implementation plan for a specific feature |
| `docs/features/<name>/PROGRESS.md` | Progress tracking for a specific feature |
| `docs/features/<name>/DECISIONS.md` | Decisions made during a specific feature |
