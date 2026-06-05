# Barkfest — Build Plan

## Overview

Build a .NET 10 Clean Architecture solution called `Barkfest`. It is a pet management
application allowing owners to register themselves and show off their pets. The solution uses
SQL Server via EF Core for relational data, and Azure Blob Storage for all images.

---

## Solution Structure

```
Barkfest.sln
├── src/
│   ├── Barkfest.AppHost
│   ├── Barkfest.ServiceDefaults
│   ├── Barkfest.Domain
│   ├── Barkfest.Application
│   ├── Barkfest.Persistence
│   ├── Barkfest.Infrastructure
│   ├── Barkfest.API
│   └── tests/
│       ├── Barkfest.Tests.Common        ← shared test helpers and builders
│       ├── Barkfest.Domain.Tests
│       ├── Barkfest.Application.Tests
│       ├── Barkfest.Persistence.Tests
│       ├── Barkfest.Infrastructure.Tests
│       ├── Barkfest.API.Tests
│       └── Barkfest.Integration.Tests
```

---

## Phase 1 — Solution Scaffold

- [x] Create solution file `Barkfest.sln`
- [x] Create all 13 projects with correct project types and target framework `net10.0`
- [x] Add all project references as defined below
- [x] Add all NuGet packages as defined below
- [x] Create `.gitignore` appropriate for a .NET solution

### Project References

```
Barkfest.AppHost
  └── Barkfest.API

Barkfest.API
  ├── Barkfest.Application
  ├── Barkfest.Persistence
  ├── Barkfest.Infrastructure
  └── Barkfest.ServiceDefaults

Barkfest.Application
  └── Barkfest.Domain

Barkfest.Persistence
  ├── Barkfest.Domain
  └── Barkfest.Application

Barkfest.Infrastructure
  ├── Barkfest.Domain
  └── Barkfest.Application

Barkfest.Tests.Common         → Barkfest.Domain
Barkfest.Domain.Tests         → Barkfest.Domain, Barkfest.Tests.Common
Barkfest.Application.Tests    → Barkfest.Application, Barkfest.Tests.Common
Barkfest.Persistence.Tests    → Barkfest.Persistence
Barkfest.Infrastructure.Tests → Barkfest.Infrastructure
Barkfest.API.Tests            → Barkfest.API
Barkfest.Integration.Tests    → Barkfest.API
```

### NuGet Packages

All package versions are managed centrally in `Directory.Packages.props` at the repo root.
Individual `.csproj` files reference packages without version numbers.

> ⚠️ Do **not** upgrade `Microsoft.OpenApi` to 3.x — see the comment in `Directory.Packages.props`
> and ROADMAP item 3 for details.

| Project | Packages |
|---|---|
| `Barkfest.AppHost` | `Aspire.Hosting.AppHost`, `Aspire.Hosting.SqlServer`, `Aspire.Hosting.Azure.Storage` |
| `Barkfest.ServiceDefaults` | `Microsoft.Extensions.ServiceDiscovery`, `OpenTelemetry.Exporter.OpenTelemetryProtocol`, `OpenTelemetry.Extensions.Hosting`, `OpenTelemetry.Instrumentation.AspNetCore`, `OpenTelemetry.Instrumentation.Http`, `OpenTelemetry.Instrumentation.Runtime` |
| `Barkfest.Domain` | `Ardalis.SmartEnum` |
| `Barkfest.Application` | `MediatR`, `FluentValidation` |
| `Barkfest.Persistence` | `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Tools`, `Aspire.Microsoft.EntityFrameworkCore.SqlServer` |
| `Barkfest.Infrastructure` | `Azure.Storage.Blobs`, `Aspire.Azure.Storage.Blobs` |
| `Barkfest.API` | `Scalar.AspNetCore`, `Serilog.AspNetCore`, `Microsoft.AspNetCore.OpenApi`, `Microsoft.OpenApi` |
| `*.Tests` (unit) | `xunit`, `Shouldly`, `NSubstitute` |
| `*.Tests` (integration) | above + `Testcontainers.MsSql`, `Testcontainers.Azurite` |
| `Barkfest.API.Tests` | above + `Microsoft.AspNetCore.Mvc.Testing` |
| `Barkfest.Integration.Tests` | `xunit`, `Shouldly`, `Testcontainers.MsSql`, `Testcontainers.Azurite` |

---

## Phase 2 — Domain Layer

### 2.1 Exceptions

- [x] Create `Barkfest.Domain/Exceptions/DomainException.cs`

### 2.2 Value Objects — use `sealed record`

- [x] Create `Barkfest.Domain/ValueObjects/ProfileImage.cs`
  - Properties: `BlobName` (string), `ContentType` (string)
  - Private constructor, static `Create()` factory method
  - Validates: `BlobName` required, `ContentType` required and validated via `SupportedImageType`
  - Trims `BlobName`, lowercases and trims `ContentType`
  - Record provides structural equality automatically — no manual `Equals`/`GetHashCode` needed

### 2.3 Static Classes

- [x] Create `Barkfest.Domain/ValueObjects/SupportedImageType.cs`
  - `AllowedContentTypes`: `image/jpeg`, `image/jpg`, `image/png`
  - `AllowedExtensions`: `.jpeg`, `.jpg`, `.png`
  - `IsContentTypeSupported(string contentType)` — case insensitive
  - `IsFileExtensionSupported(string fileName)` — case insensitive

### 2.4 SmartEnums — extend `SmartEnum<T>`

- [x] Create `Barkfest.Domain/Enums/PetType.cs`
  - `Dog` (1), `Cat` (2), `Other` (3)

- [x] Create `Barkfest.Domain/Enums/DogBreed.cs`
  - Top 25 AKC registered breeds (2025 rankings):
    1. French Bulldog, 2. Labrador Retriever, 3. Golden Retriever,
    4. German Shepherd Dog, 5. Dachshund, 6. Poodle, 7. Beagle,
    8. Rottweiler, 9. German Shorthaired Pointer, 10. Bulldog,
    11. Cane Corso, 12. Cavalier King Charles Spaniel, 13. Yorkshire Terrier,
    14. Australian Shepherd, 15. Doberman Pinscher, 16. Pembroke Welsh Corgi,
    17. Miniature Schnauzer, 18. Boxer, 19. Pomeranian, 20. Bernese Mountain Dog,
    21. Shih Tzu, 22. Great Dane, 23. Boston Terrier, 24. Chihuahua, 25. Havanese
  - Designer crossbreeds: `Labradoodle` (26), `Goldendoodle` (27), `Cockapoo` (28)
  - Catch-alls: `Mixed` (29), `Other` (30)

- [x] Create `Barkfest.Domain/Enums/CatBreed.cs`
  - Top 25 CFA registered breeds (2025 rankings):
    1. Maine Coon, 2. Ragdoll, 3. Exotic, 4. Persian, 5. Devon Rex,
    6. British Shorthair, 7. Abyssinian, 8. American Shorthair, 9. Scottish Fold,
    10. Sphynx, 11. Siberian, 12. Russian Blue, 13. Bengal, 14. Siamese,
    15. Norwegian Forest Cat, 16. Birman, 17. Burmese, 18. Tonkinese,
    19. Himalayan, 20. Oriental Shorthair, 21. Savannah, 22. Ragamuffin,
    23. Turkish Angora, 24. Manx, 25. Ocicat
  - Common owner-identified types: `DomesticShorthair` (26), `Tabby` (27)
  - Catch-alls: `Mixed` (28), `Other` (29)

### 2.5 Entities — use `class` (mutable, identity-based)

