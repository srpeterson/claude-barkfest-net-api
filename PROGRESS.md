# Barkfest — Progress

## Phase 1 — Solution Scaffold ✅ Complete

**Completed:**
- Created `Barkfest.sln` with all 11 projects (5 src, 6 tests)
- All projects target `net10.0`
- All project references wired per Clean Architecture rules
- All NuGet packages installed:
  - Domain: Ardalis.SmartEnum
  - Application: MediatR, FluentValidation
  - Persistence: Microsoft.EntityFrameworkCore.SqlServer, Microsoft.EntityFrameworkCore.Tools
  - Infrastructure: Azure.Storage.Blobs
  - API: Scalar.AspNetCore, Serilog.AspNetCore
  - Unit test projects: xUnit, Shouldly, NSubstitute
  - Integration test projects: above + Testcontainers.MsSql, Testcontainers.Azurite
  - API.Tests: above + Microsoft.AspNetCore.Mvc.Testing
- User Secrets initialised on `Barkfest.API` with placeholder keys
- `.gitignore` created (dotnet template)
- `appsettings.json` configured with connection string keys (empty values)
- Boilerplate generated files removed
- `docs/` folder created with SPEC.md, PLAN.md, DECISIONS.md, README.md

**Branch:** `feature/initial-build`

---

## Next

**Phase 2 — Domain Layer**

Start with:
1. `Barkfest.Domain/Exceptions/DomainException.cs`
2. `Barkfest.Domain/ValueObjects/SupportedImageType.cs`
3. `Barkfest.Domain/ValueObjects/ProfileImage.cs`
4. SmartEnums: `PetType`, `DogBreed`, `CatBreed`
5. Entities: `Owner`, `Breed` (abstract), `DogBreedInfo`, `CatBreedInfo`, `PetImage`, `Pet`
6. Interfaces: `IOwnerRepository`, `IPetRepository`, `IUnitOfWork`
