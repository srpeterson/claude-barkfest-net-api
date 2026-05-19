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

---

## Phase 10.2 — Test Infrastructure (Shared Builders) ✅ Complete

- `Barkfest.Tests.Common` project created with `OwnerBuilder`, `PetBuilder`, `PetImageBuilder`
- `Domain.Tests` and `Application.Tests` updated to use builders via `GlobalUsings.cs`
- `CLAUDE.md` updated: `### Test Data Builders` subsection, naming convention rules, HTTP status code words
- `docs/DECISIONS.md` updated: `## Testing` section with two decisions
- `README.md` corrected: Integration.Tests use Testcontainers (not AppHost)

---

## Phase 11 — Authentication & Authorization ✅ Complete

**506 tests across 5 runnable projects — all passing**

| Project | Tests |
|---|---|
| `Barkfest.Domain.Tests` | 159 |
| `Barkfest.Application.Tests` | 222 |
| `Barkfest.Infrastructure.Tests` | 8 |
| `Barkfest.Persistence.Tests` | 71 |
| `Barkfest.API.Tests` | 46 |

### Domain
- `Owner.PasswordHash` property + `SetPasswordHash(string hash)` method
- `Owner.IsAdmin` property + `SetIsAdmin(bool isAdmin)` method (default false)
- `Owner.Active` property + `SetActive(bool active)` method (default true)
- `Owner.IsVisible` property + `SetIsVisible(bool isVisible)` method (default true)
- `IOwnerRepository.GetByEmailAsync` added
- `ForbiddenException` (→ 403)

### Application
- `ICurrentUserService` — `Guid OwnerId { get; }`, `bool IsAdmin { get; }`
- `IJwtTokenService`, `IPasswordHasher` interfaces
- `AuthTokenDto`
- `RegisterCommand` / handler / validator
- `LoginCommand` / handler / validator (blocks inactive owners with `ForbiddenException`)
- `SetOwnerActiveCommand` / handler — admin-only, sets `Owner.Active`
- `SetOwnerVisibilityCommand` / handler — owner-only, sets `Owner.IsVisible`
- All owner + pet handlers updated: `ICurrentUserService` injected, ownership check throws `ForbiddenException`
- `DeleteOwnerCommandHandler` / `RemovePetImageCommandHandler` — admin bypass (`IsAdmin` skips ownership check)
- `CreatePetCommand` — `OwnerId` removed; handler reads from `ICurrentUserService`
- `IBrowseRepository` + `GetBrowseImagesQuery` — public read-only browse (no auth required)
- `GetOwnerByIdQuery`, `GetPetByIdQuery`, `GetPetsByOwnerIdQuery` — two-tier visibility: `Active` (admin-controlled) and `IsVisible` (owner-controlled); owners can always see their own data
- `OwnerDto` — `bool IsVisible` field added

### Persistence
- `OwnerConfiguration` — `PasswordHash`, `IsAdmin`, `Active`, `IsVisible` columns; unique index on `Email`
- `OwnerRepository.GetByEmailAsync` implemented
- `BrowseRepository` — EF Core read-model filtered by `Active && IsVisible`; `AsSplitQuery()` + `Include`/`ThenInclude`
- Migration `AddOwnerPasswordHash` generated
- Migration `AddOwnerAdminAndActive` generated
- Migration `AddOwnerIsVisible` generated

### Infrastructure
- `JwtSettings`, `JwtTokenService` (includes `is_admin` claim), `BcryptPasswordHasher`
- `DependencyInjection` updated: `IJwtTokenService`, `IPasswordHasher`, `JwtSettings` config binding

### API
- `AuthController` — `POST /v1/auth/register` + `POST /v1/auth/login` (`[AllowAnonymous]`)
- `AdminController` — `PATCH /v1/admin/owners/{id}/active` (`[Authorize]`, admin-only enforced in handler)
- `BrowseController` — `GET /v1/browse/images` (`[AllowAnonymous]`, optional `?petType=&breed=` filters)
- `OwnerController` — `[Authorize]` added, `GetAll` removed, `PATCH /v1/owners/{id}/visibility` added
- `PetController` — `[Authorize]` added, `GetAll` removed
- `CreatePetRequest` — `OwnerId` field removed (set server-side from JWT)
- `CurrentUserService` — reads `sub` + `is_admin` claims from JWT via `IHttpContextAccessor`
- `ActiveOwnerMiddleware` — DB check on every authenticated request; inactive owners → 403
- `Program.cs` — `AddJwtBearer` with `MapInboundClaims = false`, `AddHttpContextAccessor`, `ActiveOwnerMiddleware`, admin seed on startup (skipped in Testing)
- `ExceptionHandlingMiddleware` — `ForbiddenException` → 403
- `appsettings.json` / `appsettings.Development.json` (Admin section) / `appsettings.Testing.json` (new)

