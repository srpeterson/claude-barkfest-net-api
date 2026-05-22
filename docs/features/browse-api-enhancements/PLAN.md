# Browse API Enhancements — Plan

## Goal

Extend the public browse API to support server-side pagination, featured-image-only
filtering, and two new dropdown hydration endpoints. This is the backend half of wiring
up the landing page; the frontend is handled separately in `feature/wire-landing-page`.

---

## Scope

| # | Change | Layer |
|---|---|---|
| 1 | `PagedResult<T>` generic wrapper | Application |
| 2 | Featured-image-only filter on `GetBrowseImagesAsync` | Persistence |
| 3 | Server-side pagination on `GetBrowseImagesAsync` | Persistence + Application |
| 4 | Breed filter pushed to DB (EF.Property on TPH column) | Persistence |
| 5 | `GET /v1/browse/pet-types` — returns SmartEnum values | Application + API |
| 6 | `GET /v1/browse/breeds?petType=Dog` — returns breed names | Application + API |
| 7 | Tests for all new/modified behaviour | Application.Tests + API.Tests |

---

## Implementation Steps

### Step 1 — `PagedResult<T>`
- New file: `Barkfest.Application/Common/Models/PagedResult.cs`
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
- Add `int Page, int PageSize` params
- Return type: `PagedResult<BrowseImageDto>`
- Unknown petType returns `PagedResult` with empty items (not empty `IEnumerable`)

### Step 5 — New `GetBrowsePetTypesQuery`
- `Barkfest.Application/Features/Browse/Queries/GetBrowsePetTypes/`
- Handler reads from `PetType.List` — no DB call
- Returns `IReadOnlyList<string>`

### Step 6 — New `GetBrowseBreedsQuery`
- `Barkfest.Application/Features/Browse/Queries/GetBrowseBreeds/`
- Handler reads from `DogBreed.List` or `CatBreed.List` — no DB call
- Unknown petType returns empty list
- Returns `IReadOnlyList<string>`

### Step 7 — Update `BrowseController`
- `GetImages`: add `page` (default 1) and `pageSize` (default 6) query params
- New `GetPetTypes`: `GET /v1/browse/pet-types`
- New `GetBreeds`: `GET /v1/browse/breeds?petType=Dog`

### Step 8 — Tests
- `GetBrowseImagesQueryHandlerTests` — update for new signature and `PagedResult` return type
- New `GetBrowsePetTypesQueryHandlerTests`
- New `GetBrowseBreedsQueryHandlerTests`
- `BrowseControllerTests` — update for paged response shape, add new endpoint tests