- [x] Create `Barkfest.Domain/Entities/Owner.cs`
  - Properties: `Id` (Guid), `FirstName`, `LastName`, `Email`, `PhoneNumber` (nullable),
    `ProfileImage` (nullable `ProfileImage` value object),
    `Pets` (`IReadOnlyCollection<Pet>`), `CreatedAt`
  - `Id` initialised with `Guid.CreateVersion7()`
  - `CreatedAt` initialised with `DateTime.UtcNow`
  - Constants:
    - `public const int FirstNameMaxLength = 50`
    - `public const int LastNameMaxLength = 100`
    - `public const int EmailMaxLength = 75`
  - Methods:
    - `SetFirstName(string)` — required, max 50 chars, trimmed
    - `SetLastName(string)` — required, max 100 chars, trimmed
    - `SetEmail(string)` — required, valid email format, max 75 chars, lowercased and trimmed
    - `SetProfileImage(string blobName, string contentType)` — delegates to `ProfileImage.Create()`
    - `RemoveProfileImage()` — sets `ProfileImage` to null

- [x] Create `Barkfest.Domain/Entities/Breed.cs` (abstract base)
  - Properties: `Id` (Guid), `PetId` (Guid), `Pet`
  - `Id` initialised with `Guid.CreateVersion7()`

- [x] Create `Barkfest.Domain/Entities/DogBreedInfo.cs` (extends `Breed`)
  - Properties: `DogBreed` (SmartEnum)
  - `SetDogBreed(DogBreed)` — null throws `DomainException`

- [x] Create `Barkfest.Domain/Entities/CatBreedInfo.cs` (extends `Breed`)
  - Properties: `CatBreed` (SmartEnum)
  - `SetCatBreed(CatBreed)` — null throws `DomainException`

- [x] Create `Barkfest.Domain/Entities/PetImage.cs`
  - Properties: `Id` (Guid), `PetId` (Guid), `Pet`, `BlobName`, `ContentType`, `DisplayOrder`, `CreatedAt`
  - `Id` initialised with `Guid.CreateVersion7()`
  - `CreatedAt` initialised with `DateTime.UtcNow`
  - Constants:
    - `public const int BlobNameMaxLength = 500`
    - `public const int ContentTypeMaxLength = 100`
  - Methods:
    - `SetImage(string blobName, string contentType)` — validates via `SupportedImageType`
    - `SetDisplayOrder(int order)` — must be zero or greater

- [x] Create `Barkfest.Domain/Entities/Pet.cs`
  - Properties: `Id` (Guid), `Name`, `Description` (nullable), `DateOfBirth` (nullable `DateOnly`),
    `PetType`, `Breed` (nullable), `ProfileImage` (nullable `ProfileImage` value object),
    `Images` (`IReadOnlyCollection<PetImage>`), `OwnerId` (Guid), `Owner`, `CreatedAt`
  - `Id` initialised with `Guid.CreateVersion7()`
  - `CreatedAt` initialised with `DateTime.UtcNow`
  - Computed property: `Age` (nullable `int`, calculated from `DateOfBirth`, never stored in DB)
  - Constants:
    - `public const int NameMaxLength = 75`
    - `public const int MaxImages = 6`
  - Methods:
    - `SetName(string)` — required, max 75 chars, trimmed
    - `SetDescription(string?)` — optional, no max length, trimmed
    - `SetDateOfBirth(DateOnly?)` — nullable, cannot be in the future
    - `SetPetType(PetType)` — required, null throws `DomainException`
    - `SetBreed(Breed?)` — enforces breed type matches pet type:
      - Dog → must be `DogBreedInfo`
      - Cat → must be `CatBreedInfo`
      - Other → breed must be null
    - `SetProfileImage(string blobName, string contentType)` — delegates to `ProfileImage.Create()`
    - `RemoveProfileImage()` — sets `ProfileImage` to null
    - `AddImage(PetImage)` — null throws, enforces max 6 images via `MaxImages` constant
    - `RemoveImage(Guid petImageId)` — not found throws `DomainException`

### 2.6 Interfaces

- [x] Create `Barkfest.Domain/Interfaces/IOwnerRepository.cs`
  - `GetByIdAsync(Guid id, CancellationToken)`
  - `GetAllAsync(CancellationToken)`
  - `AddAsync(Owner, CancellationToken)`
  - `UpdateAsync(Owner, CancellationToken)`
  - `DeleteAsync(Guid id, CancellationToken)`

- [x] Create `Barkfest.Domain/Interfaces/IPetRepository.cs`
  - `GetByIdAsync(Guid id, CancellationToken)`
  - `GetAllAsync(CancellationToken)`
  - `GetByOwnerIdAsync(Guid ownerId, CancellationToken)`
  - `AddAsync(Pet, CancellationToken)`
  - `UpdateAsync(Pet, CancellationToken)`
  - `DeleteAsync(Guid id, CancellationToken)`

- [x] Create `Barkfest.Domain/Interfaces/IUnitOfWork.cs`
  - `SaveChangesAsync(CancellationToken)`

---

## Phase 3 — Application Layer

### 3.1 Common

- [x] Create `Barkfest.Application/Common/Exceptions/NotFoundException.cs`
- [x] Create `Barkfest.Application/Common/Interfaces/IBlobStorageService.cs`
  - `UploadAsync(string containerName, string blobName, Stream content, string contentType, CancellationToken)`
  - `DownloadAsync(string containerName, string blobName, CancellationToken)`
  - `DeleteAsync(string containerName, string blobName, CancellationToken)`
  - `ExistsAsync(string containerName, string blobName, CancellationToken)`
- [x] Create `Barkfest.Application/Common/Behaviors/ValidationBehavior.cs`
- [x] Create `Barkfest.Application/Common/Behaviors/LoggingBehavior.cs`

### 3.2 DTOs — use `record`

- [x] Create `Barkfest.Application/Features/Owners/DTOs/OwnerDto.cs` (record)
  - `Guid Id`, `string FirstName`, `string LastName`, `string Email`,
    `string? PhoneNumber`, `ProfileImageDto? ProfileImage`, `DateTime CreatedAt`

- [x] Create `Barkfest.Application/Features/Pets/DTOs/PetDto.cs` (record)
  - `Guid Id`, `string Name`, `string? Description`, `DateOnly? DateOfBirth`,
    `int? Age`, `string PetType`, `string? Breed`, `ProfileImageDto? ProfileImage`,
    `IReadOnlyCollection<PetImageDto> Images`, `Guid OwnerId`, `DateTime CreatedAt`

- [x] Create `Barkfest.Application/Features/Pets/DTOs/PetImageDto.cs` (record)
  - `Guid Id`, `string BlobName`, `string ContentType`, `int DisplayOrder`, `DateTime CreatedAt`

- [x] Create `Barkfest.Application/Features/Pets/DTOs/ProfileImageDto.cs` (record)
  - `string BlobName`, `string ContentType`

### 3.3 Mappings — static extension methods, no AutoMapper

- [x] Create `Barkfest.Application/Features/Owners/OwnerMappings.cs`
  - `ToDto(this Owner)` → `OwnerDto`
  - `ToDtoList(this IEnumerable<Owner>)` → `IEnumerable<OwnerDto>`

- [x] Create `Barkfest.Application/Features/Pets/PetMappings.cs`
  - `ToDto(this Pet)` → `PetDto`
  - `ToDtoList(this IEnumerable<Pet>)` → `IEnumerable<PetDto>`

### 3.4 Owner Commands and Queries — use `record` for commands/queries, `class` for handlers/validators

- [x] `CreateOwner/CreateOwnerCommand.cs` (record) — `IRequest<Guid>`
- [x] `CreateOwner/CreateOwnerCommandHandler.cs` (class)
- [x] `CreateOwner/CreateOwnerCommandValidator.cs` (class)
  - `FirstName`: not empty, max length via `Owner.FirstNameMaxLength`
  - `LastName`: not empty, max length via `Owner.LastNameMaxLength`
  - `Email`: not empty, valid email format, max length via `Owner.EmailMaxLength`

- [x] `UpdateOwner/UpdateOwnerCommand.cs` (record) — `IRequest`
- [x] `UpdateOwner/UpdateOwnerCommandHandler.cs` (class)
- [x] `UpdateOwner/UpdateOwnerCommandValidator.cs` (class) — same rules as Create

