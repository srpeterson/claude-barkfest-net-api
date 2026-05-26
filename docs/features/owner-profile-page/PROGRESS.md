# Owner Profile Page — Progress

## Status: Complete ✅

---

## Part 1 — Types

| # | Milestone | Status |
|---|---|---|
| 1 | `src/types/owner.ts` — `OwnerDto`, `UpdateOwnerRequest`, `ProfileImageDto` | ✅ Done |

---

## Part 2 — api.ts additions

| # | Milestone | Status |
|---|---|---|
| 1 | `getOwnerById`, `updateOwner`, `uploadOwnerProfileImage`, `removeOwnerProfileImage` | ✅ Done |

---

## Part 3 — AuthContext updates

| # | Milestone | Status |
|---|---|---|
| 1 | Add `profileImageBlobName`, `setProfileImage()`, update `signIn()` | ✅ Done |

---

## Part 4 — LoginDialog updates

| # | Milestone | Status |
|---|---|---|
| 1 | Fetch owner profile after login, pass blob name to `signIn()` | ✅ Done |

---

## Part 5 — RegisterDialog updates

| # | Milestone | Status |
|---|---|---|
| 1 | Fetch owner profile after register + auto-login, pass blob name to `signIn()` | ✅ Done |

---

## Part 6 — UpdateOwnerProfileDialog component

| # | Milestone | Status |
|---|---|---|
| 1 | Step 1 — personal info form (username info line, first/last name, email, display name) | ✅ Done |
| 2 | Display name availability check (skips when value matches current saved name) | ✅ Done |
| 3 | Step 2 — profile image (pre-load existing, single upload, no star) | ✅ Done |
| 4 | Save flow (update info → upload/remove image → update AuthContext → close) | ✅ Done |
| 5 | Loading and error states; spinner on Save button; closes immediately on success | ✅ Done |
| 6 | State reset on dialog close | ✅ Done |

---

## Part 7 — Navbar updates

| # | Milestone | Status |
|---|---|---|
| 1 | Avatar button opens `UpdateOwnerProfileDialog` | ✅ Done |
| 2 | Show profile image thumbnail when `profileImageBlobName` is set | ✅ Done |

---

## Additional work completed in this feature

| Item | Notes |
|---|---|
| Modal → Dialog rename | All components and methods standardised to `Dialog` suffix |
| `validator` npm package | `isEmail()` used in `RegisterDialog` and `UpdateOwnerProfileDialog` |
| `getBlobImageUrl` container param | Optional `containerName` added; owner images use `owner-profile-images` |
| Admin checkbox hidden in `LoginDialog` | Hidden pending admin UI MVP milestone |
| TanStack Query invalidation on display name change | Conditioned on `displayNameChanged` to avoid unnecessary DB hits |
| Blob orphan fix (`DeletePetCommand`) | Deletes all pet image blobs before DB row removal |
| Blob orphan fix (`BatchDeletePetImagesCommand`) | Resolves blob names before `RemoveImages()`, deletes after |

---

## Final

| # | Milestone | Status |
|---|---|---|
| 1 | TypeScript check clean | ✅ Done |
| 2 | Smoke tested end-to-end | ✅ Done |
| 3 | Committed and pushed | ⏳ Pending |
