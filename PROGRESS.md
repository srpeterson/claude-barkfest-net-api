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

## Post-Phase 11 — Email Verification Scaffold & Project Documentation

### Email verification scaffold
- `Owner.IsEmailVerified` (`bool`, default `false`) — not nullable, DB default `false`
- `Owner.VerificationToken` (`string?`) — nullable, no max length constraint
- `SetVerificationToken(string token)` — required, trims input, throws `DomainException` if empty
- `MarkEmailVerified()` — sets `IsEmailVerified = true`, clears `VerificationToken`
- `OwnerConfiguration` — `IsEmailVerified` (not null, default `false`), `VerificationToken` (nullable) configured
- Migration consolidated into single `InitialCreate` — both new columns included
- Login is **unenforced** — `LoginCommandHandler` unchanged; owners log in regardless of verification status

### Project documentation
- `docs/ROADMAP.md` created — post-build feature backlog in priority order
  - Email verification: domain scaffolded, enforcement deferred pending `IEmailService`
  - Image moderation: `IContentModerationService` scaffolded, activation deferred pending Azure AI Content Safety
- `CLAUDE.md` updated — documentation update rules table, ROADMAP.md reference and read trigger

**611 tests across 6 projects — all passing**

| Project | Tests |
|---|---|
| `Barkfest.Domain.Tests` | 181 |
| `Barkfest.Application.Tests` | 264 |
| `Barkfest.Infrastructure.Tests` | 8 |
| `Barkfest.Persistence.Tests` | 103 |
| `Barkfest.API.Tests` | 55 |
| `Barkfest.Integration.Tests` | 20 |

---

## Post-Phase 11 — GetAll Endpoints

- `GET /v1/owners` — lists all owners; admin JWT required; `ForbiddenException` for non-admins
- `GET /v1/admin/admins` — lists all administrators; admin JWT required; `ForbiddenException` for non-admins
- `IAdministratorRepository.GetAllAsync` — new interface method + repository implementation
- `AdministratorDto` — new DTO (Id, Username, Name, Email, PhoneNumber, CreatedAt — no PasswordHash)
- `AdministratorMappings` — `ToDto()` and `ToDtoList()` extension methods
- `GetAllAdministratorsQuery` + handler with admin-only check
- `GetAllOwnersQueryHandler` — `ICurrentUserService` injected, admin-only check added
- Admin-only enforcement in handlers (Application layer) — not just at the route level
- CLAUDE.md — Domain Constants table corrected (AccountConstraints, E164PhoneNumber, Administrator.NameMaxLength); Administrator business rules updated; Authorization section added
- DECISIONS.md — admin-only GetAll decision added

**621 tests across 6 projects — all passing**

| Project | Tests |
|---|---|
| `Barkfest.Domain.Tests` | 181 |
| `Barkfest.Application.Tests` | 268 |
| `Barkfest.Infrastructure.Tests` | 8 |
| `Barkfest.Persistence.Tests` | 103 |
| `Barkfest.API.Tests` | 61 |
| `Barkfest.Integration.Tests` | 20 |

---

## Post-Phase 11 — Static Create() Factory Methods & ROADMAP update

### Domain entities
- `Owner.Create(username, firstName, lastName, email, passwordHash, phoneNumber?)` added
- `Administrator.Create(username, name, email, phoneNumber, passwordHash)` added
- `Pet.Create(ownerId, name, petType, description?, dateOfBirth?)` added
- `PetImage.Create(blobName, contentType, displayOrder)` added
- `DogBreedInfo.Create(dogBreed)` added
- `CatBreedInfo.Create(catBreed)` added

### Handlers updated
- `RegisterCommandHandler` — uses `Owner.Create()`
- `CreateAdministratorCommandHandler` — uses `Administrator.Create()`
- `CreatePetCommandHandler` — uses `Pet.Create()`
- `AddPetImageCommandHandler` — uses `PetImage.Create()`

### Test builders updated
- `OwnerBuilder` — `_passwordHash` default added, `WithPasswordHash()` added, `Build()` uses `Owner.Create()`
- `PetBuilder` — `Build()` uses `Pet.Create()`
- `PetImageBuilder` — `Build()` uses `PetImage.Create()`

### ROADMAP.md
- Item 2 added: Value Object Emails (and Other Validated Strings) — deferred with full rationale

### Documentation
- `CLAUDE.md` — entity factory method rule added to C# Type Conventions
- `docs/DECISIONS.md` — `static Create()` factory method decision added

---

---

## Phase 12 — Frontend Scaffold ✅ Complete

### barkfest-ui (Vite + React + TypeScript)

