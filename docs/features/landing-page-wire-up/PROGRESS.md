# Landing Page — Full Wire-Up Progress

## Status: In progress

---

## Step 1 — Smoke Test With Real Data

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
| 9 | Issues list compiled for Step 4 | ⬜ Not started |

---

## Step 2 — Browse Grid Cache Invalidation

| # | Milestone | Status |
|---|---|---|
| 1 | `AddPetDialog.tsx` — `onSuccess` prop added | ⬜ Not started |
| 2 | `Navbar.tsx` — `useQueryClient` + `invalidateQueries(['browse', 'images'])` wired | ⬜ Not started |
| 3 | Verified: new pet appears on home page immediately after dialog closes | ⬜ Not started |

---

## Step 3 — Owner Attribution on PetCard

| # | Milestone | Status |
|---|---|---|
| 1 | `PetCard.tsx` — `ownerName` line added | ⬜ Not started |
| 2 | Visually verified on real data | ⬜ Not started |

---

## Step 4 — Polish Pass

| # | Milestone | Status |
|---|---|---|
| 1 | Issues from Step 1 addressed | ⬜ Not started |

---

## Step 5 — End-to-End Sign-Off

| # | Milestone | Status |
|---|---|---|
| 1 | Full flow verified (post pet → grid refresh → filters → pagination → empty states) | ⬜ Not started |
| 2 | TypeScript check clean (`pnpm --dir barkfest-ui build`) | ⬜ Not started |
| 3 | Committed and pushed | ⬜ Not started |
