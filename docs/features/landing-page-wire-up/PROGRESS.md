# Landing Page — Full Wire-Up Progress

## Status: Complete

---

## Step 1 — Owner.DisplayName (API + DB)

| # | Milestone | Status |
|---|---|---|
| 1 | Domain: `DisplayNameMaxLength`, `string? DisplayName`, `SetDisplayName()` | ✅ Complete |
| 2 | Application: `OwnerDto`, `OwnerMappings`, `BrowseImageDto` (`string?`) | ✅ Complete |
| 3 | Application: `RegisterCommand` + handler + validator | ✅ Complete |
| 4 | Application: `UpdateOwnerCommand` + handler + validator | ✅ Complete |
| 5 | Persistence: `OwnerConfiguration`, `BrowseRepository.ToDto()` | ✅ Complete |
| 6 | API: `UpdateOwnerRequest` + `OwnerController` | ✅ Complete |
| 7 | Frontend: `browse.ts` (`string \| null`), `PetCard.tsx` (conditional render) | ✅ Complete |
| 8 | Tests: Domain, validator, configuration | ✅ Complete |
| 9 | Migration `AddOwnerDisplayName` generated | ✅ Complete |
| 10 | CLAUDE.md updated | ✅ Complete |
| 11 | All 725 tests pass, `pnpm build` clean | ✅ Complete |

---

## Step 2 — Frontend: DisplayName in Register Form

| # | Milestone | Status |
|---|---|---|
| 1 | `api.ts` — `displayName` added to register payload, `ApiError` class, `checkDisplayName()` | ✅ Complete |
| 2 | `RegisterModal.tsx` — display name field (required, max 25, debounced availability check, min-length UI guard) | ✅ Complete |
| 3 | `GET /v1/auth/check-display-name` — availability endpoint, `CheckDisplayNameQuery` | ✅ Complete |
| 4 | Error handling — `ApiError` surfaces 4xx messages verbatim; duplicate email/username conflicts caught pre-save | ✅ Complete |
| 5 | Verified: DisplayName stored and shown on pet cards | ✅ Complete |

---

## Step 3 — Smoke Test With Real Data

| # | Milestone | Status |
|---|---|---|
| 1 | Register owner, post a pet, verify it appears on the home page | ✅ Complete |
| 2 | Pet type filter returns correct results | ✅ Complete |
| 3 | Breed dropdown resets when pet type changes | ✅ Complete |
| 4 | Breed filter further narrows results correctly | ✅ Complete |
| 5 | Pagination appears once pets exceed PAGE_SIZE (6) | ✅ Complete |
| 6 | Empty state — "No pets posted yet" (empty database) | ✅ Complete |
| 7 | Empty state — "Try a different breed…" (filters match nothing) | ✅ Complete |
| 8 | Featured image displayed on each card | ✅ Complete |
| 9 | Owner attribution shown when DisplayName is set | ✅ Complete |

---

## Step 4 — Browse Grid Cache Invalidation

| # | Milestone | Status |
|---|---|---|
| 1 | `AddPetDialog.tsx` — `onSuccess` prop added | ✅ Complete |
| 2 | `Navbar.tsx` — `useQueryClient` + `invalidateQueries(['browse', 'images'])` wired | ✅ Complete |
| 3 | `Navbar.tsx` — also invalidates `['browse', 'hero-strip']` so count + thumbnails refresh on pet add | ✅ Complete |
| 4 | Verified: new pet appears on home page immediately after dialog closes | ✅ Complete |

---

## Step 5 — Owner Attribution on PetCard

| # | Milestone | Status |
|---|---|---|
| 1 | `PetCard.tsx` — conditional `ownerName` line (null-safe) | ✅ Complete |
| 2 | Visually verified on real data | ✅ Complete |

---

## Step 6 — Polish Pass

| # | Milestone | Status |
|---|---|---|
| 1 | **Image proxy** — `ImagesController` (`GET /v1/images/{container}/{*blob}`) routes images through API; removes Azurite port instability; `VITE_BLOB_BASE_URL` removed, `imageUrl.ts` updated | ✅ Complete |
| 2 | **PetCard redesign** — gradient overlay, `aspect-[4/5]` portrait ratio, age display in months/years (`formatAge`), breed badge, paw icon owner attribution | ✅ Complete |
| 3 | **Navbar** — "Join the Barkfest!" primary button for unauthenticated users | ✅ Complete |
| 4 | **HeroSection** — filter-aware social proof strip (circular pet thumbnails + pet count), staggered entrance animations, badge removed, spacing tightened | ✅ Complete |
| 5 | **AddPetDialog** — date picker click-only (`onKeyDown` blocks typing), date + age pickers side-by-side with radio selection, highlighted border on active control, improved label copy ("How old is [name]?", "I know exactly!", "Rescue / Not sure") | ✅ Complete |
| 6 | **Pagination** — restyled with primary-coloured border/text on active buttons, page indicator as pill badge | ✅ Complete |
| 7 | **DogBreed** — added St. Bernard (31), replaced Cavalier King Charles Spaniel with Cocker Spaniel (12); tests updated | ✅ Complete |
| 8 | **README** — frontend environment variables section updated (image proxy, no `VITE_BLOB_BASE_URL`) | ✅ Complete |

---

## Step 7 — End-to-End Sign-Off

| # | Milestone | Status |
|---|---|---|
| 1 | Full flow verified (register with DisplayName → post pet → grid refresh → filters → pagination → empty states) | ✅ Complete |
| 2 | TypeScript check clean (`pnpm --dir barkfest-ui build`) | ✅ Complete |
| 3 | Committed and pushed | ✅ Complete |