- `barkfest-ui/` scaffolded at repo root using Vite + React + TypeScript template
- **Package manager:** pnpm
- **Routing:** React Router v6 (deliberately v6, not v7)
- **Server state:** TanStack Query v5 (`QueryClientProvider`, `ReactQueryDevtools`)
- **Styling:** Tailwind CSS v4 via `@tailwindcss/vite` plugin
- **Component library:** shadcn/ui (base-ui headless primitives)
- **Path alias:** `@/` → `src/` configured in `vite.config.ts` and `tsconfig.app.json`
- **Testing:** Vitest + React Testing Library (`pnpm test`, `pnpm test:watch`, `pnpm test:ui`)
- **pnpm-workspace.yaml** — `allowBuilds: msw: true` to allow msw postinstall (pnpm 11 requirement)
- `.env.example` committed as template; `.env` gitignored

### Routing structure

- `ShellLayout` with header nav — wraps all routes via `<Outlet />`
- Routes: `/login`, `/register`, `/owners`, `/pets`
- Default redirect: `/` → `/login`

### API client

- `src/lib/api.ts` — typed `get`, `post`, `put`, `delete` wrappers
- All requests include `credentials: 'include'` for HttpOnly cookie support (Phase 13)

### Auth context

- `src/hooks/useAuth.ts` — `isAuthenticated`, `accountId`, `accountType` in sessionStorage
- `signIn(accountId, accountType)` / `signOut()` helpers

### .NET API changes

- CORS policy `BarkfestUI` — `AllowCredentials()`, origin from `Cors:AllowedOrigin` config
- `app.UseCors("BarkfestUI")` added before `UseAuthentication()`
- `appsettings.Development.json` — `"Cors": { "AllowedOrigin": "http://localhost:5173" }`

### Aspire integration

- `Aspire.Hosting.JavaScript` v13.3.4 added to `Barkfest.AppHost`
- `AppHost.cs` — `AddViteApp("barkfest-ui", "../../barkfest-ui").WithPnpm().WithHttpEndpoint(port: 5173).WithEnvironment("VITE_API_BASE_URL", api.GetEndpoint("https")).WaitFor(api)`
- Aspire auto-installs pnpm packages and starts Vite dev server — no `.env` needed when running via Aspire

### Versions

| Package | Version |
|---|---|
| Vite | 8.0.12 |
| React | 19.2.6 |
| React Router DOM | 7.15.1 |
| TanStack Query | 5.100.11 |
| Tailwind CSS | 4.3.0 |
| TypeScript | 6.0.2 |
| Vitest | 4.1.6 |
| @types/node | 25.9.0 |

**621 .NET tests — all passing. Frontend: 0 tests (scaffold only, tests written per feature in Phase 13+)**

---

## Chore — NuGet Package Upgrades

All NuGet packages upgraded to latest compatible versions. `Directory.Packages.props` is the single source of truth (central package management).

| Package | From | To | Note |
|---|---|---|---|
| `Microsoft.AspNetCore.OpenApi` | 10.0.7 | 10.0.8 | |
| `Microsoft.OpenApi` | 2.0.0 | 2.7.4 | Pinned to 2.x — see ROADMAP item 3 and comment in `Directory.Packages.props` |
| `Microsoft.Extensions.Logging.Abstractions` | 10.0.0 | 10.0.8 | |
| `Aspire.Azure.Storage.Blobs` | 13.3.3 | 13.3.4 | |
| `Aspire.Microsoft.EntityFrameworkCore.SqlServer` | 13.3.3 | 13.3.4 | |
| `Microsoft.EntityFrameworkCore.*` | 10.0.7 | 10.0.8 | |
| `Aspire.Hosting.Azure.Storage` | 13.3.3 | 13.3.4 | |
| `Aspire.Hosting.SqlServer` | 13.3.3 | 13.3.4 | |
| `Microsoft.Extensions.Http.Resilience` | 10.5.0 | 10.6.0 | |
| `Microsoft.Extensions.ServiceDiscovery` | 10.5.0 | 10.6.0 | |
| `coverlet.collector` | 10.0.0 | 10.0.1 | |
| `Testcontainers.Azurite` | 4.11.0 | 4.12.0 | |
| `Testcontainers.MsSql` | 4.11.0 | 4.12.0 | |

**621 tests — all passing after upgrades.**

---

## Chore — Program.cs Refactoring (Startup Folder)

`Program.cs` extracted from ~160 lines into three focused static extension method classes
under `src/Barkfest.API/Startup/`:

