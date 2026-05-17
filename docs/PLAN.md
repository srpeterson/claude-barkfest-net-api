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
│   └── Barkfest.API
└── tests/
    ├── Barkfest.Domain.Tests
    ├── Barkfest.Application.Tests
    ├── Barkfest.Persistence.Tests
    ├── Barkfest.Infrastructure.Tests
    ├── Barkfest.API.Tests
    └── Barkfest.Integration.Tests
```

---

## Phase 1 — Solution Scaffold

- [ ] Create solution file `Barkfest.sln`
- [ ] Create all 13 projects with correct project types and target framework `net10.0`
- [ ] Add all project references as defined below
- [ ] Add all NuGet packages as defined below
- [ ] Create `.gitignore` appropriate for a .NET solution

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

Barkfest.Domain.Tests         → Barkfest.Domain
Barkfest.Application.Tests    → Barkfest.Application
Barkfest.Persistence.Tests    → Barkfest.Persistence
Barkfest.Infrastructure.Tests → Barkfest.Infrastructure
Barkfest.API.Tests            → Barkfest.API
Barkfest.Integration.Tests    → (none — talks to running app over HTTP)
```

### NuGet Packages

| Project | Packages |
|---|---|
| `Barkfest.AppHost` | `Aspire.Hosting.AppHost`, `Aspire.Hosting.SqlServer`, `Aspire.Hosting.Azure.Storage` |
| `Barkfest.ServiceDefaults` | `Microsoft.Extensions.ServiceDiscovery`, `OpenTelemetry.Exporter.OpenTelemetryProtocol`, `OpenTelemetry.Extensions.Hosting`, `OpenTelemetry.Instrumentation.AspNetCore`, `OpenTelemetry.Instrumentation.Http`, `OpenTelemetry.Instrumentation.Runtime` |
| `Barkfest.Domain` | `Ardalis.SmartEnum` |
| `Barkfest.Application` | `MediatR`, `FluentValidation` |
| `Barkfest.Persistence` | `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Tools`, `Aspire.Microsoft.EntityFrameworkCore.SqlServer` |
| `Barkfest.Infrastructure` | `Azure.Storage.Blobs`, `Aspire.Azure.Storage.Blobs` |
| `Barkfest.API` | `Scalar.AspNetCore`, `Serilog.AspNetCore` |
| `*.Tests` (unit) | `xunit`, `Shouldly`, `NSubstitute` |
| `*.Tests` (integration) | above + `Testcontainers.MsSql`, `Testcontainers.Azurite` |
| `Barkfest.API.Tests` | above + `Microsoft.AspNetCore.Mvc.Testing` |
| `Barkfest.Integration.Tests` | `xunit`, `Shouldly`, `Testcontainers.MsSql`, `Testcontainers.Azurite` |

---

## Phase 2 — Domain Layer

### 2.1 Exceptions

- [ ] Create `Barkfest.Domain/Exceptions/DomainException.cs`

### 2.2 Value Objects — use `sealed record`

- [ ] Create `Barkfest.Domain/ValueObjects/ProfileImage.cs`
  - Properties: `BlobName` (string), `ContentType` (string)
  - Private constructor, static `Create()` factory method
  - Validates: `BlobName` required, `ContentType` required and validated via `SupportedImageType`
  - Trims `BlobName`, lowercases and trims `ContentType`
  - Record provides structural equality automatically — no manual `Equals`/`GetHashCode` needed

### 2.3 Static Classes

- [ ] Create `Barkfest.Domain/ValueObjects/SupportedImageType.cs`
  - `AllowedContentTypes`: `image/jpeg`, `image/jpg`, `image/png`
  - `AllowedExtensions`: `.jpeg`, `.jpg`, `.png`
  - `IsAllowedContentType(string contentType)` — case insensitive
  - `IsAllowedExtension(string fileName)` — case insensitive

### 2.4 SmartEnums — extend `SmartEnum<T>`

- [ ] Create `Barkfest.Domain/Enums/PetType.cs`
  - `Dog` (1), `Cat` (2), `Other` (3)

- [ ] Create `Barkfest.Domain/Enums/DogBreed.cs`
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

- [ ] Create `Barkfest.Domain/Enums/CatBreed.cs`
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

- [ ] Create `Barkfest.Domain/Entities/Owner.cs`
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

