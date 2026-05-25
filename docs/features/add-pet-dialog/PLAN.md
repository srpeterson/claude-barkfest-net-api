# Add Pet Dialog — Plan

## Goal

Build a multi-step "Add Pet" modal dialog accessible from the authenticated owner
Navbar. The dialog walks the owner through two steps — pet details then image upload —
and submits to the existing API endpoints. No new API work is required; all endpoints
are already in place.

---

## Scope

| # | Change | Layer |
|---|---|---|
| 1 | `src/types/pet.ts` — TypeScript types for create pet request and image upload result | UI types |
| 2 | `src/lib/api.ts` — `createPet()`, `addPetImages()`, and `setFeaturedImage()` functions | UI lib |
| 3 | `AddPetDialog.tsx` — multi-step dialog component | UI component |
| 4 | `Navbar.tsx` — wire "Post a Pet" button to open `AddPetDialog` | UI component |

---

## Part 1 — Types (`src/types/pet.ts`)

- `CreatePetRequest` — `{ name, petType, breed, dateOfBirth?, description? }`
- `AddPetImagesResult` — shape of the `POST /v1/pets/{id}/images` response (per-image success/failure array)

---

## Part 2 — `api.ts` additions

### 2.1 — `createPet(data: CreatePetRequest): Promise<string>`
- `POST /v1/pets`
- Returns the new pet `id` (extracted from the `Location` header or response body)

### 2.2 — `addPetImages(petId: string, files: File[]): Promise<AddPetImagesResult>`
- `POST /v1/pets/{petId}/images`
- Sends `multipart/form-data` with each file appended under the same field name

### 2.3 — `setFeaturedImage(petId: string, imageId: string): Promise<void>`
- `PUT /v1/pets/{petId}/images/{imageId}/featured`

---

## Part 3 — `AddPetDialog.tsx`

Self-contained modal with two steps. All state resets on close.

### 3.1 — Step 1: Pet details form

| Field | Type | Required | Notes |
|---|---|---|---|
| Name | Text input | Yes | Max `Pet.NameMaxLength` chars |
| Pet Type | Dropdown | Yes | `Dog` / `Cat` |
| Breed | Dropdown | Yes | Fetched via `getBrowseBreeds(petType)` — same query as `FilterBar`; `staleTime: Infinity`; sorted A–Z with Mixed/Other pinned last; resets when Pet Type changes |
| Date of Birth | Date input | No | Cannot be in the future; `DateOnly` format sent to API |
| Description | Textarea | No | No max length enforced in UI |

- "Next" button disabled until Name, Pet Type, and Breed are filled
- "Next" advances to Step 2

### 3.2 — Step 2: Image upload

- File input: `accept="image/jpeg,image/jpg,image/png"`, multiple
- Thumbnail preview grid — each thumbnail has a remove button
- Minimum 1 image before "Submit" is enabled
- Maximum `Pet.MaxImages` (6) images — additional files ignored beyond the limit
- Featured image selection: click a thumbnail to designate it as featured (highlighted with a star or border); defaults to the first image added

### 3.3 — Submission flow

1. `POST /v1/pets` — create the pet, capture the returned `petId`
2. `POST /v1/pets/{petId}/images` — upload all selected files
3. `PUT /v1/pets/{petId}/images/{imageId}/featured` — set the designated featured image
4. On full success: show inline success message, then close dialog after a short delay
5. On any failure: show inline error message; do not close the dialog

### 3.4 — Navigation and reset

- Back button on Step 2 returns to Step 1 (preserving form values)
- Closing the dialog (X button or backdrop click) resets all state via `useEffect`
- Submission in progress: all inputs and buttons disabled; loading spinner on Submit

---

## Part 4 — Navbar wire-up

- `Navbar.tsx` — "Post a Pet" button in the authenticated owner state opens `AddPetDialog`
- Dialog open/close state owned by `Navbar` (or lifted to a shared context if needed)
