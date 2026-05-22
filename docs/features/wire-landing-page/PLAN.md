# Wire Landing Page — Plan

## Goal

Connect the Barkfest landing page UI to the browse API endpoints introduced in
`feature/browse-api-enhancements`. Replace all hardcoded data, stub types, and
client-side pagination with real API calls.

---

## Scope

| # | Change | Layer |
|---|---|---|
| 1 | `VITE_BLOB_BASE_URL` added to `.env` | UI config |
| 2 | `src/types/browse.ts` — TypeScript types for API responses | UI types |
| 3 | `src/config/petTypes.ts` — API value → display label mapping | UI config |
| 4 | `src/lib/imageUrl.ts` — blob URL construction helper | UI lib |
| 5 | `src/lib/api.ts` — three browse API functions | UI lib |
| 6 | `PetCard.tsx` — updated to `BrowseImageDto` shape | UI component |
| 7 | `PetGrid.tsx` — updated type + contextual empty state | UI component |
| 8 | `FilterBar.tsx` — API-driven pet type and breed dropdowns | UI component |
| 9 | `HomePage.tsx` — real API call, server-side pagination | UI page |
| 10 | `src/types/pet.ts` — deleted (replaced by browse.ts) | UI cleanup |

---

## Key Decisions (established in planning)

- **Image URL:** `VITE_BLOB_BASE_URL` env var + `/pet-images/{blobName}` constructed client-side
- **Pet type dropdown:** API-driven via `GET /v1/browse/pet-types`; "All" is synthetic UI option; display labels via `petTypes.ts`
- **Breed dropdown:** API-driven via `GET /v1/browse/breeds?petType=`; disabled when "All" selected; shown as-is
- **Pagination:** `PagedResult.hasMore` drives Next button; `page > 1` drives Previous button
- **Empty state:** contextual — filters active → friendly message; no filters → "Be the first!"