- [ ] Create `Barkfest.Domain/Entities/Breed.cs` (abstract base)
  - Properties: `Id` (Guid), `PetId` (Guid), `Pet`
  - `Id` initialised with `Guid.CreateVersion7()`

- [ ] Create `Barkfest.Domain/Entities/DogBreedInfo.cs` (extends `Breed`)
  - Properties: `DogBreed` (SmartEnum)
  - `SetDogBreed(DogBreed)` — null throws `DomainException`

- [ ] Create `Barkfest.Domain/Entities/CatBreedInfo.cs` (extends `Breed`)
  - Properties: `CatBreed` (SmartEnum)
  - `SetCatBreed(CatBreed)` — null throws `DomainException`

- [ ] Create `Barkfest.Domain/Entities/PetImage.cs`
  - Properties: `Id` (Guid), `PetId` (Guid), `Pet`, `BlobName`, `ContentType`, `DisplayOrder`, `CreatedAt`
  - `Id` initialised with `Guid.CreateVersion7()`
  - `CreatedAt` initialised with `DateTime.UtcNow`
  - Constants:
    - `public const int BlobNameMaxLength = 500`
    - `public const int ContentTypeMaxLength = 100`
  - Methods:
    - `SetImage(string blobName, string contentType)` — validates via `SupportedImageType`
    - `SetDisplayOrder(int order)` — must be zero or greater

- [ ] Create `Barkfest.Domain/Entities/Pet.cs`
  - Properties: `Id` (Guid), `Name`, `Description` (nullable), `DateOfBirth` (nullable `DateOnly`),
    `PetType`, `Breed` (nullable), `ProfileImage` (nullable `ProfileImage` value object),
    `Images` (`IReadOnlyCollection<PetImage>`), `OwnerId` (Guid), `Owner`, `CreatedAt`
  - `Id` initialised with `Guid.CreateVersion7()`
  - `CreatedAt` initialised with `DateTime.UtcNow`
  - Computed property: `Age` (nullable `int`, calculated from `DateOfBirth`, never stored in DB)
  - Constants:
    - `public const int NameMaxLength = 75`
    - `public const int MaxImages = 5`
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
    - `AddImage(PetImage)` — null throws, enforces max 5 images via `MaxImages` constant
    - `RemoveImage(Guid petImageId)` — not found throws `DomainException`

### 2.6 Interfaces

- [ ] Create `Barkfest.Domain/Interfaces/IOwnerRepository.cs`
  - `GetByIdAsync(Guid id, CancellationToken)`
  - `GetAllAsync(CancellationToken)`
  - `AddAsync(Owner, CancellationToken)`
  - `UpdateAsync(Owner, CancellationToken)`
  - `DeleteAsync(Guid id, CancellationToken)`

- [ ] Create `Barkfest.Domain/Interfaces/IPetRepository.cs`
  - `GetByIdAsync(Guid id, CancellationToken)`
  - `GetAllAsync(CancellationToken)`
  - `GetByOwnerIdAsync(Guid ownerId, CancellationToken)`
  - `AddAsync(Pet, CancellationToken)`
  - `UpdateAsync(Pet, CancellationToken)`
  - `DeleteAsync(Guid id, CancellationToken)`

- [ ] Create `Barkfest.Domain/Interfaces/IUnitOfWork.cs`
  - `SaveChangesAsync(CancellationToken)`

---

## Phase 3 — Application Layer

### 3.1 Common

- [ ] Create `Barkfest.Application/Common/Exceptions/NotFoundException.cs`
- [ ] Create `Barkfest.Application/Common/Interfaces/IBlobStorageService.cs`
  - `UploadAsync(string containerName, string blobName, Stream content, string contentType, CancellationToken)`
  - `DownloadAsync(string containerName, string blobName, CancellationToken)`
  - `DeleteAsync(string containerName, string blobName, CancellationToken)`
  - `ExistsAsync(string containerName, string blobName, CancellationToken)`
- [ ] Create `Barkfest.Application/Common/Behaviors/ValidationBehavior.cs`
- [ ] Create `Barkfest.Application/Common/Behaviors/LoggingBehavior.cs`

### 3.2 DTOs — use `record`

