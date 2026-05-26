# Owner Profile Page — Progress

## Status: In progress

---

## Part 1 — Types

| # | Milestone | Status |
|---|---|---|
| 1 | `src/types/owner.ts` — `OwnerDto`, `UpdateOwnerRequest`, `ProfileImageDto` | ⬜ Not started |

---

## Part 2 — api.ts additions

| # | Milestone | Status |
|---|---|---|
| 1 | `getOwnerById`, `updateOwner`, `uploadOwnerProfileImage`, `removeOwnerProfileImage` | ⬜ Not started |

---

## Part 3 — AuthContext updates

| # | Milestone | Status |
|---|---|---|
| 1 | Add `profileImageBlobName`, `setProfileImage()`, update `signIn()` | ⬜ Not started |

---

## Part 4 — LoginModal updates

| # | Milestone | Status |
|---|---|---|
| 1 | Fetch owner profile after login, pass blob name to `signIn()` | ⬜ Not started |

---

## Part 5 — RegisterModal updates

| # | Milestone | Status |
|---|---|---|
| 1 | Fetch owner profile after register + auto-login, pass blob name to `signIn()` | ⬜ Not started |

---

## Part 6 — UpdateOwnerProfile component

| # | Milestone | Status |
|---|---|---|
| 1 | Step 1 — personal info form (username info line, first/last name, email, display name) | ⬜ Not started |
| 2 | Display name availability check (skips when value matches current saved name) | ⬜ Not started |
| 3 | Step 2 — profile image (pre-load existing, single upload, no star) | ⬜ Not started |
| 4 | Save flow (update info → upload/remove image → update AuthContext) | ⬜ Not started |
| 5 | Loading, success, and error states | ⬜ Not started |
| 6 | State reset on dialog close | ⬜ Not started |

---

## Part 7 — Navbar updates

| # | Milestone | Status |
|---|---|---|
| 1 | Avatar button opens `UpdateOwnerProfile` dialog | ⬜ Not started |
| 2 | Show profile image thumbnail when `profileImageBlobName` is set | ⬜ Not started |

---

## Final

| # | Milestone | Status |
|---|---|---|
| 1 | TypeScript check clean | ⬜ Not started |
| 2 | Smoke tested end-to-end | ⬜ Not started |
| 3 | Committed and pushed | ⬜ Not started |
