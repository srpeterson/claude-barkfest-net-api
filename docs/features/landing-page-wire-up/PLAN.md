# Landing Page — Full Wire-Up Plan

## Goal

Verify the landing page works correctly end-to-end with real owner accounts and real
pet data, fix the one confirmed gap (browse grid does not refresh after a new pet is
posted), add owner attribution to pet cards, and resolve any UI rough edges discovered
during smoke testing.

No new API endpoints or database changes are required — the browse API is complete.
All work is frontend-only.

---

## Current State

| Concern | Status |
|---|---|
| `BrowseImageDto` shape (petName, age, breed, description, blobName, ownerName) | Complete |
| Sort order — newest first (`OrderByDescending(pi => pi.Pet.CreatedAt)`) | Complete |
| `PetCard` — image, name, age badge, description, breed | Complete |
| `PetGrid` — loading spinner, empty state (no pets / no results) | Complete |
| `FilterBar` / `PetTypeBreedSelector` — pet type + breed dropdowns | Complete |
| `HomePage` — pagination state, filter state, reset on type change | Complete |
| Pagination — prev/next buttons, hidden when not needed | Complete |
| Empty state copy — "No pets posted yet" vs "Try a different breed or pet type" | Complete |
| Browse grid refreshes after `AddPetDialog` success | **Missing** |
| Owner attribution on `PetCard` | **Missing** |

---

## Step 1 — Smoke Test With Real Data

Before writing any code, run the app with real owner accounts and pets to identify
any remaining issues:

- Register an owner, log in, post a pet with images via the Add Pet dialog
- Verify the pet appears on the home page grid after refreshing
- Verify pet type filter and breed filter return correct results
- Verify breed dropdown resets when pet type changes
- Verify pagination appears once pets exceed 6 (`PAGE_SIZE`)
- Verify empty state ("No pets posted yet") renders when the database is empty
- Verify empty state ("Try a different breed…") renders when filters match nothing
- Verify featured image is shown on the card (not a non-featured gallery image)
- Note any layout or spacing issues for Step 4

---

## Step 2 — Browse Grid Cache Invalidation

**Problem:** After `AddPetDialog` submits successfully, the home page browse grid
is not refreshed. The new pet only appears if the user manually reloads the page.

**Root cause:** `AddPetDialog` has no access to `queryClient`, and its `onClose`
callback in `Navbar` does not trigger a cache invalidation.

**Fix:**
- Call `queryClient.invalidateQueries({ queryKey: ['browse', 'images'] })` after
  `AddPetDialog` reports success
- The cleanest approach is an `onSuccess` prop on `AddPetDialog` that `Navbar`
  invokes; `Navbar` calls `useQueryClient()` and invalidates there
- Invalidate the entire `['browse', 'images']` prefix so all page/filter
  combinations are marked stale — a broad invalidation is correct here since
  a new pet should appear regardless of the current filter state

**Files to change:**
- `barkfest-ui/src/components/AddPetDialog.tsx` — add `onSuccess?: () => void` prop,
  call it just before `setSuccess(true)`
- `barkfest-ui/src/components/Navbar.tsx` — add `useQueryClient`, pass
  `onSuccess` to `AddPetDialog` that calls `queryClient.invalidateQueries`

---

## Step 3 — Owner Attribution on PetCard

**Context:** `BrowseImageDto.ownerName` is populated by the API but not shown
on the card. Adding it gives the landing page a social feel — visitors can see
whose pet each card belongs to.

**Proposed display:**
```
┌──────────────┐
│   [image]    │
├──────────────┤
│ Buddy     3y │   ← petName + age badge
│ by Stephen   │   ← ownerName (small, muted)
│ Beagle       │   ← breed
│ desc…        │   ← description
└──────────────┘
```

`ownerName` rendered as a small muted line between the name row and the breed,
or just below the name row.

**Files to change:**
- `barkfest-ui/src/components/PetCard.tsx` — add `ownerName` line

---

## Step 4 — Polish Pass

Address any rough edges discovered during Step 1. Common candidates:

- FilterBar `sticky top-16` — verify it doesn't overlap content on mobile or at
  unusual scroll positions
- Pagination bar — verify it only appears when there are pets to paginate
- Card animation (`animate-fade-in-up`) — verify it fires correctly on filter
  changes (not just initial load)
- Image aspect ratio — verify portrait vs landscape photos are cropped consistently
- Responsive grid — verify single-column layout on small screens looks correct

---

## Step 5 — End-to-End Sign-Off

Final verification pass:

1. Post a new pet → grid refreshes automatically on the home page
2. Filter by pet type → correct results, breed dropdown updates
3. Filter by breed → further narrows results correctly
4. Clear filters → full grid returns
5. Page through results → pagination works, "Previous" disabled on page 1
6. Owner name shown on every card
7. Empty states shown correctly for both scenarios
8. TypeScript check clean: `pnpm --dir barkfest-ui build`