| File | Extension method | Responsibility |
|---|---|---|
| `ServiceRegistration.cs` | `AddBarkfestServices(this WebApplicationBuilder)` | Serilog, controllers, CORS, OpenAPI, Application/Persistence/Infrastructure DI, JWT auth, CurrentUserService |
| `DatabaseInitializer.cs` | `InitialiseDatabaseAsync(this WebApplication)` | `MigrateAsync()` + `SeedAdminAsync` (skips Testing environment) |
| `PipelineConfiguration.cs` | `ConfigurePipeline(this WebApplication)` | Middleware order, route mapping |

`Program.cs` is now 14 lines. `CLAUDE.md` and `DECISIONS.md` updated to reflect the pattern.

**621 tests — all passing.**

---

---

## Chore — Application Insights (Azure Monitor OpenTelemetry)

Added `Azure.Monitor.OpenTelemetry.AspNetCore` 1.5.0 to `Barkfest.ServiceDefaults`.
Activated the Azure Monitor exporter block in `Extensions.cs` — conditionally enabled when
`APPLICATIONINSIGHTS_CONNECTION_STRING` is present in configuration. Gracefully inactive in
local dev (Aspire dashboard used instead). Zero config required for local development.

**How to activate in Azure:**
Set `APPLICATIONINSIGHTS_CONNECTION_STRING` as an environment variable in Azure App Service
(Settings → Environment variables) or via Key Vault. Never commit a real connection string.

**621 tests — all passing. 0 build warnings.**

---

---

## Chore — Increase Pet.MaxImages from 5 to 6

- `Pet.MaxImages` constant changed from `5` → `6` in `Barkfest.Domain/Entities/Pet.cs`
- All tests already reference `Pet.MaxImages` — no test changes required
- SPEC.md, PLAN.md, CLAUDE.md updated to reflect the new limit

**621 tests — all passing.**

---

## Phase 13 — Deployment Pipeline ✅ Complete

### Step 1 — Bicep Infrastructure (`infra/main.bicep`)

- Azure Container Registry, Container Apps Environment, Container App, SQL Server + Database, Storage Account + Blob Container, Application Insights, Log Analytics Workspace, Static Web App
- `Dockerfile` (multi-stage build) and `.dockerignore`
- Bicep compiled and verified (`az bicep build`)
- Resources provisioned to Azure (`az deployment sub create --location centralus`)

### Step 2 — GitHub Secrets

- Service Principal created (`az ad sp create-for-rbac`) with subscription-level Contributor
- All 18 secrets added to GitHub repository Settings → Secrets and variables → Actions

### Step 3 — API Pipeline (`.github/workflows/api.yml`)

- Triggers on merge to `main`
- Build, test, Docker build/push to ACR, set Container App env vars from secrets, deploy

### Step 4 — Frontend Pipeline (`.github/workflows/ui.yml`)

- Triggers on merge to `main`
- Install, test, build React frontend, deploy to Azure Static Web Apps

### Step 5 — Verified

- API live at `https://ca-barkfest.greenisland-212561c8.centralus.azurecontainerapps.io`
- Frontend live at `https://gray-rock-0394ee50f.7.azurestaticapps.net`
- Container App set to `minReplicas=1` — always warm, no cold starts

### Notes

- Service Principal requires subscription-level Contributor (not just resource group) for Container Apps
- `Microsoft.App` provider registration was stuck — fixed via Azure Portal re-register
- `az containerapp` commands require the containerapp CLI extension (`az extension add --name containerapp`)

---

## Phase 14 — Browse API Enhancements ✅ Complete

- `PagedResult<T>` generic wrapper (`Page`, `PageSize`, `TotalCount`, computed `HasMore`)
- `IBrowseRepository` updated — return type changed to `PagedResult<BrowseImageDto>`, `page` and `pageSize` params added
- `BrowseRepository` updated — `.Where(pi => pi.IsFeaturedImage)` filter (one card per pet), breed filter pushed to DB via `EF.Property<int>(pi.Pet.Breed, "BreedValue")` on TPH column, `CountAsync` + `Skip/Take` pagination
- `GetBrowseImagesQuery` updated — `Page` and `PageSize` params, `PagedResult<BrowseImageDto>` return type; unknown `petType` returns empty `PagedResult`
- `GetBrowsePetTypesQuery` + handler — reads from `PetType.List`, no DB call
- `GetBrowseBreedsQuery` + handler — reads from `DogBreed.List` / `CatBreed.List`, ordered by SmartEnum value, no DB call
- `BrowseController` updated — `page` + `pageSize` query params on `GetImages`; new `GET /v1/browse/pet-types` and `GET /v1/browse/breeds?petType=` endpoints
- `BrowseRepositoryTests` added — ordering, featured filter, pagination
- 704 tests — all passing