- [x] `DeleteOwner/DeleteOwnerCommand.cs` (record) — `IRequest`
- [x] `DeleteOwner/DeleteOwnerCommandHandler.cs` (class)

- [x] `UploadOwnerProfileImage/UploadOwnerProfileImageCommand.cs` (record)
- [x] `UploadOwnerProfileImage/UploadOwnerProfileImageCommandHandler.cs` (class)
- [x] `UploadOwnerProfileImage/UploadOwnerProfileImageCommandValidator.cs` (class)
  - `ContentType`: must pass `SupportedImageType.IsContentTypeSupported()`
  - `FileName`: must pass `SupportedImageType.IsFileExtensionSupported()`

- [x] `RemoveOwnerProfileImage/RemoveOwnerProfileImageCommand.cs` (record)
- [x] `RemoveOwnerProfileImage/RemoveOwnerProfileImageCommandHandler.cs` (class)
  - Only calls `IBlobStorageService.DeleteAsync()` if owner has an existing image

- [x] `GetOwnerById/GetOwnerByIdQuery.cs` (record) — `IRequest<OwnerDto>`
- [x] `GetOwnerById/GetOwnerByIdQueryHandler.cs` (class)

- [x] `GetAllOwners/GetAllOwnersQuery.cs` (record) — `IRequest<IEnumerable<OwnerDto>>`
- [x] `GetAllOwners/GetAllOwnersQueryHandler.cs` (class)

### 3.5 Pet Commands and Queries

- [x] `CreatePet/CreatePetCommand.cs` (record) — `IRequest<Guid>`
- [x] `CreatePet/CreatePetCommandHandler.cs` (class)
- [x] `CreatePet/CreatePetCommandValidator.cs` (class)
  - `Name`: not empty, max length via `Pet.NameMaxLength`

- [x] `UpdatePet/UpdatePetCommand.cs` (record) — `IRequest`
- [x] `UpdatePet/UpdatePetCommandHandler.cs` (class)
- [x] `UpdatePet/UpdatePetCommandValidator.cs` (class) — same rules as Create

- [x] `DeletePet/DeletePetCommand.cs` (record) — `IRequest`
- [x] `DeletePet/DeletePetCommandHandler.cs` (class)

- [x] `UploadPetProfileImage/UploadPetProfileImageCommand.cs` (record)
- [x] `UploadPetProfileImage/UploadPetProfileImageCommandHandler.cs` (class)
- [x] `UploadPetProfileImage/UploadPetProfileImageCommandValidator.cs` (class)
  - Same image type validation as Owner

- [x] `RemovePetProfileImage/RemovePetProfileImageCommand.cs` (record)
- [x] `RemovePetProfileImage/RemovePetProfileImageCommandHandler.cs` (class)

- [x] `AddPetImage/AddPetImageCommand.cs` (record)
- [x] `AddPetImage/AddPetImageCommandHandler.cs` (class)
  - Enforces `Pet.MaxImages` limit
- [x] `AddPetImage/AddPetImageCommandValidator.cs` (class)
  - Same image type validation as Owner

- [x] `RemovePetImage/RemovePetImageCommand.cs` (record)
- [x] `RemovePetImage/RemovePetImageCommandHandler.cs` (class)

- [x] `GetPetById/GetPetByIdQuery.cs` (record) — `IRequest<PetDto>`
- [x] `GetPetById/GetPetByIdQueryHandler.cs` (class)

- [x] `GetAllPets/GetAllPetsQuery.cs` (record) — `IRequest<IEnumerable<PetDto>>`
- [x] `GetAllPets/GetAllPetsQueryHandler.cs` (class)

- [x] `GetPetsByOwnerId/GetPetsByOwnerIdQuery.cs` (record) — `IRequest<IEnumerable<PetDto>>`
- [x] `GetPetsByOwnerId/GetPetsByOwnerIdQueryHandler.cs` (class)

### 3.6 Dependency Injection

- [x] Create `Barkfest.Application/DependencyInjection.cs`
  - `AddApplication()` extension method
  - Registers MediatR with `ValidationBehavior` and `LoggingBehavior`
  - Registers FluentValidation validators

---

## Phase 4 — Persistence Layer

### 4.1 DbContext

- [x] Create `Barkfest.Persistence/AppDbContext.cs`
  - `DbSet<Owner> Owners`
  - `DbSet<Pet> Pets`
  - `DbSet<PetImage> PetImages`
  - `DbSet<Breed> Breeds`

### 4.2 EF Core Configurations

> **Column naming rule:** All primary key `Id` properties map to `{EntityName}Id`
> in the database via `HasColumnName()`. This ensures raw SQL and Dapper queries
> are self-describing in joins.

- [x] Create `Barkfest.Persistence/Configurations/OwnerConfiguration.cs`
  - `Id` → column `OwnerId`, default `newsequentialid()`
  - `FirstName` → `nvarchar(50)`, not null
  - `LastName` → `nvarchar(100)`, not null
  - `Email` → `nvarchar(75)`, not null
  - `PhoneNumber` → `nvarchar(max)`, nullable
  - `OwnsOne(ProfileImage)` → columns `ProfileImageBlobName` nvarchar(500), `ProfileImageContentType` nvarchar(100), both nullable

- [x] Create `Barkfest.Persistence/Configurations/PetConfiguration.cs`
  - `Id` → column `PetId`, default `newsequentialid()`
  - `OwnerId` → FK → `Owners.OwnerId`, cascade delete
  - `Name` → `nvarchar(75)`, not null
  - `Description` → `nvarchar(max)`, nullable
  - `DateOfBirth` → `date`, nullable
  - `PetType` → `int`, not null, SmartEnum conversion (`pt.Value` / `PetType.FromValue()`)
  - `OwnsOne(ProfileImage)` → columns `ProfileImageBlobName` nvarchar(500), `ProfileImageContentType` nvarchar(100), both nullable
  - `Ignore(p => p.Age)` — computed, not stored
  - One-to-many with `Owner` via `HasOne(...).WithMany(o => o.Pets)`

- [x] Create `Barkfest.Persistence/Configurations/PetImageConfiguration.cs`
  - `Id` → column `PetImageId`, default `newsequentialid()`
  - `PetId` → column `PetId`, FK → `Pets.PetId`, cascade delete
  - `BlobName` → `nvarchar(500)`, not null
  - `ContentType` → `nvarchar(100)`, not null
  - `DisplayOrder` → `int`, not null

- [x] Create `Barkfest.Persistence/Configurations/BreedConfiguration.cs`
  - `Id` → column `BreedId`, default `newsequentialid()`
  - `PetId` → column `PetId`, FK → `Pets.PetId`, cascade delete
  - TPH discriminator column `BreedType` nvarchar(50): `"Dog"` → `DogBreedInfo`, `"Cat"` → `CatBreedInfo`
  - `DogBreedInfo.DogBreed` → column `BreedValue`, SmartEnum int conversion, nullable
  - `CatBreedInfo.CatBreed` → column `BreedValue`, SmartEnum int conversion, nullable
  - One-to-one with `Pet` via `HasOne(...).WithOne(p => p.Breed)`

### 4.3 Repositories

- [x] Create `Barkfest.Persistence/Repositories/OwnerRepository.cs` implementing `IOwnerRepository`
- [x] Create `Barkfest.Persistence/Repositories/PetRepository.cs` implementing `IPetRepository`
- [x] Create `Barkfest.Persistence/UnitOfWork.cs` implementing `IUnitOfWork`

### 4.4 Migration

- [x] Generate migration `InitialCreate`:
  ```bash
  dotnet ef migrations add InitialCreate \
    --project src/Barkfest.Persistence \
    --startup-project src/Barkfest.API
  ```
- [x] Verify `Up()` method creates all tables with correct column names, types, and constraints
- [x] Do NOT run `dotnet ef database update` — migration applied at runtime via `MigrateAsync()`

**Expected tables and columns:**