### Tests
- `Barkfest.Tests.Common` — `JwtTestHelper.GenerateToken(Guid ownerId, bool isAdmin = false)`, `is_admin` claim included
- `BarkfestApiFactory` / `IntegrationApiFactory` — `CreateAuthenticatedClient()` + `CreateAuthenticatedAdminClient()` helpers
- `AuthControllerTests` — 6 tests (register + login scenarios)
- `AdminControllerTests` — 7 tests (activate/deactivate, auth, 403 for non-admin, blocked login, middleware 403)
- `BrowseControllerTests` — 5 tests (public access, filters, unrecognised petType → empty)
- `OwnersControllerTests` / `PetsControllerTests` — fully rewritten: register → authenticated client, 401 tests added
- `OwnerLifecycleTests` / `PetLifecycleTests` — register via `/v1/auth/register`, all requests use authenticated client
- `SetOwnerActiveCommandHandlerTests` — 3 tests (admin activate/deactivate, non-admin forbidden, not found)
- `SetOwnerVisibilityCommandHandlerTests` — 3 tests (owner sets visibility, non-owner forbidden, not found)
- `LoginCommandHandlerTests` — added inactive owner test
- `GetOwnerByIdQueryHandlerTests` — inactive + invisible scenarios with admin/owner bypass
- `GetPetByIdQueryHandlerTests` — inactive + invisible scenarios with admin/owner bypass
- `GetPetsByOwnerIdQueryHandlerTests` — inactive + invisible scenarios with admin/owner bypass
- `OwnerTests` — `SetIsVisible`, `NewOwner_When_Instantiated_Returns_IsVisibleTrue` added

---

---

## Post-Phase 11 — Admin → Administrator Rename

- Entity `Admin` → `Administrator` (`Administrator.cs`, `IAdministratorRepository`, `AdministratorRepository`, `AdministratorConfiguration`)
- Feature folder `Features/Admin/` → `Features/Administrators/`
- `CreateAdminCommand/Handler/Validator` → `CreateAdministratorCommand/Handler/Validator`
- DB table `Admins` → `Administrators`, PK column `AdminId` → `AdministratorId`
- Migration `SeparateAdminIdentity` and model snapshot updated in place (not yet applied)
- All type aliases (`using AdminEntity = ...`) removed — no longer needed
- All test files renamed and updated to match

---

## Post-Phase 11 — Administrator Management

- `UpdateAdministratorPasswordCommand` / handler / validator — admin-only, updates another admin's password hash
- `DeleteAdministratorCommand` / handler — admin-only, deletes another admin; self-delete throws `ForbiddenException`
- `ICurrentUserService` extended: `Guid? AdminId` added alongside existing `Guid? OwnerId` and `bool IsAdmin`
- `CurrentUserService` updated to read `AdminId` from JWT `sub` when `account_type == "admin"`
- `AdminController` extended: `PATCH /v1/admin/admins/{id}/password`, `DELETE /v1/admin/admins/{id}`
- All 4 migrations consolidated into single `InitialCreate` migration (clean schema, no incremental history)
- `README.md` updated: First Login section documents dev credentials and SeedAdminAsync flow (local dev only)
- `CLAUDE.md` updated: `Administrator.EmailMaxLength` constant, Administrator business rules section
- `docs/DECISIONS.md` updated: Admin/Owner separation decision, self-delete guard decision, trust model decision

**538 tests across 5 runnable projects — all passing**

| Project | Tests |
|---|---|
| `Barkfest.Domain.Tests` | 159 |
| `Barkfest.Application.Tests` | 231 |
| `Barkfest.Infrastructure.Tests` | 8 |
| `Barkfest.Persistence.Tests` | 85 |
| `Barkfest.API.Tests` | 55 |

- `AdministratorConfigurationTests` — table name, PK column, EmailMaxLength, required fields, unique email index
- `AdministratorRepositoryTests` — AddAsync, GetByIdAsync, GetByEmailAsync (including case-insensitive), DeleteAsync
- Application.Tests — `UpdateAdministratorPasswordCommandHandlerTests` (3), `UpdateAdministratorPasswordCommandValidatorTests` (2), `DeleteAdministratorCommandHandlerTests` (4)
- API.Tests — `AdminControllerTests` extended with 9 new tests (update password: 4, delete: 5)

---

---

## Post-Phase 11 — Username Login