- [ ] Create `Barkfest.Application/Features/Owners/DTOs/OwnerDto.cs` (record)
  - `Guid Id`, `string FirstName`, `string LastName`, `string Email`,
    `string? PhoneNumber`, `ProfileImageDto? ProfileImage`, `DateTime CreatedAt`

- [ ] Create `Barkfest.Application/Features/Pets/DTOs/PetDto.cs` (record)
  - `Guid Id`, `string Name`, `string? Description`, `DateOnly? DateOfBirth`,
    `int? Age`, `string PetType`, `string? Breed`, `ProfileImageDto? ProfileImage`,
    `IReadOnlyCollection<PetImageDto> Images`, `Guid OwnerId`, `DateTime CreatedAt`

- [ ] Create `Barkfest.Application/Features/Pets/DTOs/PetImageDto.cs` (record)
  - `Guid Id`, `string BlobName`, `string ContentType`, `int DisplayOrder`, `DateTime CreatedAt`

- [ ] Create `Barkfest.Application/Features/Pets/DTOs/ProfileImageDto.cs` (record)
  - `string BlobName`, `string ContentType`

### 3.3 Mappings — static extension methods, no AutoMapper

- [ ] Create `Barkfest.Application/Features/Owners/OwnerMappings.cs`
  - `ToDto(this Owner)` → `OwnerDto`
  - `ToDtoList(this IEnumerable<Owner>)` → `IEnumerable<OwnerDto>`

- [ ] Create `Barkfest.Application/Features/Pets/PetMappings.cs`
  - `ToDto(this Pet)` → `PetDto`
  - `ToDtoList(this IEnumerable<Pet>)` → `IEnumerable<PetDto>`

### 3.4 Owner Commands and Queries — use `record` for commands/queries, `class` for handlers/validators

- [ ] `CreateOwner/CreateOwnerCommand.cs` (record) — `IRequest<Guid>`
- [ ] `CreateOwner/CreateOwnerCommandHandler.cs` (class)
- [ ] `CreateOwner/CreateOwnerCommandValidator.cs` (class)
  - `FirstName`: not empty, max length via `Owner.FirstNameMaxLength`
  - `LastName`: not empty, max length via `Owner.LastNameMaxLength`
  - `Email`: not empty, valid email format, max length via `Owner.EmailMaxLength`

- [ ] `UpdateOwner/UpdateOwnerCommand.cs` (record) — `IRequest`
- [ ] `UpdateOwner/UpdateOwnerCommandHandler.cs` (class)
- [ ] `UpdateOwner/UpdateOwnerCommandValidator.cs` (class) — same rules as Create

- [ ] `DeleteOwner/DeleteOwnerCommand.cs` (record) — `IRequest`
- [ ] `DeleteOwner/DeleteOwnerCommandHandler.cs` (class)

- [ ] `UploadOwnerProfileImage/UploadOwnerProfileImageCommand.cs` (record)
- [ ] `UploadOwnerProfileImage/UploadOwnerProfileImageCommandHandler.cs` (class)
- [ ] `UploadOwnerProfileImage/UploadOwnerProfileImageCommandValidator.cs` (class)
  - `ContentType`: must pass `SupportedImageType.IsAllowedContentType()`
  - `FileName`: must pass `SupportedImageType.IsAllowedExtension()`

- [ ] `RemoveOwnerProfileImage/RemoveOwnerProfileImageCommand.cs` (record)
- [ ] `RemoveOwnerProfileImage/RemoveOwnerProfileImageCommandHandler.cs` (class)
  - Only calls `IBlobStorageService.DeleteAsync()` if owner has an existing image

- [ ] `GetOwnerById/GetOwnerByIdQuery.cs` (record) — `IRequest<OwnerDto>`
- [ ] `GetOwnerById/GetOwnerByIdQueryHandler.cs` (class)

- [ ] `GetAllOwners/GetAllOwnersQuery.cs` (record) — `IRequest<IEnumerable<OwnerDto>>`
- [ ] `GetAllOwners/GetAllOwnersQueryHandler.cs` (class)

### 3.5 Pet Commands and Queries

- [ ] `CreatePet/CreatePetCommand.cs` (record) — `IRequest<Guid>`
- [ ] `CreatePet/CreatePetCommandHandler.cs` (class)
- [ ] `CreatePet/CreatePetCommandValidator.cs` (class)
  - `Name`: not empty, max length via `Pet.NameMaxLength`

