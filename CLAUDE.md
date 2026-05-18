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

// Entity — class
public class Owner
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    // ...
}
```

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
extension method. The API wires everything together in `Program.cs`:

```csharp
builder.AddServiceDefaults();                           // Aspire — telemetry, health checks
builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration); // standard EF Core, key "barkfest-sql"
builder.AddInfrastructure();                            // Aspire-aware — reads "barkfest-blobs"
```

`AddPersistence` takes `IServiceCollection` + `IConfiguration` (standard EF Core registration).
`AddInfrastructure` takes `IHostApplicationBuilder` (Aspire-aware `AddAzureBlobServiceClient`).
See DECISIONS.md — `AddSqlServerDbContext` was tried but dropped due to `WebApplicationFactory`
configuration injection limitations in .NET 10's minimal hosting model.

Never register services from one layer inside another layer's `DependencyInjection.cs`.

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

**Note:** FluentValidation's `EmailAddress()` does not reject spaces in the local
part of an email address (e.g. `"space in@example.com"` passes). This is by design
in FluentValidation. Do not write test cases expecting it to fail.

---

## Domain Constants

These constants are defined on the entity and must be used everywhere —
validators, tests, EF Core configuration.

| Constant | Value | Location |
|---|---|---|
| `Owner.FirstNameMaxLength` | 50 | `Owner.cs` |
| `Owner.LastNameMaxLength` | 100 | `Owner.cs` |
| `Owner.EmailMaxLength` | 75 | `Owner.cs` |
| `Pet.NameMaxLength` | 75 | `Pet.cs` |
| `Pet.MaxImages` | 5 | `Pet.cs` |
| `PetImage.BlobNameMaxLength` | 500 | `PetImage.cs` |
| `PetImage.ContentTypeMaxLength` | 100 | `PetImage.cs` |

---

## Business Rules

### Owner
- `FirstName` — required, max `Owner.FirstNameMaxLength` chars, trimmed
- `LastName` — required, max `Owner.LastNameMaxLength` chars, trimmed
- `Email` — required, valid email format, max `Owner.EmailMaxLength` chars, lowercased and trimmed
- `PhoneNumber` — optional, no max length constraint

### Pet
- `Name` — required, max `Pet.NameMaxLength` chars, trimmed
- `Description` — optional, no max length, trimmed if provided
- `DateOfBirth` — optional `DateOnly`, cannot be in the future
- `Age` — computed from `DateOfBirth` at runtime, **never stored in the database**
- `PetType` — required SmartEnum
- `Breed` — must match `PetType`: Dog → `DogBreedInfo`, Cat → `CatBreedInfo`, Other → null only
- `Images` — maximum `Pet.MaxImages` gallery images

### Images (applies to all image uploads across the entire application)
- Allowed content types: `image/jpeg`, `image/jpg`, `image/png`
- Allowed extensions: `.jpeg`, `.jpg`, `.png`
- Validated by `SupportedImageType` static class in `Barkfest.Domain`
- Enforced at two layers: Domain (entity methods) and Application (FluentValidation)
- Binary files stored in Azure Blob Storage — SQL Server stores only `BlobName` and `ContentType`

### Profile Images
- Both `Owner` and `Pet` have an optional profile image
- Represented as a `ProfileImage` value object (`sealed record`) with `BlobName` and `ContentType`
- Mapped to two nullable columns in the DB via EF Core `OwnsOne()`:
  `ProfileImageBlobName` nvarchar(500), `ProfileImageContentType` nvarchar(100)

---

## EF Core

- Use `IEntityTypeConfiguration<T>` for all entity configuration — no data annotations
- SmartEnums stored as `int` using `.HasConversion(pt => pt.Value, value => PetType.FromValue(value))`
- `Breed` uses Table Per Hierarchy (TPH) with discriminator column `BreedType` (`"Dog"` or `"Cat"`)
- `ProfileImage` value object mapped using `OwnsOne()` — no separate table
- `Pet.Age` must be ignored: `builder.Ignore(p => p.Age)`
- All cascade deletes: `Owner` → `Pets`, `Pet` → `PetImages`, `Pet` → `Breeds`
- Migration applied at startup via `MigrateAsync()` — never run `dotnet ef database update`

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

Use Serilog. Configured in `Program.cs`.

---

## Exception Handling

`ExceptionHandlingMiddleware` in `Barkfest.API/Middleware/` handles all exceptions:

| Exception | HTTP Response |
|---|---|
| `NotFoundException` | 404 Not Found |
| `DomainException` | 400 Bad Request |
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
| `PetBuilder` | `OwnerId=NewGuid`, `Name="Buddy"`, `PetType=Dog` |
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

3. Stage specific files by name as work progresses. Run `dotnet test` before
   any `git commit` and confirm all tests pass. Never commit if any
   tests are failing.

4. Commit after each logical milestone is complete and verified:
   - After Phase 2 — Domain layer complete and tests passing
   - After Phase 3 — Application layer complete and tests passing
   - After Phase 4 — Persistence layer and migration verified
   - After Phase 5 — Infrastructure layer complete
   - After Phase 6 — API layer complete
   - After Phase 7 — All tests written and passing
   - After Phase 8 — Aspire wired and running locally

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

---

## Progress Tracking

- **Update `PROGRESS.md` immediately when a phase is completed** — do not wait
  until the end of the project.
- Mark the phase as complete, list what was built, and update the `Next` section
  to point to the next phase.
- If context is running low, stop at a clean boundary, update `PROGRESS.md`, and
  the next session can resume by reading `PROGRESS.md` first, then `PLAN.md`.

---

## Key Files

| File | Purpose |
|---|---|
| `CLAUDE.md` | This file — Claude Code session rules and conventions |
| `docs/PLAN.md` | Full build plan — all phases and implementation details |
| `docs/DECISIONS.md` | Architectural and technical decisions with reasoning |
| `docs/SPEC.md` | Functional specification — what the app does |
| `PROGRESS.md` | Current build progress — updated after each phase |
| `README.md` | Repo landing page — rendered by GitHub |