---

## Phase 15 — Handoff Home Page ✅ Complete

- Claude Design handoff imported into `barkfest-ui`
- CSS conflicts resolved — merged CSS variables, aligned `@theme`, fixed collisions
- Font stack replaced: Geist → DM Sans Variable (body) + Playfair Display (headings)
- Home page wired into routing in `App.tsx`
- Visual verification passed, tests passing

---

## Phase 16 — Home Page Wire Filter ✅ Complete

- `FilterBar.tsx` Pet Type and Breed dropdowns hydrated from the browse API
- Pet types driven by `GET /v1/browse/pet-types`; "All" is a synthetic UI option; display labels via `src/config/petTypes.ts`
- Breeds driven by `GET /v1/browse/breeds?petType=`; disabled when "All" selected
- `staleTime: Infinity` for both queries — lists only change with a deployment
- Unit tests for `getBlobImageUrl` and `getPetTypeLabel` (9/9 passing)

---

## Phase 17 — Authentication UI ✅ Complete

### API — HttpOnly Cookie Auth
- Login endpoints (`/v1/auth/login`, `/v1/auth/admin/login`) set a `barkfest_auth` HttpOnly cookie (`Secure`, `SameSite=Strict`) instead of returning the token in the response body
- `POST /v1/auth/logout` endpoint added — clears the cookie
- `AddJwtBearer` updated to read the token from the cookie via `OnMessageReceived`
- Password minimum length raised from 8 → 10 characters (aligned with frontend strength meter guidance); `AccountConstraints.PasswordMinLength` updated
- All API tests updated and passing

### UI — Auth Context, Modals, Navbar
- `AuthContext.tsx` — auth state, `signIn`, `signOut`, modal open/close state
- `useAuth.ts` hook
- `api.ts` — `credentials: 'include'` on all requests, `setUnauthorizedHandler`, `login`, `adminLogin`, `logout` helpers
- `LoginModal.tsx` — username + password fields, admin checkbox (UI present, disabled — admin login available via Scalar), validation, resets on close
- `RegisterModal.tsx` — full registration form with zxcvbn password strength meter and confirm password field
- `Navbar.tsx` — three-state render: unauthenticated (Sign In), owner (Post a Pet + Sign Out), admin (label + Sign Out)
- `ProtectedRoute.tsx` — owner-only gate; unauthenticated users redirected to home page with login modal triggered
- `App.tsx` — 401 interception handler registered; expired token signs user out and re-prompts login
- TypeScript check clean, UI smoke tested

---

## PR #11 — Breed Refactor ✅ Complete

Replaced the separate `Breeds` table (TPH with `DogBreedInfo`/`CatBreedInfo` entity classes) with a single `Breed int NOT NULL` column directly on `Pets`. The `PetType` column already provides the species discriminator, making `BreedType` and `BreedId` redundant.

### Changes
- `Breed` abstract entity + `DogBreedInfo`/`CatBreedInfo` subclasses removed from Domain
- `Pet.Breed` changed from a navigation property to an `int` column storing the SmartEnum value
- `BreedConfiguration`, TPH discriminator, and `DbSet<Breed>` removed from Persistence
- Migration copies existing breed values before dropping the `Breeds` table
- All application handlers, validators, mappings, and tests updated
- `PetDto.Breed` changed from a nested object to a plain `string` (resolved from SmartEnum name)

**720 tests — all passing.**

---

## PR #12 — Owner Profile Dialog & Pet Dialog UX Polish ✅ Complete

### Backend
- `DeletePetCommand` and `BatchDeletePetImagesCommand` — blob cleanup added before removing DB records (orphaned blobs bug fixed)
- Tests added for blob deletion in both command handler test classes

### UI
- `UpdateOwnerProfileDialog` — two-step dialog (step 1: personal info, step 2: profile photo) opened from Navbar avatar
- Profile image round avatar displayed in Navbar; `profileImageBlobName` persisted to `sessionStorage`
- Owner profile image fetched on login and register for instant Navbar avatar
- Display name availability check skips unchanged value; browse cache invalidated only when display name changes
- `isEmail` validation (validator pkg) added to Register and Profile dialogs
- Admin checkbox hidden in `LoginDialog` pending admin UI milestone
- Success screens removed from `AddPetDialog` and `UpdateOwnerProfileDialog` — dialogs close immediately on save with a "Saving…" spinner
- `LoginModal`/`RegisterModal` renamed to `LoginDialog`/`RegisterDialog` throughout