- [ ] `UpdatePet/UpdatePetCommand.cs` (record) — `IRequest`
- [ ] `UpdatePet/UpdatePetCommandHandler.cs` (class)
- [ ] `UpdatePet/UpdatePetCommandValidator.cs` (class) — same rules as Create

- [ ] `DeletePet/DeletePetCommand.cs` (record) — `IRequest`
- [ ] `DeletePet/DeletePetCommandHandler.cs` (class)

- [ ] `UploadPetProfileImage/UploadPetProfileImageCommand.cs` (record)
- [ ] `UploadPetProfileImage/UploadPetProfileImageCommandHandler.cs` (class)
- [ ] `UploadPetProfileImage/UploadPetProfileImageCommandValidator.cs` (class)
  - Same image type validation as Owner

- [ ] `RemovePetProfileImage/RemovePetProfileImageCommand.cs` (record)
- [ ] `RemovePetProfileImage/RemovePetProfileImageCommandHandler.cs` (class)

- [ ] `AddPetImage/AddPetImageCommand.cs` (record)
- [ ] `AddPetImage/AddPetImageCommandHandler.cs` (class)
  - Enforces `Pet.MaxImages` limit
- [ ] `AddPetImage/AddPetImageCommandValidator.cs` (class)
  - Same image type validation as Owner

- [ ] `RemovePetImage/RemovePetImageCommand.cs` (record)
- [ ] `RemovePetImage/RemovePetImageCommandHandler.cs` (class)

- [ ] `GetPetById/GetPetByIdQuery.cs` (record) — `IRequest<PetDto>`
- [ ] `GetPetById/GetPetByIdQueryHandler.cs` (class)

- [ ] `GetAllPets/GetAllPetsQuery.cs` (record) — `IRequest<IEnumerable<PetDto>>`
- [ ] `GetAllPets/GetAllPetsQueryHandler.cs` (class)

- [ ] `GetPetsByOwnerId/GetPetsByOwnerIdQuery.cs` (record) — `IRequest<IEnumerable<PetDto>>`
- [ ] `GetPetsByOwnerId/GetPetsByOwnerIdQueryHandler.cs` (class)

### 3.6 Dependency Injection

- [ ] Create `Barkfest.Application/DependencyInjection.cs`
  - `AddApplication()` extension method
  - Registers MediatR with `ValidationBehavior` and `LoggingBehavior`
  - Registers FluentValidation validators

---

## Phase 4 — Persistence Layer

### 4.1 DbContext

- [ ] Create `Barkfest.Persistence/AppDbContext.cs`
  - `DbSet<Owner> Owners`
  - `DbSet<Pet> Pets`
  - `DbSet<PetImage> PetImages`
  - `DbSet<Breed> Breeds`

### 4.2 EF Core Configurations

> **Column naming rule:** All primary key `Id` properties map to `{EntityName}Id`
> in the database via `HasColumnName()`. This ensures raw SQL and Dapper queries
> are self-describing in joins.

- [ ] Create `Barkfest.Persistence/Configurations/OwnerConfiguration.cs`
  - `Id` → column `OwnerId`, default `newsequentialid()`
  - `FirstName` → `nvarchar(50)`, not null
  - `LastName` → `nvarchar(100)`, not null
  - `Email` → `nvarchar(75)`, not null
  - `PhoneNumber` → `nvarchar(max)`, nullable
  - `OwnsOne(ProfileImage)` → columns `ProfileImageBlobName` nvarchar(500), `ProfileImageContentType` nvarchar(100), both nullable

- [ ] Create `Barkfest.Persistence/Configurations/PetConfiguration.cs`
  - `Id` → column `PetId`, default `newsequentialid()`
  - `OwnerId` → FK → `Owners.OwnerId`, cascade delete
  - `Name` → `nvarchar(75)`, not null
  - `Description` → `nvarchar(max)`, nullable
  - `DateOfBirth` → `date`, nullable
  - `PetType` → `int`, not null, SmartEnum conversion (`pt.Value` / `PetType.FromValue()`)
  - `OwnsOne(ProfileImage)` → columns `ProfileImageBlobName` nvarchar(500), `ProfileImageContentType` nvarchar(100), both nullable
  - `Ignore(p => p.Age)` — computed, not stored
  - One-to-many with `Owner` via `HasOne(...).WithMany(o => o.Pets)`

