# Add Pet Dialog — Progress

## Status: In progress

---

## Part 1 — Types

| # | Milestone | Status |
|---|---|---|
| 1 | `CreatePetRequest` and `AddPetImagesResult` types added to `src/types/pet.ts` | ✅ Complete |

---

## Part 2 — api.ts additions

| # | Milestone | Status |
|---|---|---|
| 1 | `createPet()` — `POST /v1/pets` | ✅ Complete |
| 2 | `addPetImages()` — `POST /v1/pets/{id}/images` (multipart/form-data) | ✅ Complete |
| 3 | `setFeaturedImage()` — `PUT /v1/pets/{id}/images/{imageId}/featured` | ✅ Complete |

---

## Part 3 — AddPetDialog component

| # | Milestone | Status |
|---|---|---|
| 1 | Step 1 — pet details form (name, pet type, breed, date of birth, description) | ✅ Complete |
| 2 | Breed dropdown reacts to pet type selection | ✅ Complete |
| 3 | Step 2 — image upload with thumbnail previews and remove buttons | ✅ Complete |
| 4 | Featured image selection | ✅ Complete |
| 5 | Submission flow (create pet → upload images → set featured) | ✅ Complete |
| 6 | Loading, success, and error states | ✅ Complete |
| 7 | State reset on dialog close | ✅ Complete |

---

## Part 4 — Navbar wire-up

| # | Milestone | Status |
|---|---|---|
| 1 | "Post a Pet" button opens `AddPetDialog` | ✅ Complete |

---

## Part 5 — UX & Styling Polish

| # | Milestone | Status |
|---|---|---|
| 1 | Installed shadcn Switch component | ✅ Complete |
| 2 | Birthday/age toggle: checkbox → Switch → custom radio buttons | ✅ Complete |
| 3 | Age selector: NativeSelect dropdown → custom +/− stepper | ✅ Complete |
| 4 | Date picker: styled native input with hidden browser icon + Lucide CalendarDays button | ✅ Complete |
| 5 | Progress bar: added "The basics" / "Showtime" step labels | ✅ Complete |
| 6 | Field labels: `font-medium` → `font-semibold` | ✅ Complete |
| 7 | Description made required in UI (backend remains optional) | ✅ Complete |
| 8 | Step 2 featured hint: plain ★ → Lucide Star icon + "Tap a photo to feature it." | ✅ Complete |
| 9 | Success screen: bare icon → warm `bg-primary/10` circle container | ✅ Complete |
| 10 | `autoFocus` on Name input | ✅ Complete |
| 11 | Focus rings: `ring-ring/40` → `ring-primary/50` across all inputs | ✅ Complete |

---

## Final

| # | Milestone | Status |
|---|---|---|
| 1 | TypeScript check clean | ✅ Complete |
| 2 | Smoke tested end-to-end | ✅ Complete |
| 3 | Committed and pushed | ✅ Complete |