`Owners`: `OwnerId`, `FirstName` nvarchar(50), `LastName` nvarchar(100),
`Email` nvarchar(75), `PhoneNumber` nvarchar(max) nullable,
`ProfileImageBlobName` nvarchar(500) nullable, `ProfileImageContentType` nvarchar(100) nullable,
`CreatedAt` datetime2

`Pets`: `PetId`, `OwnerId` FK, `Name` nvarchar(75), `Description` nvarchar(max) nullable,
`DateOfBirth` date nullable, `PetType` int,
`ProfileImageBlobName` nvarchar(500) nullable, `ProfileImageContentType` nvarchar(100) nullable,
`CreatedAt` datetime2

`PetImages`: `PetImageId`, `PetId` FK, `BlobName` nvarchar(500), `ContentType` nvarchar(100),
`DisplayOrder` int, `CreatedAt` datetime2

`Breeds`: `BreedId`, `PetId` FK, `BreedType` nvarchar(50), `BreedValue` int nullable

### 4.5 Dependency Injection

- [x] Create `Barkfest.Persistence/DependencyInjection.cs`
  - `AddPersistence(IServiceCollection, IConfiguration)` extension method
  - Registers `AppDbContext` with SQL Server using `services.AddDbContext<AppDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("barkfest")))`
  - Registers `IOwnerRepository` → `OwnerRepository`
  - Registers `IPetRepository` → `PetRepository`
  - Registers `IUnitOfWork` → `UnitOfWork`
  - Note: standard EF Core `AddDbContext` used instead of Aspire's `AddSqlServerDbContext` for `WebApplicationFactory` test compatibility — see DECISIONS.md

---

## Phase 5 — Infrastructure Layer

- [x] Create `Barkfest.Infrastructure/Storage/AzureBlobStorageService.cs` implementing `IBlobStorageService`
  - `UploadAsync()`, `DownloadAsync()`, `DeleteAsync()`, `ExistsAsync()`

- [x] Create `Barkfest.Infrastructure/Messaging/EmailService.cs`

- [x] Create `Barkfest.Infrastructure/DependencyInjection.cs`
  - `AddInfrastructure(IHostApplicationBuilder)` extension method
  - Registers `BlobServiceClient` using `builder.AddAzureBlobClient("barkfest-blobs")`
  - Registers `IBlobStorageService` → `AzureBlobStorageService`

---

## Phase 6 — API Layer

### 6.1 Controllers

- [x] Create `Barkfest.API/Controllers/OwnersController.cs`
  - `GET    /api/owners`                               → `GetAllOwnersQuery`
  - `GET    /api/owners/{id:guid}`                     → `GetOwnerByIdQuery` — 404 if not found
  - `POST   /api/owners`                               → `CreateOwnerCommand` — 201 Created
  - `PUT    /api/owners/{id:guid}`                     → `UpdateOwnerCommand` — 404 if not found
  - `DELETE /api/owners/{id:guid}`                     → `DeleteOwnerCommand` — 404 if not found
  - `POST   /api/owners/{id:guid}/profile-image`       → `UploadOwnerProfileImageCommand`
  - `DELETE /api/owners/{id:guid}/profile-image`       → `RemoveOwnerProfileImageCommand`

- [x] Create `Barkfest.API/Controllers/PetsController.cs`
  - `GET    /api/pets`                                 → `GetAllPetsQuery`
  - `GET    /api/pets/{id:guid}`                       → `GetPetByIdQuery` — 404 if not found
  - `GET    /api/owners/{ownerId:guid}/pets`           → `GetPetsByOwnerIdQuery`
  - `POST   /api/pets`                                 → `CreatePetCommand` — 201 Created
  - `PUT    /api/pets/{id:guid}`                       → `UpdatePetCommand` — 404 if not found
  - `DELETE /api/pets/{id:guid}`                       → `DeletePetCommand` — 404 if not found
  - `POST   /api/pets/{id:guid}/profile-image`         → `UploadPetProfileImageCommand`
  - `DELETE /api/pets/{id:guid}/profile-image`         → `RemovePetProfileImageCommand`
  - `POST   /api/pets/{id:guid}/images`                → `AddPetImageCommand`
  - `DELETE /api/pets/{id:guid}/images/{imageId:guid}` → `RemovePetImageCommand`

### 6.2 Middleware

- [x] Create `Barkfest.API/Middleware/ExceptionHandlingMiddleware.cs`
  - Catches `NotFoundException` → 404
  - Catches `DomainException` → 400
  - Catches unhandled exceptions → 500

### 6.3 Program.cs

- [x] Configure `Program.cs`:
  ```csharp
  builder.AddServiceDefaults();

  builder.Services.AddApplication();
  builder.AddPersistence();
  builder.AddInfrastructure();

  builder.Services.AddControllers();
  builder.Services.AddOpenApi();

  var app = builder.Build();

  if (app.Environment.IsDevelopment())
  {
      app.MapOpenApi();
      app.MapScalarApiReference();
  }

  app.UseMiddleware<ExceptionHandlingMiddleware>();
  app.UseHttpsRedirection();
  app.UseAuthorization();
  app.MapControllers();

  // Apply migrations at startup
  using (var scope = app.Services.CreateScope())
  {
      var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
      await db.Database.MigrateAsync();
  }

  app.Run();
  ```

### 6.4 Configuration

- [x] Create `appsettings.json`:
  ```json
  {
    "ConnectionStrings": {
      "barkfest-sql": "",
      "barkfest-blobs": ""
    },
    "_readme": "Connection strings are injected by Aspire when running locally. In production or CI populate these via environment variables or a secrets manager."
  }
  ```

---

## Phase 7 — Test Projects

### 7.1 Barkfest.Domain.Tests

- [x] `OwnerTests.cs`
  - `SetFirstName`: valid, trim, at max length, null throws, empty throws, whitespace throws, exceeds max length throws
  - `SetLastName`: valid, trim, at max length, null throws, empty throws, whitespace throws, exceeds max length throws
  - `SetEmail`: valid, lowercase and trim, at max length, null throws, empty throws, whitespace throws, no @ symbol throws, no domain throws, no TLD throws, spaces throw, exceeds max length throws
  - `SetProfileImage`: valid, null blob name throws, empty blob name throws, null content type throws, empty content type throws, unsupported content type throws
  - `RemoveProfileImage`: clears profile image

- [x] `PetTests.cs`
  - `SetName`: valid, trim, at max length, null throws, empty throws, whitespace throws, exceeds max length throws
  - `SetDescription`: valid, trim, accepts null
  - `SetDateOfBirth`: valid, accepts null, future date throws
  - `Age`: null when no DOB, correct age when DOB set, zero when DOB is today
  - `SetPetType`: valid, null throws
  - `SetBreed`: dog gets dog breed, cat gets cat breed, accepts null, dog given cat breed throws, cat given dog breed throws, Other with breed set throws
  - `SetProfileImage`: valid, null blob name throws, empty blob name throws, null content type throws, empty content type throws, unsupported content type throws
  - `RemoveProfileImage`: clears profile image
  - `AddImage`: adds when under limit, accepts up to max limit, exceeds max throws, null throws
  - `RemoveImage`: removes when found, not found throws
  - Use `Pet.MaxImages` constant — never hardcode the number

- [x] `PetImageTests.cs`
  - `SetImage`: valid, jpeg accepted, jpg accepted, png accepted, null blob name throws, empty blob name throws, null content type throws, empty content type throws, unsupported content type throws
  - `SetDisplayOrder`: valid, zero accepted, negative throws

- [x] `ProfileImageTests.cs`
  - `Create`: valid, lowercases and trims content type, trims blob name, jpeg accepted, jpg accepted, png accepted
  - `Create` sad path: null blob name throws, empty blob name throws, whitespace blob name throws, null content type throws, empty content type throws, whitespace content type throws, unsupported content type throws
  - Equality: same values are equal, different values are not equal