- [ ] Create `Barkfest.Persistence/Configurations/PetImageConfiguration.cs`
  - `Id` → column `PetImageId`, default `newsequentialid()`
  - `PetId` → column `PetId`, FK → `Pets.PetId`, cascade delete
  - `BlobName` → `nvarchar(500)`, not null
  - `ContentType` → `nvarchar(100)`, not null
  - `DisplayOrder` → `int`, not null

- [ ] Create `Barkfest.Persistence/Configurations/BreedConfiguration.cs`
  - `Id` → column `BreedId`, default `newsequentialid()`
  - `PetId` → column `PetId`, FK → `Pets.PetId`, cascade delete
  - TPH discriminator column `BreedType` nvarchar(50): `"Dog"` → `DogBreedInfo`, `"Cat"` → `CatBreedInfo`
  - `DogBreedInfo.DogBreed` → column `BreedValue`, SmartEnum int conversion, nullable
  - `CatBreedInfo.CatBreed` → column `BreedValue`, SmartEnum int conversion, nullable
  - One-to-one with `Pet` via `HasOne(...).WithOne(p => p.Breed)`

### 4.3 Repositories

- [ ] Create `Barkfest.Persistence/Repositories/OwnerRepository.cs` implementing `IOwnerRepository`
- [ ] Create `Barkfest.Persistence/Repositories/PetRepository.cs` implementing `IPetRepository`
- [ ] Create `Barkfest.Persistence/UnitOfWork.cs` implementing `IUnitOfWork`

### 4.4 Migration

- [ ] Generate migration `InitialCreate`:
  ```bash
  dotnet ef migrations add InitialCreate \
    --project src/Barkfest.Persistence \
    --startup-project src/Barkfest.API
  ```
- [ ] Verify `Up()` method creates all tables with correct column names, types, and constraints
- [ ] Do NOT run `dotnet ef database update` — migration applied at runtime via `MigrateAsync()`

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

- [ ] Create `Barkfest.Persistence/DependencyInjection.cs`
  - `AddPersistence(IHostApplicationBuilder)` extension method
  - Registers `AppDbContext` with SQL Server using `builder.AddSqlServerDbContext<AppDbContext>("barkfest-db")`
  - Registers `IOwnerRepository` → `OwnerRepository`
  - Registers `IPetRepository` → `PetRepository`
  - Registers `IUnitOfWork` → `UnitOfWork`

---

## Phase 5 — Infrastructure Layer

- [ ] Create `Barkfest.Infrastructure/Storage/AzureBlobStorageService.cs` implementing `IBlobStorageService`
  - `UploadAsync()`, `DownloadAsync()`, `DeleteAsync()`, `ExistsAsync()`

- [ ] Create `Barkfest.Infrastructure/Messaging/EmailService.cs`

- [ ] Create `Barkfest.Infrastructure/DependencyInjection.cs`
  - `AddInfrastructure(IHostApplicationBuilder)` extension method
  - Registers `BlobServiceClient` using `builder.AddAzureBlobClient("barkfest-blobs")`
  - Registers `IBlobStorageService` → `AzureBlobStorageService`

---

## Phase 6 — API Layer

### 6.1 Controllers

- [ ] Create `Barkfest.API/Controllers/OwnersController.cs`
  - `GET    /api/owners`                               → `GetAllOwnersQuery`
  - `GET    /api/owners/{id:guid}`                     → `GetOwnerByIdQuery` — 404 if not found
  - `POST   /api/owners`                               → `CreateOwnerCommand` — 201 Created
  - `PUT    /api/owners/{id:guid}`                     → `UpdateOwnerCommand` — 404 if not found
  - `DELETE /api/owners/{id:guid}`                     → `DeleteOwnerCommand` — 404 if not found
  - `POST   /api/owners/{id:guid}/profile-image`       → `UploadOwnerProfileImageCommand`
  - `DELETE /api/owners/{id:guid}/profile-image`       → `RemoveOwnerProfileImageCommand`

- [ ] Create `Barkfest.API/Controllers/PetsController.cs`
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

- [ ] Create `Barkfest.API/Middleware/ExceptionHandlingMiddleware.cs`
  - Catches `NotFoundException` → 404
  - Catches `DomainException` → 400
  - Catches unhandled exceptions → 500

