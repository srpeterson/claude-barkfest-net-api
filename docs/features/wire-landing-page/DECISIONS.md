# Wire Landing Page — Decisions

All architectural decisions for this feature were established during the planning
session before any code was written. See the summary below; full rationale is in
the session history.

---

## `staleTime: Infinity` for pet types and breeds

Pet type and breed lists can only change with a backend deployment. Caching them
indefinitely for the lifetime of the page session avoids redundant API calls on
every filter interaction.

---

## Breed dropdown width changed to `w-48`

Breed names can be long ("Cavalier King Charles Spaniel"). Widened from `w-40` to
`w-48` to avoid truncation in the native select element.

---

## Pagination buttons hidden when on page 1 with no next page

`{(hasPrev || hasMore) && ...}` — the pagination row is hidden entirely when there
is only one page of results, keeping the UI clean for small datasets.

---

## `handleBreedChange` resets page to 1

Changing the breed filter resets pagination to page 1, consistent with how
`handleTypeChange` behaves. Avoids landing on a page that no longer exists after
narrowing the result set.