- [x] `BreedTests.cs`
  - `SetDogBreed`: valid, null throws
  - `SetCatBreed`: valid, null throws

- [x] `SupportedImageTypeTests.cs`
  - `IsContentTypeSupported`: jpeg true, jpg true, png true, unsupported false, case insensitive
  - `IsFileExtensionSupported`: .jpeg true, .jpg true, .png true, unsupported false, case insensitive

- [x] `PetTypeTests.cs`
  - Has Dog, Cat, Other values
  - Has exactly 3 values
  - Lookup by name: Dog, Cat, Other, invalid throws
  - Lookup by value: Dog, Cat, Other, invalid throws

- [x] `DogBreeedTests.cs`
  - Has exactly 30 values
  - Includes Labradoodle, Goldendoodle, Cockapoo, Mixed, Other
  - Lookup by name, lookup by value, invalid name throws, invalid value throws
  - Unique values, unique names

- [x] `CatBreedTests.cs`
  - Has exactly 29 values
  - Includes DomesticShorthair, Tabby, Mixed, Other
  - Lookup by name, lookup by value, invalid name throws, invalid value throws
  - Unique values, unique names

### 7.2 Barkfest.Application.Tests

Use NSubstitute for all mocking. Use Shouldly for all assertions.

- [x] `CreateOwnerCommandHandlerTests.cs`
- [x] `UpdateOwnerCommandHandlerTests.cs`
- [x] `DeleteOwnerCommandHandlerTests.cs`
- [x] `GetOwnerByIdQueryHandlerTests.cs` — returns OwnerDto when found, throws NotFoundException when not found
- [x] `GetAllOwnersQueryHandlerTests.cs`

- [x] `CreateOwnerCommandValidatorTests.cs`
  - `FirstName`: valid, at max length, empty fails, null fails, whitespace fails, exceeds max length fails
  - `LastName`: valid, at max length, empty fails, null fails, whitespace fails, exceeds max length fails
  - `Email`: valid, at max length, empty fails, null fails, whitespace fails, no @ symbol fails, no domain fails, no TLD fails, spaces fail, exceeds max length fails

- [x] `UpdateOwnerCommandValidatorTests.cs` — mirror same cases as Create

- [x] `UploadOwnerProfileImageCommandHandlerTests.cs`
  - Uploads image and updates owner
  - Throws NotFoundException when owner not found

- [x] `UploadOwnerProfileImageCommandValidatorTests.cs`
  - Content type: jpeg passes, jpg passes, png passes, unsupported fails
  - Extension: .jpeg passes, .jpg passes, .png passes, unsupported fails

- [x] `RemoveOwnerProfileImageCommandHandlerTests.cs`
  - Removes from blob storage and clears owner
  - Throws NotFoundException when owner not found
  - Does not call blob storage when owner has no existing image

- [x] `CreatePetCommandHandlerTests.cs`
- [x] `UpdatePetCommandHandlerTests.cs`
- [x] `DeletePetCommandHandlerTests.cs`
- [x] `GetPetByIdQueryHandlerTests.cs`
- [x] `GetAllPetsQueryHandlerTests.cs`
- [x] `GetPetsByOwnerIdQueryHandlerTests.cs`

- [x] `CreatePetCommandValidatorTests.cs`
  - `Name`: valid, at max length, empty fails, null fails, whitespace fails, exceeds max length fails

- [x] `UpdatePetCommandValidatorTests.cs` — mirror same cases as Create

- [x] `AddPetImageCommandHandlerTests.cs`
  - Adds image successfully
  - Throws when max images exceeded
  - Throws NotFoundException when pet not found

- [x] `AddPetImageCommandValidatorTests.cs`
  - Content type: jpeg passes, jpg passes, png passes, unsupported fails
  - Extension: .jpeg passes, .jpg passes, .png passes, unsupported fails

- [x] `RemovePetImageCommandHandlerTests.cs`
  - Removes from blob storage and pet
  - Throws NotFoundException when pet not found

- [x] `ValidationBehaviorTests.cs`

### 7.3 Barkfest.Persistence.Tests

- [x] `Fixtures/DatabaseFixture.cs` — Testcontainers SQL Server, applies migrations via `MigrateAsync()`
- [x] `Repositories/OwnerRepositoryTests.cs`
- [x] `Repositories/PetRepositoryTests.cs`
- [x] `Configurations/OwnerConfigurationTests.cs`
- [x] `Configurations/PetConfigurationTests.cs`
- [x] `Configurations/PetImageConfigurationTests.cs`
- [x] `Configurations/BreedConfigurationTests.cs`

### 7.4 Barkfest.Infrastructure.Tests

- [x] `Fixtures/AzuriteFixture.cs` — Testcontainers Azurite
- [x] `Storage/AzureBlobStorageServiceTests.cs`

### 7.5 Barkfest.API.Tests

- [x] `Fixtures/ApiFactory.cs`
  - Extends `WebApplicationFactory<Program>`
  - Replaces SQL Server with Testcontainers SQL Server
  - Replaces Azure Blob Storage with Testcontainers Azurite
  - Both run in containers — no real external services

- [x] `Controllers/OwnersControllerTests.cs`
  - CRUD: GET 200, POST 201, PUT 200, DELETE 204, not found 404
  - Email: missing 400, invalid format 400, exceeds max length 400
  - FirstName: missing 400, exceeds max length 400
  - LastName: missing 400, exceeds max length 400
  - Profile image: valid upload 200, not found 404, unsupported content type 400, unsupported extension 400

- [x] `Controllers/PetsControllerTests.cs`
  - CRUD: GET 200, POST 201, PUT 200, DELETE 204, not found 404
  - Name: missing 400, exceeds max length 400
  - Profile image: valid upload 200, not found 404, unsupported content type 400
  - Gallery: add valid 200, max exceeded 400, remove valid 204, not found 404

### 7.6 Barkfest.Integration.Tests

References `Barkfest.API` — uses `WebApplicationFactory<Program>` with Testcontainers (SQL Server + Azurite). Fully self-contained; no running AppHost required.

- [x] `Config/IntegrationTestSettings.cs` — base URL and settings
- [x] `Flows/OwnerFlowTests.cs`
  - Full lifecycle: create → read → update → delete → confirm 404
  - Email: missing 400, invalid 400, exceeds max length 400
  - FirstName: missing 400, exceeds max length 400
  - LastName: missing 400, exceeds max length 400
  - Profile image: upload succeeds, unsupported type 400, remove succeeds

- [x] `Flows/PetFlowTests.cs`
  - Full lifecycle: create → read → update → delete → confirm 404
  - Name: missing 400, exceeds max length 400
  - Profile image: upload succeeds, unsupported type 400, remove succeeds
  - Gallery: add succeeds, max exceeded 400, remove succeeds

---

## Phase 8 — .NET Aspire (Local Dev Orchestration)

Scope: local development orchestration only. No deployment (`azd`, Azure Container Apps, or
Bicep) is included in this phase. Goal: any dev can clone the repo and be fully running in under
2 minutes (Docker and EF Core tools are documented prerequisites in the README).

### 8.1 Create Projects

- [x] Create `src/Barkfest.AppHost` — Aspire host project (`Microsoft.NET.Sdk`)
- [x] Create `src/Barkfest.ServiceDefaults` — Aspire defaults project (`Microsoft.NET.Sdk`)
- [x] Add `Barkfest.AppHost` → `Barkfest.API` project reference
- [x] Add `Barkfest.API` → `Barkfest.ServiceDefaults` project reference
- [x] Add NuGet packages per Phase 1 NuGet table

### 8.2 AppHost — `Program.cs`

Containers are **persistent** with **explicit named volumes**. Resource names and volume names
are hardcoded and project-scoped to prevent collisions when a developer has multiple Aspire
solutions running simultaneously.

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("barkfest-sql")
                 .WithLifetime(ContainerLifetime.Persistent)
                 .WithDataVolume("barkfest-sql-data");