**Test count unchanged — all passing.**

---

## PR #13 — SmartEnum Integer Values for Pet Type/Breed ✅ Complete

Replaced string-based pet type and breed representation with integer SmartEnum values throughout the full stack.

### Changes
- Browse endpoints return `{ name, value }` objects instead of plain strings — integer flows directly from option value attribute with no client-side lookup map
- `FilterBar`, `HeroSection`, and `HomePage` carry integer state
- `AddPetDialog` migrated to new `PetTypeBreedFormFields` component (form-specific, integer props)
- `PetTypeBreedSelector` retained as a filter-only component
- Cross-species breed validation uses `SmartEnum.TryFromValue` for correctness
- API query parameters, command/query records, repository interfaces, and persistence layer all updated

**Test count unchanged — all passing.**

---

## PR #14 — Pet Likes & Public Pet Detail ✅ Complete

### Backend
- `Pet.Likes` — `int NOT NULL DEFAULT 0` column; `IncrementLikes()` / `DecrementLikes()` domain methods (decrement floors at zero)
- `POST /v1/pets/{id}/likes` — increments likes; `[AllowAnonymous]`
- `DELETE /v1/pets/{id}/likes` — decrements likes, floors at zero; `[AllowAnonymous]`
- `GET /v1/pets/{id}` — made `[AllowAnonymous]` for public Pet Details page
- `OwnerId` added to `BrowseImageDto` — available at navigation time before full pet data loads
- Migration `AddPetLikes` generated

### Tests
- `PetsControllerTests` extended with likes endpoint tests (increment, decrement, floor at zero, public access)

**754 tests — all passing.**

| Project | Tests |
|---|---|
| `Barkfest.Domain.Tests` | 193 |
| `Barkfest.Application.Tests` | 346 |
| `Barkfest.Infrastructure.Tests` | 8 |
| `Barkfest.Persistence.Tests` | 97 |
| `Barkfest.API.Tests` | 89 |
| `Barkfest.Integration.Tests` | 21 |

---

## PR #15 — UI Redesign, Owner Password Change & Cache Fixes ✅ Complete

### UI Redesign
- `BarkfestMark` — stroke-based paw SVG used as brand header in all dialogs
- `Navbar` — cream translucent sticky bar; filter controls (pet type + breed dropdowns) moved into Navbar (desktop: centred between logo and auth; mobile: compact pill opening a bottom sheet)
- `LoginPage`/`RegisterPage` — split-panel layout with brand panel (local pet photos mosaic, testimonial), eye toggles, focus box-shadows
- `PetDetailPage` — magazine-style hero layout, floating info card, lightbox, like/unlike button with optimistic updates; owner sees read-only like count + edit/delete kebab menu
- `ManagePetsPage` — table layout with bulk delete bar, `HidePetsToggle` with optimistic update
- `PetCard` — navigable (clicks through to Pet Details page)
- All dialogs updated with `BarkfestMark` brand header
- `EditPetModal` — two-step pre-filled edit dialog (pet info + image management)
- `public/pets/` — 6 local pet photos for Sign In brand panel mosaic

### New Features
- `ChangePasswordDialog` — self-service password change (current password required)
- Username availability check on registration — `GET /v1/auth/check-username`; debounced inline feedback
- `useIsMobile` hook

### API Changes
- `PetDto.Id` → `PetDto.PetId`, `PetImageDto.Id` → `PetImageDto.PetImageId` — self-describing JSON keys
- `Owner.Active` → `Owner.IsActive` — consistency with `IsVisible`, `IsEmailVerified`; migration `AlterOwnerActiveColumn`
- `GET /v1/auth/check-username` — new public endpoint

### Cache & Sync Fixes
- `LoginPage` fetches owner after login to populate `profileImageBlobName` (was only done in `LoginDialog`)
- Navbar profile image backed by React Query (staleTime 30s, refetchOnWindowFocus) — picks up changes made on another device
- `UpdateOwnerProfileDialog` writes new blob name directly into query cache after upload — instant same-device update without logout
- `api.ts` — `cache: 'no-store'` on all fetch calls to prevent browser caching of API responses

### Developer Experience
- Vite dev proxy (`/v1` → `https://localhost:7101`) — `pnpm dev --host` works for mobile testing without `.env` configuration
- `RegisterDialog` removed — re-auth via `LoginDialog` only; registration goes through `RegisterPage`

**754 tests — all passing. UI: 9 tests passing.**

---

## Fix — Navbar displayName TypeScript Error ✅ Complete

- Removed unused `displayName` variable in `Navbar.tsx` that was causing a CI TypeScript error
