# Barkfest — Progress

## Phase 1 — Solution Scaffold ✅ Complete

- Created `Barkfest.sln` with 13 projects (7 src, 6 tests)
- All projects target `net10.0`
- All project references wired per Clean Architecture rules
- All NuGet packages installed per PLAN.md
- `.gitignore` created
- `appsettings.json` configured with Aspire connection string keys
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
- `DependencyInjection.cs` using Aspire-aware `AddSqlServerDbContext<AppDbContext>("barkfest-db")`

---

## Phase 5 — Infrastructure Layer ✅ Complete

- `AzureBlobStorageService` implementing `IBlobStorageService`
- `DependencyInjection.cs` using Aspire-aware `AddAzureBlobClient("barkfest-blobs")`

---

## Phase 6 — API Layer ✅ Complete

- `OwnersController` — 7 endpoints
- `PetsController` — 10 endpoints
- `ExceptionHandlingMiddleware` — maps `NotFoundException` → 404, `DomainException` → 400, unhandled → 500
- `Program.cs` with `AddServiceDefaults()`, migrations on startup via `MigrateAsync()`
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

## Phase 8 — .NET Aspire (Local Dev Orchestration) 🔲 Not Started

---

## Next

**Phase 8 — .NET Aspire**

1. Create `Barkfest.AppHost` and `Barkfest.ServiceDefaults` projects
2. Add project references and NuGet packages per PLAN.md
3. Implement `AppHost/Program.cs` with persistent containers and named volumes
4. Wire `AddServiceDefaults()` into API `Program.cs`
5. Update `AddPersistence` and `AddInfrastructure` to Aspire-aware registrations
6. Remove User Secrets from API
