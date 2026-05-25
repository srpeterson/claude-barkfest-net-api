# Landing Page — Full Wire-Up Progress

## Status: In progress

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
| 1 | `api.ts` — `displayName` added to register payload | ⬜ Not started |
| 2 | `RegisterModal.tsx` — optional `displayName` input field added | ⬜ Not started |
| 3 | Verified: DisplayName stored and shown on pet cards | ⬜ Not started |

---

## Step 3 — Smoke Test With Real Data

| # | Milestone | Status |
|---|---|---|
| 1 | Register owner, post a pet, verify it appears on the home page | ⬜ Not started |
| 2 | Pet type filter returns correct results | ⬜ Not started |
| 3 | Breed dropdown resets when pet type changes | ⬜ Not started |
| 4 | Breed filter further narrows results correctly | ⬜ Not started |
| 5 | Pagination appears once pets exceed PAGE_SIZE (6) | ⬜ Not started |
| 6 | Empty state — "No pets posted yet" (empty database) | ⬜ Not started |
| 7 | Empty state — "Try a different breed…" (filters match nothing) | ⬜ Not started |
| 8 | Featured image displayed on each card | ⬜ Not started |
| 9 | Owner attribution shown when DisplayName is set | ⬜ Not started |

---

## Step 4 — Browse Grid Cache Invalidation

| # | Milestone | Status |
|---|---|---|
| 1 | `AddPetDialog.tsx` — `onSuccess` prop added | ✅ Complete |
| 2 | `Navbar.tsx` — `useQueryClient` + `invalidateQueries(['browse', 'images'])` wired | ✅ Complete |
| 3 | Verified: new pet appears on home page immediately after dialog closes | ⬜ Not started |

---

## Step 5 — Owner Attribution on PetCard

| # | Milestone | Status |
|---|---|---|
| 1 | `PetCard.tsx` — conditional `ownerName` line (null-safe) | ✅ Complete |
| 2 | Visually verified on real data | ⬜ Not started |

---

## Step 6 — Polish Pass

| # | Milestone | Status |
|---|---|---|
| 1 | Issues from smoke test addressed | ⬜ Not started |

---

## Step 7 — End-to-End Sign-Off

| # | Milestone | Status |
|---|---|---|
| 1 | Full flow verified (register with DisplayName → post pet → grid refresh → filters → pagination → empty states) | ⬜ Not started |
| 2 | TypeScript check clean (`pnpm --dir barkfest-ui build`) | ⬜ Not started |
| 3 | Committed and pushed | ⬜ Not started |