var db = sql.AddDatabase("barkfest");

var storage = builder.AddAzureStorage("barkfest-storage")
                     .RunAsEmulator(e => e
                         .WithLifetime(ContainerLifetime.Persistent)
                         .WithDataVolume("barkfest-blobs-data"));

var blobs = storage.AddBlobs("barkfest-blobs");

builder.AddProject<Projects.Barkfest_API>("barkfest-api")
       .WithReference(db)
       .WithReference(blobs)
       .WaitFor(sql)
       .WaitFor(blobs);

builder.Build().Run();
```

**First `dotnet run`:** Aspire checks Docker — containers do not exist, creates them with stable
names `barkfest-sql` and `barkfest-storage`. The API starts, `MigrateAsync()` runs, schema is
created. Ready.

**Subsequent runs:** Aspire finds existing containers, starts them if stopped. All data is
intact. No manual steps.

**Stopping the AppHost:** containers remain in Docker with data preserved. Volumes
`barkfest-sql-data` and `barkfest-blobs-data` survive even a `docker rm`.

### 8.3 ServiceDefaults — `Extensions.cs`

Aspire scaffolds this file with an `AddServiceDefaults()` extension method providing:
- OpenTelemetry tracing and metrics (ASP.NET Core, HttpClient, runtime)
- Health check endpoints (`/health`, `/alive`)
- Service discovery

No custom code is added to this file — it is Aspire-generated boilerplate.

### 8.4 API Wiring Changes

**`Program.cs`** — add as the first line before `builder.Services` registrations:

```csharp
builder.AddServiceDefaults();
```

Remove the `if (IsDevelopment) builder.Configuration.AddUserSecrets<Program>()` call —
User Secrets are no longer used.

**`Barkfest.Persistence/DependencyInjection.cs`** — update connection string key name only:

```csharp
// Before (DefaultConnection key):
services.AddDbContext<AppDbContext>(opts =>
    opts.UseSqlServer(config.GetConnectionString("DefaultConnection")));

// After (barkfest key, standard EF Core AddDbContext):
services.AddDbContext<AppDbContext>(opts =>
    opts.UseSqlServer(config.GetConnectionString("barkfest")));
```

Standard `AddDbContext` is intentionally used instead of Aspire's `AddSqlServerDbContext`.
`WebApplicationFactory.ConfigureAppConfiguration` cannot reliably inject configuration with
the correct priority in .NET 10's minimal hosting model — see DECISIONS.md for details.
The Aspire AppHost still injects `ConnectionStrings__barkfest` at runtime (via `AddDatabase("barkfest")`); no Aspire
extension method is required for that injection to work.

**`Barkfest.Infrastructure/DependencyInjection.cs`** — switch to Aspire-aware registration:

```csharp
// Before:
services.AddSingleton(new BlobServiceClient(
    config.GetConnectionString("AzureBlobStorage")));

// After:
builder.AddAzureBlobServiceClient("barkfest-blobs");
```

`AddAzureBlobServiceClient` reads the Aspire-injected connection string and registers
`BlobServiceClient` with health checks and telemetry automatically.

> **`appsettings.json`** has no `ConnectionStrings` section. Aspire injects everything at
> runtime via environment variables. In production or CI, populate
> `ConnectionStrings__barkfest-sql` and `ConnectionStrings__barkfest-blobs` via environment
> variables or a secrets manager.

### 8.5 User Secrets

User Secrets are removed from `Barkfest.API`. Aspire injects all connection strings
automatically when running through the AppHost. No manual secret configuration is required
after cloning.

### 8.6 Test Projects

All six test projects remain **completely unchanged**. Aspire is not referenced by any test project:
- Unit tests have no connection string dependency
- `Barkfest.Persistence.Tests` uses `ModelHelper` (no live connection)
- `Barkfest.Infrastructure.Tests` manages its own Azurite via Testcontainers
- `Barkfest.API.Tests` manages its own SQL Server + Azurite via Testcontainers
- `Barkfest.Integration.Tests` uses `WebApplicationFactory<Program>` with its own Testcontainers

### 8.7 Running Locally

```bash
# Clone and run — that's it
dotnet run --project src/Barkfest.AppHost
```

Aspire dashboard opens automatically (typically `https://localhost:15888`) showing live logs,
traces, and health for all resources. The API URL is listed there.

---

## Phase 9 — API Refinements

### 9.1 Route Prefix
- **Decision:** Use `v1/` prefix — meaningful, signals versioning intent, forward-thinking
- [x] `OwnersController`: `[Route("api/owners")]` → `[Route("v1/owners")]`
- [x] `PetsController`: `[Route("api/pets")]` → `[Route("v1/pets")]` and `[Route("api/owners/{ownerId:guid}/pets")]` → `[Route("v1/owners/{ownerId:guid}/pets")]`
- [x] Update all URL strings in `Barkfest.API.Tests` and `Barkfest.Integration.Tests`

### 9.2 Controller Class Names
- **Decision:** Singular — class name represents the resource type, not the collection; plural belongs in the route
- [x] Rename `OwnersController.cs` → `OwnerController.cs`
- [x] Rename `PetsController.cs` → `PetController.cs`
- [x] Update all references in `Barkfest.API.Tests`

---

## Phase 10 — Test Refinements

### 10.1 Test Naming Convention

**Pattern:**
- Happy path / response tests: `[Method]_When_[Condition]_Returns_[Result]`
- Exception tests: `[Method]_When_[Condition]_Throws_[ExceptionType]`

**Rules:**
- Use descriptive words for HTTP status codes — never raw numbers:
  - `200` → `Ok`
  - `201` → `Created`
  - `204` → `NoContent`
  - `400` → `BadRequest`
  - `404` → `NotFound`
  - `500` → `InternalServerError`
- `When_` is required — no skipping the condition clause
- Result must describe behaviour, not implementation detail (e.g. `Returns_NotFound` not `Returns404`)

**Applies to all six test projects:**
- [x] `Barkfest.Domain.Tests`
- [x] `Barkfest.Application.Tests`
- [x] `Barkfest.Infrastructure.Tests`
- [x] `Barkfest.Persistence.Tests`
- [x] `Barkfest.API.Tests`
- [x] `Barkfest.Integration.Tests`

### 10.2 Test Data Builder Pattern

- [x] Create `tests/Barkfest.Tests.Common/Barkfest.Tests.Common.csproj`
  - References `Barkfest.Domain` only
  - Inherits `tests/Directory.Build.props` (net10.0, ImplicitUsings, Nullable)
- [x] Create `tests/Barkfest.Tests.Common/Builders/OwnerBuilder.cs`
  - Defaults: `FirstName="Test"`, `LastName="Owner"`, unique guid-based email
  - Fluent methods: `WithFirstName`, `WithLastName`, `WithEmail`, `WithPhoneNumber`, `WithProfileImage`
- [x] Create `tests/Barkfest.Tests.Common/Builders/PetBuilder.cs`
  - Defaults: `OwnerId=NewGuid`, `Name="Buddy"`, `PetType=Dog`
  - Fluent methods: `WithOwnerId`, `WithName`, `WithDescription`, `WithDateOfBirth`, `WithPetType`, `WithProfileImage`, `WithImage`
- [x] Create `tests/Barkfest.Tests.Common/Builders/PetImageBuilder.cs`
  - Defaults: `BlobName="pets/test/gallery/photo.jpg"`, `ContentType="image/jpeg"`, `DisplayOrder=0`
  - Fluent methods: `WithBlobName`, `WithContentType`, `WithDisplayOrder`
- [x] Add `Barkfest.Tests.Common` project reference to `Barkfest.Domain.Tests`
- [x] Add `Barkfest.Tests.Common` project reference to `Barkfest.Application.Tests`
- [x] Add `GlobalUsings.cs` to `Barkfest.Domain.Tests` and `Barkfest.Application.Tests`
  — `global using Barkfest.Tests.Common.Builders;`