- `Owner.Username` property added with `SetUsername()` and `UsernameMaxLength = 50`
- Login changed from email/password to username/password — `LoginCommand.Email` → `LoginCommand.Username`
- `RegisterCommand` extended with `Username` field
- `RegisterCommandHandler` checks username uniqueness (DomainException if taken) before email uniqueness
- `LoginCommandHandler` uses `GetByUsernameAsync` instead of `GetByEmailAsync`
- `IOwnerRepository.GetByUsernameAsync` added; `GetByEmailAsync` retained for register uniqueness check
- `OwnerRepository.GetByUsernameAsync` implemented (exact case-sensitive match)
- `OwnerConfiguration` — `Username` column `nvarchar(50) NOT NULL`, unique index `IX_Owners_Username`
- `OwnerDto` — `Username` field added
- `OwnerMappings` — `Username` mapped
- `OwnerBuilder` — `_username` default (unique guid), `WithUsername()` method, `SetUsername()` called in `Build()`
- Migration `AddOwnerUsername` generated
- Email remains unique — contact field, not used for login
- `CLAUDE.md` — `Owner.UsernameMaxLength` constant, updated Owner business rules

**553 tests across 5 runnable projects — all passing**

| Project | Tests |
|---|---|
| `Barkfest.Domain.Tests` | 164 |
| `Barkfest.Application.Tests` | 236 |
| `Barkfest.Infrastructure.Tests` | 8 |
| `Barkfest.Persistence.Tests` | 90 |
| `Barkfest.API.Tests` | 55 |

---

## Post-Phase 11 — Administrator Username, Phone Validation, Content Moderation & Domain Refactoring

### Administrator identity
- `Administrator.Username` added — login identifier (max 50, trimmed, unique index `IX_Administrators_Username`)
- Admin login switched from email/password → username/password — `AdminLoginCommand.Email` → `AdminLoginCommand.Username`
- `IAdministratorRepository.GetByUsernameAsync` added; `GetByEmailAsync` retained for create uniqueness check
- `AdministratorRepository.GetByUsernameAsync` implemented (exact case-sensitive match)
- `SeedAdminAsync` in `Program.cs` updated to read and set `Admin:Username`
- `appsettings.Development.json` — `Admin:Username = "admin"` replaces email-based seed key
- `README.md` — First Login section updated to use `username` field

### Administrator profile fields
- `Administrator.Name` — required string, max 100 characters, trimmed
- `Administrator.PhoneNumber` — required string, E.164 format, max 25 characters
- `CreateAdministratorCommand` extended with `Name` and `PhoneNumber`
- `CreateAdministratorCommandValidator` — Name (required, max length) and PhoneNumber (required, E.164) rules added
- `AdministratorConfiguration` — `Name` and `PhoneNumber` columns configured
- `SeedAdminAsync` reads `Admin:Name` and `Admin:PhoneNumber` from config
- `appsettings.Development.json` — `Admin:Name = "Barkfest Admin"`, `Admin:PhoneNumber = "+15555550100"` added

### Phone number validation (Owner)
- `Owner.PhoneNumber` — E.164 format enforced (`^\+[1-9]\d{1,14}$`); null/empty clears the field
- `Owner.PhoneNumberMaxLength = 25` — column constraint added via migration
- `RegisterCommandValidator` and `UpdateOwnerCommandValidator` — E.164 `.Matches()` rule added (`.When` guards optional field)
- `OwnerConfiguration` — `nvarchar(25)` column constraint

### NotFoundException overload
- Added `NotFoundException(string name, string keyName, object key)` overload — produces `"Owner with username 'x' was not found."` instead of the generic id form
- `LoginCommandHandler` and `AdminLoginCommandHandler` use the new overload

### Content moderation scaffold
- `IContentModerationService` — `Task<bool> IsImageSafeAsync(Stream, CancellationToken)` in `Application.Common.Interfaces`
- `NoOpContentModerationService` — always returns `true`; detailed TODO comment points to Azure AI Content Safety
- Injected (singleton) into `UploadOwnerProfileImageCommandHandler`, `UploadPetProfileImageCommandHandler`, `AddPetImageCommandHandler`
- All three handlers reject images that fail moderation with `DomainException` before any blob upload

### Domain refactoring — shared ValueObjects
- `E164PhoneNumber` static class (`ValueObjects/`) — `Pattern`, `MaxLength`, `IsValid()` — single source of truth for all E.164 concerns; `Owner` and `Administrator` removed their duplicated constants and now call `E164PhoneNumber.IsValid()`
- `AccountConstraints` static class (`ValueObjects/`) — `EmailMaxLength = 75`, `UsernameMaxLength = 50` — replaces duplicated constants on both entities; all validators and EF Core configurations updated

### Migration consolidation
- All previous migrations merged into a single `InitialCreate` migration representing the full current schema

**621 tests across 6 projects — all passing**

| Project | Tests |
|---|---|
| `Barkfest.Domain.Tests` | 174 |
| `Barkfest.Application.Tests` | 264 |
| `Barkfest.Infrastructure.Tests` | 8 |
| `Barkfest.Persistence.Tests` | 100 |
| `Barkfest.API.Tests` | 55 |
| `Barkfest.Integration.Tests` | 20 |

---

## Next

All phases complete.
