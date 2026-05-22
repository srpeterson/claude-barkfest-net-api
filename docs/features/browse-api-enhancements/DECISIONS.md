# Browse API Enhancements — Decisions

## Breed filter pushed to DB via `EF.Property`

**Decision:** Use `EF.Property<int>(pi.Pet.Breed, "BreedValue") == breedValue` to filter
breeds at the DB level rather than loading all records into memory and filtering in C#.

**Why:** Correct server-side pagination requires count and skip/take to operate on the
already-filtered dataset. In-memory breed filtering after a DB load would give wrong
pagination counts and load unnecessary rows.

**How:** Resolve the breed string to its SmartEnum integer value first (using
`DogBreed.List.FirstOrDefault` / `CatBreed.List.FirstOrDefault`), then use EF.Property
to compare against the `BreedValue` TPH column shared by both `DogBreedInfo` and
`CatBreedInfo`. Unknown breed names return an empty `PagedResult` immediately, no DB
query issued.

---

## `GetBrowsePetTypesQuery` and `GetBrowseBreedsQuery` — no repository

**Decision:** Pet type and breed lists come directly from SmartEnum reflection (`PetType.List`,
`DogBreed.List`, `CatBreed.List`). No repository interface needed, no DB query.

**Why:** The breed and pet type lists are defined in code. They can only change with a
deployment. A DB query would add latency and complexity for data that is always in sync
with the running binary.

---

## Breed ordering — by SmartEnum value (insertion order)

**Decision:** Breeds are returned ordered by `SmartEnum.Value` (ascending), which matches
the order they were defined in the SmartEnum class.

**Why:** The SmartEnum values were assigned in a deliberate order (most popular breeds
first within each species). Preserving that order gives the best UX out of the box
without needing a separate sort configuration.

---

## `PagedResult<T>` — placed in Application/Common/Models

**Decision:** `PagedResult<T>` lives in `Barkfest.Application/Common/Models/`.

**Why:** It is a generic application-layer construct, not domain logic. Keeping it in
`Common/Models` makes it available to any feature handler without creating a dependency
on a specific feature namespace.