- [x] Replace all private `BuildXxx()` helpers in `Domain.Tests` and `Application.Tests`
  with builder calls

---

## Phase 11 — Authentication & Authorization ✅ Complete

### Decisions

| Decision | Choice |
|---|---|
| Registration | `POST /v1/auth/register` — creates Owner + stores PasswordHash in one step |
| Token strategy | Access token only (no refresh tokens) |
| Identity approach | Custom JWT — no ASP.NET Core Identity |
| Authorization model | Owners manage only their own data; ownership checked in handlers |
| `GetAll` endpoints | Removed (`GET /v1/owners`, `GET /v1/pets`) — no admin role, no use case |

### New NuGet Packages

| Project | Package |
|---|---|
| `Barkfest.API` | `Microsoft.AspNetCore.Authentication.JwtBearer` |
| `Barkfest.Infrastructure` | `BCrypt.Net-Next`, `System.IdentityModel.Tokens.Jwt` |
| `Barkfest.Tests.Common` | `Microsoft.IdentityModel.Tokens`, `System.IdentityModel.Tokens.Jwt` |

### Domain

- [x] `Owner.PasswordHash` property + `SetPasswordHash(string hash)` method
- [x] `IOwnerRepository.GetByEmailAsync(string email, CancellationToken)` — returns `Owner?`
- [x] `ForbiddenException` (maps to 403 via `ExceptionHandlingMiddleware`)

### Application

- [x] `ICurrentUserService` — `Guid OwnerId { get; }`
- [x] `IJwtTokenService` — `string GenerateToken(Owner owner)`, `DateTime GetExpiry()`
- [x] `IPasswordHasher` — `string Hash(string password)`, `bool Verify(string password, string hash)`
- [x] `AuthTokenDto(string AccessToken, Guid OwnerId, DateTime ExpiresAt)`
- [x] `RegisterCommand` / `RegisterCommandHandler` / `RegisterCommandValidator`
  - `Password`: not empty, min `AccountConstraints.PasswordMinLength` (10), max `AccountConstraints.PasswordMaxLength` (72) — BCrypt silently truncates beyond 72; no complexity rules enforced (see DECISIONS.md)
- [x] `LoginCommand` / `LoginCommandHandler` / `LoginCommandValidator`
- [x] All owner + pet handlers updated with `ICurrentUserService` ownership check → `ForbiddenException`
- [x] `CreatePetCommand` — `OwnerId` removed; handler reads from `ICurrentUserService`

### Persistence

- [x] `OwnerConfiguration` — `PasswordHash` column (required), unique index on `Email`
- [x] `OwnerRepository.GetByEmailAsync` implemented
- [x] Migration `AddOwnerPasswordHash` generated

### Infrastructure

- [x] `JwtSettings` — `SecretKey`, `Issuer`, `Audience`, `ExpiryMinutes`
- [x] `JwtTokenService` implementing `IJwtTokenService`
- [x] `BcryptPasswordHasher` implementing `IPasswordHasher`
- [x] `DependencyInjection` updated

### API

