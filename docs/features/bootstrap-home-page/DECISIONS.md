# Decisions — Bootstrap Home Page

Decisions made during the Bootstrap Home Page feature.

---

## Font Stack — DM Sans + Playfair Display

**Decision:** Replace Geist Variable with DM Sans Variable (body) + Playfair Display (headings).

**Rationale:** Geist was a default scaffold font — a developer/tech typeface with no deliberate brand intent. DM Sans + Playfair Display is a better fit for Barkfest as a consumer lifestyle app: DM Sans is warm, rounded, and highly readable for UI text; Playfair Display adds personality and a premium feel to headings.

**Packages:**
- `@fontsource-variable/dm-sans` — variable font, single file covers all weights
- `@fontsource/playfair-display` — static font, three weight files (400, 600, 700)

**`@theme` font variables will be updated during the full handoff import in Step 2.**

---

## Minimum one image on pet creation — deferred to own branch

**Decision:** Enforce the "minimum 1 image" rule at the API layer, not just the UI layer.

**Rationale:** A UI-only convention can be bypassed by any client. The rule belongs at
the API layer so it is enforced regardless of how the API is called.

**Approach:** Combine `POST /v1/pets` into a single multipart request (pet metadata +
at least 1 image). Deferred to `feature/require-pet-image-on-create` to keep this
feature branch focused on UI bootstrapping only.

**Tracked in:** `docs/ROADMAP.md` item 6.

---

## `Pet` type uses snake_case — deferred to API wiring phase

**Decision:** Accept the Base44-derived `Pet` type in `src/types/pet.ts` as-is for this phase.

**Detail:** The type uses snake_case property names (`image_url`, `owner_id`, `created_date`)
inherited from the Base44 design. The Barkfest API returns camelCase (`imageUrl`, `ownerId`,
`createdAt`). This mismatch will be resolved when the home page is wired to the real API
in a later phase — either by updating the type or by mapping at the API client layer.

**Action required in the API wiring phase:** Update `src/types/pet.ts` to match the actual
API response shape and update any components that reference the snake_case properties.