### 6.3 Program.cs

- [ ] Configure `Program.cs`:
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

- [ ] Create `appsettings.json`:
  ```json
  {
    "ConnectionStrings": {
      "barkfest-db": "",
      "barkfest-blobs": ""
    },
    "_readme": "Connection strings are injected by Aspire when running locally. In production or CI populate these via environment variables or a secrets manager."
  }
  ```

---

## Phase 7 — Test Projects

### 7.1 Barkfest.Domain.Tests

- [ ] `OwnerTests.cs`
  - `SetFirstName`: valid, trim, at max length, null throws, empty throws, whitespace throws, exceeds max length throws
  - `SetLastName`: valid, trim, at max length, null throws, empty throws, whitespace throws, exceeds max length throws
  - `SetEmail`: valid, lowercase and trim, at max length, null throws, empty throws, whitespace throws, no @ symbol throws, no domain throws, no TLD throws, spaces throw, exceeds max length throws
  - `SetProfileImage`: valid, null blob name throws, empty blob name throws, null content type throws, empty content type throws, unsupported content type throws
  - `RemoveProfileImage`: clears profile image

- [ ] `PetTests.cs`
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

- [ ] `PetImageTests.cs`
  - `SetImage`: valid, jpeg accepted, jpg accepted, png accepted, null blob name throws, empty blob name throws, null content type throws, empty content type throws, unsupported content type throws
  - `SetDisplayOrder`: valid, zero accepted, negative throws

- [ ] `ProfileImageTests.cs`
  - `Create`: valid, lowercases and trims content type, trims blob name, jpeg accepted, jpg accepted, png accepted
  - `Create` sad path: null blob name throws, empty blob name throws, whitespace blob name throws, null content type throws, empty content type throws, whitespace content type throws, unsupported content type throws
  - Equality: same values are equal, different values are not equal

- [ ] `BreedTests.cs`
  - `SetDogBreed`: valid, null throws
  - `SetCatBreed`: valid, null throws

- [ ] `SupportedImageTypeTests.cs`
  - `IsAllowedContentType`: jpeg true, jpg true, png true, unsupported false, case insensitive
  - `IsAllowedExtension`: .jpeg true, .jpg true, .png true, unsupported false, case insensitive

- [ ] `PetTypeTests.cs`
  - Has Dog, Cat, Other values
  - Has exactly 3 values
  - Lookup by name: Dog, Cat, Other, invalid throws
  - Lookup by value: Dog, Cat, Other, invalid throws

- [ ] `DogBreeedTests.cs`
  - Has exactly 30 values
  - Includes Labradoodle, Goldendoodle, Cockapoo, Mixed, Other
  - Lookup by name, lookup by value, invalid name throws, invalid value throws
  - Unique values, unique names

- [ ] `CatBreedTests.cs`
  - Has exactly 29 values
  - Includes DomesticShorthair, Tabby, Mixed, Other
  - Lookup by name, lookup by value, invalid name throws, invalid value throws
  - Unique values, unique names

### 7.2 Barkfest.Application.Tests

Use NSubstitute for all mocking. Use Shouldly for all assertions.

- [ ] `CreateOwnerCommandHandlerTests.cs`
- [ ] `UpdateOwnerCommandHandlerTests.cs`
- [ ] `DeleteOwnerCommandHandlerTests.cs`
- [ ] `GetOwnerByIdQueryHandlerTests.cs` — returns OwnerDto when found, throws NotFoundException when not found
- [ ] `GetAllOwnersQueryHandlerTests.cs`

- [ ] `CreateOwnerCommandValidatorTests.cs`
  - `FirstName`: valid, at max length, empty fails, null fails, whitespace fails, exceeds max length fails
  - `LastName`: valid, at max length, empty fails, null fails, whitespace fails, exceeds max length fails
  - `Email`: valid, at max length, empty fails, null fails, whitespace fails, no @ symbol fails, no domain fails, no TLD fails, spaces fail, exceeds max length fails

- [ ] `UpdateOwnerCommandValidatorTests.cs` — mirror same cases as Create

- [ ] `UploadOwnerProfileImageCommandHandlerTests.cs`
  - Uploads image and updates owner
  - Throws NotFoundException when owner not found

