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

### Decision: Maximum 5 gallery images per pet
**Choice:** `Pet.MaxImages = 5` enforced as a domain constant in `Pet.cs`.

**Reason:** Prevents unbounded image uploads. The limit is a constant rather
than a hardcoded magic number so it can be increased in one place without
touching validators, tests, or any other code. Start conservative — increase
when there is user demand.

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

### Decision: Testcontainers for all integration tests
**Choice:** `Testcontainers.MsSql` for SQL Server and `Testcontainers.Azurite`
for Azure Blob Storage in all integration and API tests.

**Reason:** No manual setup, no shared test databases, no dependency on a local
SQL Server or Azure Storage instance. Each test run gets a clean isolated
container spun up automatically. Tests are fully repeatable on any machine and
in CI/CD pipelines. `DatabaseFixture` and `AzuriteFixture` handle container
lifecycle via `IAsyncLifetime`.

---

### Decision: Serilog for logging
**Choice:** Serilog configured in `Program.cs`.

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

## Git and Workflow

### Decision: Branch protection on `main`, PRs only
**Choice:** GitHub ruleset prevents direct commits to `main`. All changes
must go through an approved PR with squash and merge.

**Reason:** Keeps `main` always releasable. Squash and merge produces a clean
linear history — one PR equals one commit on `main`. Code review enforced
for all changes regardless of author.

---

### Decision: `claude/` prefix for all Claude Code branches
**Choice:** All branches created by Claude Code use the prefix `claude/` —
e.g. `claude/feature/initial-build`, `claude/fix/email-validation`.

**Reason:** Immediately identifiable in GitHub's branch list and PR view.
Distinguishes Claude Code initiated branches from branches created by human
developers at a glance.

---

### Decision: Never use `git add .` or `git add -A`
**Choice:** All staging must name specific files explicitly. Claude Code must
show the user which files will be staged and wait for approval before staging.

**Reason:** `git add .` silently stages unintended files — secrets, build
artifacts, temporary files. Explicit staging with user approval ensures only
intended files enter the commit. This is a non-negotiable safety rule.

---

### Decision: `dotnet test` must pass before any commit
**Choice:** Claude Code runs `dotnet test` before every `git commit` and waits
for all tests to pass. Staging does not require tests to pass.

**Reason:** Prevents broken code from ever landing in a commit — even locally.
Staging is a work-in-progress operation and does not need a test gate. Committing
is a statement that work is complete and verified — tests must confirm this.