- [x] `AuthController` — `POST /v1/auth/register` (201), `POST /v1/auth/login` (200) — `[AllowAnonymous]`
- [x] `OwnerController` / `PetController` — `[Authorize]` added, `GetAll` endpoints removed
- [x] `CreatePetRequest` — `OwnerId` field removed
- [x] `CurrentUserService` — reads `sub` claim via `IHttpContextAccessor`
- [x] `Program.cs` — `AddJwtBearer` with `MapInboundClaims = false` (required for .NET 10's `JsonWebTokenHandler`)
- [x] `ExceptionHandlingMiddleware` — `ForbiddenException` → 403
- [x] `appsettings.json` / `appsettings.Development.json` / `appsettings.Testing.json`

### Tests (650 total — all passing)

- [x] `JwtTestHelper.GenerateToken(Guid ownerId)` in `Barkfest.Tests.Common`
- [x] `CreateAuthenticatedClient(Guid ownerId)` in `BarkfestApiFactory` and `IntegrationApiFactory`
- [x] `AuthControllerTests` — 6 tests
- [x] `OwnersControllerTests` / `PetsControllerTests` — rewritten with auth, 401/403 tests added
- [x] `OwnerLifecycleTests` / `PetLifecycleTests` — register via `/v1/auth/register`, all requests authenticated
- [x] All Application.Tests handler tests — `ICurrentUserService` mocked, `ForbiddenException` tests added
- [x] Auth command/validator tests — `RegisterCommandHandlerTests`, `RegisterCommandValidatorTests`, `LoginCommandHandlerTests`, `LoginCommandValidatorTests`

### Key Implementation Note

.NET 10's `JwtBearerHandler` uses `JsonWebTokenHandler` by default. The common advice of calling
`JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear()` does **not** work — it only affects
`JwtSecurityTokenHandler`. The correct fix is `options.MapInboundClaims = false` in `AddJwtBearer()`.

---

## Phase 12 — Frontend Scaffold ✅ Complete

### Decisions

| Decision | Choice |
|---|---|
| Build tool | Vite + React + TypeScript |
| Package manager | pnpm |
| Routing | React Router v7 (library mode) |
| Server state | TanStack Query v5 |
| Styling | Tailwind CSS v4 via `@tailwindcss/vite` |
| Components | shadcn/ui (base-ui headless primitives) |
| Token storage | HttpOnly cookies — CORS wired here; cookie login/logout in Phase 13 |
| Folder structure | Feature-based under `src/` |
| Aspire integration | `AddViteApp` + `WithPnpm` — Aspire injects `VITE_API_BASE_URL` |
| Testing | Vitest + React Testing Library |

### Structure

```
barkfest-ui/
├── src/
│   ├── components/ui/    ← shadcn/ui generated components
│   ├── features/         ← auth, owners, pets feature folders (Phase 13+)
│   ├── hooks/
│   │   └── useAuth.ts
│   ├── layouts/
│   │   └── ShellLayout.tsx
│   ├── lib/
│   │   └── api.ts        ← typed fetch wrappers (credentials: 'include')
│   ├── pages/            ← LoginPage, RegisterPage, OwnersPage, PetsPage
│   ├── test/
│   │   └── setup.ts      ← jest-dom matchers
│   ├── App.tsx
│   └── main.tsx
├── .env.example
├── .gitignore
├── pnpm-workspace.yaml   ← allowBuilds: msw (pnpm 11 requirement)
├── vite.config.ts        ← test block included (Vitest config co-located)
├── tsconfig.app.json
└── package.json
```

### Scripts

| Command | Purpose |
|---|---|
| `pnpm dev` | Vite dev server (run via Aspire, not directly) |
| `pnpm build` | Production build → `dist/` |
| `pnpm test` | Vitest single-pass run — CI-safe, exit 0 with no tests |
| `pnpm test:watch` | Interactive watch mode |
| `pnpm test:ui` | Vitest browser UI |

### .NET API changes

- CORS policy `BarkfestUI` added — `AllowCredentials()`, `AllowAnyHeader()`, `AllowAnyMethod()`
- Origin read from `Cors:AllowedOrigin` config key (defaults to `http://localhost:5173`)
- `appsettings.Development.json` — `"Cors": { "AllowedOrigin": "http://localhost:5173" }`
- `app.UseCors("BarkfestUI")` inserted before `UseAuthentication()`

### Aspire changes

- `Aspire.Hosting.JavaScript` v13.3.4 added to `Barkfest.AppHost.csproj`
- `AppHost.cs` registers `barkfest-ui` resource with `VITE_API_BASE_URL` injected from API endpoint

---

## Phase 13 — Deployment Pipeline

### Azure Resources (`infra/main.bicep`)

| Resource | Type | Purpose |
|---|---|---|
| Resource Group | `Microsoft.Resources/resourceGroups` | Logical container for all Barkfest resources |
| Azure Container Registry | `Microsoft.ContainerRegistry/registries` | Stores Docker images built by GitHub Actions |
| Container Apps Environment | `Microsoft.App/managedEnvironments` | Shared hosting environment for Container Apps |
| Container App | `Microsoft.App/containerApps` | Hosts the .NET 10 API |
| SQL Server + Database | `Microsoft.Sql/servers` + `databases` | Production SQL Server (Basic SKU) |
| Storage Account + Blob Container | `Microsoft.Storage/storageAccounts` | Production Blob Storage (`barkfest-blobs` container) |
| Application Insights | `Microsoft.Insights/components` | Telemetry |
| Log Analytics Workspace | `Microsoft.OperationalInsights/workspaces` | Backend for Application Insights + container logs |
| Static Web App | `Microsoft.Web/staticSites` | Hosts the React frontend (Free SKU) |

Provisioning command:
```bash
az deployment sub create \
  --name barkfest-resources \
  --location centralus \
  --template-file infra/main.bicep \
  --parameters sqlAdminLogin=<username> sqlAdminPassword=<password>
```

### GitHub Secrets

18 secrets stored in GitHub repository Settings → Secrets and variables → Actions:

`AZURE_CREDENTIALS`, `CONTAINER_APP_NAME`, `REGISTRY_LOGIN_SERVER`, `REGISTRY_USERNAME`,
`REGISTRY_PASSWORD`, `API_URL`, `STATIC_WEB_APP_NAME`, `AZURE_STATIC_WEB_APPS_API_TOKEN`,
`SQL_CONNECTION_STRING`, `BLOB_CONNECTION_STRING`, `APPINSIGHTS_CONNECTION_STRING`,
`JWT_SECRET_KEY`, `ADMIN_USERNAME`, `ADMIN_NAME`, `ADMIN_EMAIL`, `ADMIN_PHONE_NUMBER`,
`ADMIN_PASSWORD`, `CORS_ALLOWED_ORIGIN`

### API Pipeline (`.github/workflows/api.yml`)

Triggers on push to `main`. Steps: checkout → setup .NET 10 → restore → build → test → Docker login → Docker build + push to ACR (tagged with `github.sha` and `latest`) → `az containerapp update` with new image and env vars.

### Frontend Pipeline (`.github/workflows/ui.yml`)

Triggers on push to `main`. Steps: checkout → setup Node + pnpm → `pnpm install` → `pnpm test` → `pnpm build` (with `VITE_API_BASE_URL` from `API_URL` secret) → deploy `dist/` to Azure Static Web Apps.

---

## Phase 14 — Browse API Enhancements

### Goal

Extend the public browse API to support server-side pagination, featured-image-only filtering, and two new dropdown hydration endpoints.

### Scope

| # | Change | Layer |
|---|---|---|
| 1 | `PagedResult<T>` generic wrapper | Application |
| 2 | Featured-image-only filter on `GetBrowseImagesAsync` | Persistence |
| 3 | Server-side pagination on `GetBrowseImagesAsync` | Persistence + Application |
| 4 | Breed filter pushed to DB (`EF.Property` on TPH column) | Persistence |
| 5 | `GET /v1/browse/pet-types` — returns SmartEnum values | Application + API |
| 6 | `GET /v1/browse/breeds?petType=Dog` — returns breed names | Application + API |
| 7 | Tests for all new/modified behaviour | Application.Tests + Persistence.Tests + API.Tests |

### Step 1 — `PagedResult<T>`

- `Barkfest.Application/Common/Models/PagedResult.cs`
- `record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)`
- Computed `bool HasMore => Page * PageSize < TotalCount`

### Step 2 — Update `IBrowseRepository`

- Return type: `PagedResult<BrowseImageDto>` (was `IEnumerable<BrowseImageDto>`)
- New params: `int page, int pageSize`

### Step 3 — Update `BrowseRepository`

- Add `.Where(pi => pi.IsFeaturedImage)` — one card per pet
- Move breed filter to DB using `EF.Property<int>(pi.Pet.Breed, "BreedValue")`
- `CountAsync` + `Skip/Take` for pagination
- Return `PagedResult<BrowseImageDto>`

### Step 4 — Update `GetBrowseImagesQuery`

- Add `int Page, int PageSize` params; return type `PagedResult<BrowseImageDto>`
- Unknown `petType` returns `PagedResult` with empty items

### Step 5 — `GetBrowsePetTypesQuery`

- Handler reads from `PetType.List` — no DB call
- Returns `IReadOnlyList<string>`

### Step 6 — `GetBrowseBreedsQuery`

- Handler reads from `DogBreed.List` or `CatBreed.List`, ordered by `SmartEnum.Value`
- Unknown `petType` returns empty list

### Step 7 — Update `BrowseController`

- `GetImages`: add `page` (default 1) and `pageSize` (default 6) query params
- New `GET /v1/browse/pet-types`
- New `GET /v1/browse/breeds?petType=`

### Step 8 — Tests

- `GetBrowseImagesQueryHandlerTests` — updated for new signature and `PagedResult` return type
- `GetBrowsePetTypesQueryHandlerTests` — new
- `GetBrowseBreedsQueryHandlerTests` — new
- `BrowseRepositoryTests` — ordering, featured filter, pagination
- `BrowseControllerTests` — updated for paged response shape, new endpoint tests

---

## Phase 15 — Handoff Home Page

Per-feature plan, progress, and decisions docs were removed in PR #16 (repo housekeeping).
See [PROGRESS.md](PROGRESS.md) for the completed phase summary.

---

## Phase 16 — Home Page Wire Filter

Per-feature plan, progress, and decisions docs were removed in PR #16 (repo housekeeping).
See [PROGRESS.md](PROGRESS.md) for the completed phase summary.

---

## General Rules — Always Follow These

- Target framework: `.NET 10`
- All primary keys are `Guid` using `Guid.CreateVersion7()` for application-side generation
- SQL Server uses `newsequentialid()` as the database-level default to prevent index fragmentation
- All primary key `Id` properties map to `{EntityName}Id` DB columns via `HasColumnName()`
- **Use `sealed record` for:** Value Objects (`ProfileImage`)
- **Use `record` for:** DTOs, Commands, Queries
- **Use `class` for:** Entities, Handlers, Validators, Repositories, Services, Configurations, DbContext
- No AutoMapper — manual static extension methods for all object mapping
- No Moq — NSubstitute for all mocking
- No FluentAssertions — Shouldly for all assertions
- No Swagger/Swashbuckle — Scalar for API documentation
- All handlers implement MediatR `IRequestHandler<TRequest, TResponse>`
- All validators extend FluentValidation `AbstractValidator<T>`
- Repository interfaces defined in `Barkfest.Domain`, implemented in `Barkfest.Persistence`
- `IBlobStorageService` defined in `Barkfest.Application`, implemented in `Barkfest.Infrastructure`
- `SupportedImageType` enforced at both Domain (`ProfileImage.Create()`, `PetImage.SetImage()`) and Application (validators) layers
- `Pet.MaxImages` constant used in all image limit tests — never hardcode the number `6`
- `Owner.FirstNameMaxLength`, `Owner.LastNameMaxLength`, `Owner.EmailMaxLength`, `Pet.NameMaxLength` constants used in all length-related tests — never hardcode the numbers
- Each `src` project has its own `DependencyInjection.cs` with a self-registering extension method
- Connection strings injected by Aspire for local dev; populated via environment variables or secrets manager in production and CI — never committed to source control
- Testcontainers used for all integration tests — both SQL Server (`Testcontainers.MsSql`) and Azure Blob Storage (`Testcontainers.Azurite`) run in containers, no real external services in any test project
- `Age` is computed from `DateOfBirth` at runtime — never stored in the database
- Cascade deletes: `Owner` → `Pets`, `Pet` → `PetImages`, `Pet` → `Breeds`
- `ExceptionHandlingMiddleware` maps `NotFoundException` → 404, `DomainException` → 400, unhandled → 500
- Migration applied at runtime via `MigrateAsync()` on startup — never run `dotnet ef database update`