- [ ] `UploadOwnerProfileImageCommandValidatorTests.cs`
  - Content type: jpeg passes, jpg passes, png passes, unsupported fails
  - Extension: .jpeg passes, .jpg passes, .png passes, unsupported fails

- [ ] `RemoveOwnerProfileImageCommandHandlerTests.cs`
  - Removes from blob storage and clears owner
  - Throws NotFoundException when owner not found
  - Does not call blob storage when owner has no existing image

- [ ] `CreatePetCommandHandlerTests.cs`
- [ ] `UpdatePetCommandHandlerTests.cs`
- [ ] `DeletePetCommandHandlerTests.cs`
- [ ] `GetPetByIdQueryHandlerTests.cs`
- [ ] `GetAllPetsQueryHandlerTests.cs`
- [ ] `GetPetsByOwnerIdQueryHandlerTests.cs`

- [ ] `CreatePetCommandValidatorTests.cs`
  - `Name`: valid, at max length, empty fails, null fails, whitespace fails, exceeds max length fails

- [ ] `UpdatePetCommandValidatorTests.cs` — mirror same cases as Create

- [ ] `AddPetImageCommandHandlerTests.cs`
  - Adds image successfully
  - Throws when max images exceeded
  - Throws NotFoundException when pet not found

- [ ] `AddPetImageCommandValidatorTests.cs`
  - Content type: jpeg passes, jpg passes, png passes, unsupported fails
  - Extension: .jpeg passes, .jpg passes, .png passes, unsupported fails

- [ ] `RemovePetImageCommandHandlerTests.cs`
  - Removes from blob storage and pet
  - Throws NotFoundException when pet not found

- [ ] `ValidationBehaviorTests.cs`

### 7.3 Barkfest.Persistence.Tests

- [ ] `Fixtures/DatabaseFixture.cs` — Testcontainers SQL Server, applies migrations via `MigrateAsync()`
- [ ] `Repositories/OwnerRepositoryTests.cs`
- [ ] `Repositories/PetRepositoryTests.cs`
- [ ] `Configurations/OwnerConfigurationTests.cs`
- [ ] `Configurations/PetConfigurationTests.cs`
- [ ] `Configurations/PetImageConfigurationTests.cs`
- [ ] `Configurations/BreedConfigurationTests.cs`

### 7.4 Barkfest.Infrastructure.Tests

- [ ] `Fixtures/AzuriteFixture.cs` — Testcontainers Azurite
- [ ] `Storage/AzureBlobStorageServiceTests.cs`

### 7.5 Barkfest.API.Tests

- [ ] `Fixtures/ApiFactory.cs`
  - Extends `WebApplicationFactory<Program>`
  - Replaces SQL Server with Testcontainers SQL Server
  - Replaces Azure Blob Storage with Testcontainers Azurite
  - Both run in containers — no real external services

- [ ] `Controllers/OwnersControllerTests.cs`
  - CRUD: GET 200, POST 201, PUT 200, DELETE 204, not found 404
  - Email: missing 400, invalid format 400, exceeds max length 400
  - FirstName: missing 400, exceeds max length 400
  - LastName: missing 400, exceeds max length 400
  - Profile image: valid upload 200, not found 404, unsupported content type 400, unsupported extension 400

- [ ] `Controllers/PetsControllerTests.cs`
  - CRUD: GET 200, POST 201, PUT 200, DELETE 204, not found 404
  - Name: missing 400, exceeds max length 400
  - Profile image: valid upload 200, not found 404, unsupported content type 400
  - Gallery: add valid 200, max exceeded 400, remove valid 204, not found 404

### 7.6 Barkfest.Integration.Tests

No project references — communicates with running app over HTTP.

- [ ] `Config/IntegrationTestSettings.cs` — base URL and settings
- [ ] `Flows/OwnerFlowTests.cs`
  - Full lifecycle: create → read → update → delete → confirm 404
  - Email: missing 400, invalid 400, exceeds max length 400
  - FirstName: missing 400, exceeds max length 400
  - LastName: missing 400, exceeds max length 400
  - Profile image: upload succeeds, unsupported type 400, remove succeeds

- [ ] `Flows/PetFlowTests.cs`
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

