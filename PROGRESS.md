# Barkfest — Progress

## Phase 1 — Solution Scaffold ✅ Complete

- Created `Barkfest.sln` with 13 projects (7 src, 6 tests)
- All projects target `net10.0`
- All project references wired per Clean Architecture rules
- All NuGet packages installed per PLAN.md
- `.gitignore` created
- `appsettings.json` configured (no placeholder connection strings — Aspire injects at runtime)
- `README.md` at repo root (renders on GitHub landing page)
- `docs/` folder created with SPEC.md, PLAN.md, DECISIONS.md

---

## Phase 2 — Domain Layer ✅ Complete

- `DomainException.cs`
- `ProfileImage` value object (`sealed record`) with `Create()` factory
- `SupportedImageType` static class — jpeg, jpg, png
- SmartEnums: `PetType` (3), `DogBreed` (30), `CatBreed` (29)
- Entities: `Owner`, `Pet`, `PetImage`, `Breed` (abstract), `DogBreedInfo`, `CatBreedInfo`
- Interfaces: `IOwnerRepository`, `IPetRepository`, `IUnitOfWork`

---

## Phase 3 — Application Layer ✅ Complete

- `NotFoundException`, `IBlobStorageService`
- `ValidationBehavior`, `LoggingBehavior`
- DTOs: `OwnerDto`, `PetDto`, `PetImageDto`, `ProfileImageDto`
- Mappings: `OwnerMappings`, `PetMappings`
- Owner commands and queries: Create, Update, Delete, GetById, GetAll, UploadProfileImage, RemoveProfileImage
- Pet commands and queries: Create, Update, Delete, GetById, GetAll, GetByOwnerId, UploadProfileImage, RemovePetProfileImage, AddImage, RemoveImage
- All validators wired with FluentValidation
- `DependencyInjection.cs`

---

## Phase 4 — Persistence Layer ✅ Complete

- `AppDbContext` with `DbSet<Owner>`, `DbSet<Pet>`, `DbSet<PetImage>`, `DbSet<Breed>`
- EF Core configurations: `OwnerConfiguration`, `PetConfiguration`, `PetImageConfiguration`, `BreedConfiguration`
- TPH discriminator on `Breed` table (`BreedType` column, `"Dog"` / `"Cat"` values)
- `OwnerRepository`, `PetRepository`, `UnitOfWork`
- `InitialCreate` migration generated and verified
- `DependencyInjection.cs` using `AddDbContext<AppDbContext>` with Aspire connection string key `"barkfest-sql"`

---

## Phase 5 — Infrastructure Layer ✅ Complete

- `AzureBlobStorageService` implementing `IBlobStorageService`
- `DependencyInjection.cs` using Aspire-aware `AddAzureBlobServiceClient("barkfest-blobs")`

---

## Phase 6 — API Layer ✅ Complete

- `OwnersController` — 7 endpoints
- `PetsController` — 10 endpoints
- `ExceptionHandlingMiddleware` — maps `NotFoundException` → 404, `DomainException` → 400, unhandled → 500
- `Program.cs` with `AddServiceDefaults()`, migrations on startup via `MigrateAsync()` (skipped in Testing)
- Scalar API documentation

---

## Phase 7 — Test Projects ✅ Complete

**409 tests across 6 projects — all passing**

| Project | Tests |
|---|---|
| `Barkfest.Domain.Tests` | 145 |
| `Barkfest.Application.Tests` | 145 |
| `Barkfest.Infrastructure.Tests` | 8 |
| `Barkfest.Persistence.Tests` | 71 |
| `Barkfest.API.Tests` | 20 |
| `Barkfest.Integration.Tests` | 20 |

- Domain: entities, value objects, SmartEnums, `SupportedImageType`
- Application: all command/query handlers, all validators, `ValidationBehavior`
- Infrastructure: `AzureBlobStorageService` against live Azurite (Testcontainers)
- Persistence: EF Core configuration tests via `ModelHelper`, repository integration tests
- API.Tests: controller tests via `WebApplicationFactory` + Testcontainers
- Integration.Tests: full owner and pet lifecycle flows over HTTP

---