- [ ] Create `src/Barkfest.AppHost` — Aspire host project (`Microsoft.NET.Sdk`)
- [ ] Create `src/Barkfest.ServiceDefaults` — Aspire defaults project (`Microsoft.NET.Sdk`)
- [ ] Add `Barkfest.AppHost` → `Barkfest.API` project reference
- [ ] Add `Barkfest.API` → `Barkfest.ServiceDefaults` project reference
- [ ] Add NuGet packages per Phase 1 NuGet table

### 8.2 AppHost — `Program.cs`

Containers are **persistent** with **explicit named volumes**. Resource names and volume names
are hardcoded and project-scoped to prevent collisions when a developer has multiple Aspire
solutions running simultaneously.

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("barkfest-db")
                 .WithLifetime(ContainerLifetime.Persistent)
                 .WithDataVolume("barkfest-db-data");

var blobs = builder.AddAzureStorage("barkfest-storage")
                   .RunAsEmulator(e => e
                       .WithLifetime(ContainerLifetime.Persistent)
                       .WithDataVolume("barkfest-blobs-data"))
                   .AddBlobs("barkfest-blobs");

builder.AddProject<Projects.Barkfest_API>("barkfest-api")
       .WithReference(sql)
       .WithReference(blobs);

builder.Build().Run();
```

**First `dotnet run`:** Aspire checks Docker — containers do not exist, creates them with stable
names `barkfest-db` and `barkfest-storage`. The API starts, `MigrateAsync()` runs, schema is
created. Ready.

**Subsequent runs:** Aspire finds existing containers, starts them if stopped. All data is
intact. No manual steps.

**Stopping the AppHost:** containers remain in Docker with data preserved. Volumes
`barkfest-db-data` and `barkfest-blobs-data` survive even a `docker rm`.

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

**`Barkfest.Persistence/DependencyInjection.cs`** — switch to Aspire-aware registration:

```csharp
// Before:
services.AddDbContext<AppDbContext>(opts =>
    opts.UseSqlServer(config.GetConnectionString("DefaultConnection")));

// After:
builder.AddSqlServerDbContext<AppDbContext>("barkfest-db");
```

`AddSqlServerDbContext` reads the Aspire-injected connection string and adds health checks,
telemetry, and retry resilience automatically.

**`Barkfest.Infrastructure/DependencyInjection.cs`** — switch to Aspire-aware registration:

```csharp
// Before:
services.AddSingleton(new BlobServiceClient(
    config.GetConnectionString("AzureBlobStorage")));

// After:
builder.AddAzureBlobClient("barkfest-blobs");
```

`AddAzureBlobClient` reads the Aspire-injected connection string and registers `BlobServiceClient`
with health checks and telemetry automatically.

> **Non-Aspire environments:** `appsettings.json` retains `ConnectionStrings:barkfest-db` and
> `ConnectionStrings:barkfest-blobs` as empty placeholders with a `_readme` note. In production
> or CI these are populated via environment variables or secrets manager — Aspire is the local
> dev orchestrator only.

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
- `Barkfest.Integration.Tests` talks to a running app over HTTP

### 8.7 Running Locally

```bash
# Clone and run — that's it
dotnet run --project src/Barkfest.AppHost
```

Aspire dashboard opens automatically (typically `https://localhost:15888`) showing live logs,
traces, and health for all resources. The API URL is listed there.

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
- `Pet.MaxImages` constant used in all image limit tests — never hardcode the number `5`
- `Owner.FirstNameMaxLength`, `Owner.LastNameMaxLength`, `Owner.EmailMaxLength`, `Pet.NameMaxLength` constants used in all length-related tests — never hardcode the numbers
- Each `src` project has its own `DependencyInjection.cs` with a self-registering extension method
- Connection strings injected by Aspire for local dev; populated via environment variables or secrets manager in production and CI — never committed to source control
- Testcontainers used for all integration tests — both SQL Server (`Testcontainers.MsSql`) and Azure Blob Storage (`Testcontainers.Azurite`) run in containers, no real external services in any test project
- `Age` is computed from `DateOfBirth` at runtime — never stored in the database
- Cascade deletes: `Owner` → `Pets`, `Pet` → `PetImages`, `Pet` → `Breeds`
- `ExceptionHandlingMiddleware` maps `NotFoundException` → 404, `DomainException` → 400, unhandled → 500
- Migration applied at runtime via `MigrateAsync()` on startup — never run `dotnet ef database update`