## Phase 8 — .NET Aspire (Local Dev Orchestration) ✅ Complete

- `Barkfest.AppHost` project — orchestrates SQL Server and Azurite with persistent named volumes
- `Barkfest.ServiceDefaults` project — OpenTelemetry, health checks, service discovery defaults
- `AppHost.cs` — `ContainerLifetime.Persistent`, volumes `barkfest-sql-data` and `barkfest-blobs-data`
- API `Program.cs` — `builder.AddServiceDefaults()`, `app.MapDefaultEndpoints()`, `MigrateAsync()` on startup
- Infrastructure DI — `AddAzureBlobServiceClient("barkfest-blobs")` (Aspire-aware)
- Persistence DI — `AddDbContext<AppDbContext>` with key `"barkfest-sql"` (Aspire injects connection string via env var; standard EF Core registration used for test compatibility — see DECISIONS.md)
- `appsettings.json` — no placeholder connection strings; Aspire injects everything at runtime
- `BarkfestApiFactory` — updated for Aspire: injects connection strings via `DbContextOptions` replacement; BlobServiceClient version-pinned for Azurite compatibility
- All 409 tests pass with the updated setup

---

## Phase 9 — API Refinements ✅ Complete

- `OwnersController.cs` → `OwnerController.cs` (singular class name, .NET convention)
- `PetsController.cs` → `PetController.cs` (singular class name, .NET convention)
- Route prefix changed from `api/` → `v1/` on both controllers — more meaningful, signals versioning intent
- Routes remain plural (`v1/owners`, `v1/pets`) — REST convention; HTTP verb + ID conveys single vs collection
- All URL strings updated in `Barkfest.API.Tests` and `Barkfest.Integration.Tests`
- All 409 tests passing

---

## Post-Completion Changes

### Naming
- Renamed Aspire SQL Server resource from `"barkfest-db"` → `"barkfest-sql"` and volume from `"barkfest-db-data"` → `"barkfest-sql-data"` across `AppHost.cs`, `Persistence/DependencyInjection.cs`, `appsettings.json`, `CLAUDE.md`, `DECISIONS.md`, `PLAN.md`

### README
- Added Git and EF Core CLI tools (`dotnet ef` — minimum version `10.0.7`, via `dotnet tool restore`) to Prerequisites
- Added `(Highly Recommended)` heading to Docker image pre-pull section
- Added `docker images` verification step and IPv6 fix note (Windows network adapter level)
- Added Visual Studio 2026 note to .NET 10 SDK prerequisite

### API
- `launchSettings.json` — both `http` and `https` profiles updated: `"launchBrowser": true`, `"launchUrl": "scalar/v1"`

### Verified
- Aspire ran successfully for the first time locally — SQL Server and Azurite containers started, migration applied, Scalar UI opened

---

## Phase 10 — Test Refinements ✅ Complete

**378 tests across 5 runnable projects — all passing**

| Project | Tests |
|---|---|
| `Barkfest.Domain.Tests` | 142 |
| `Barkfest.Application.Tests` | 137 |
| `Barkfest.Infrastructure.Tests` | 8 |
| `Barkfest.Persistence.Tests` | 71 |
| `Barkfest.API.Tests` | 20 |

- All test names across all 6 projects renamed to follow `[Method]_When_[Condition]_Returns_[Result]` / `[Method]_When_[Condition]_Throws_[ExceptionType]`
- HTTP status codes use words not numbers (`NoContent`, `NotFound`, `BadRequest`, `Created`)
- `Fails_For[Property]` used consistently for validator failure tests
- All `_AtMaxLength_Passes` boundary tests removed — only failure cases tested
- EF Core configuration tests left unchanged — static model facts, no method/condition pattern applies
- Scenario-style lifecycle tests (`OwnerCrudLifecycle_*`, `PetCrudLifecycle_*`, `FullLifecycle_*`) left unchanged
- `SupportedImageType` methods renamed: `IsAllowedContentType` → `IsContentTypeSupported`, `IsAllowedExtension` → `IsFileExtensionSupported` — all call sites updated across Domain, Application, and PLAN.md

---

## Next

All phases complete.
