# Owner Profile Page — Plan

## Goal

Build an `UpdateOwnerProfile` dialog — a 2-step modal (modelled on `AddPetDialog`) that
lets an authenticated owner update their personal info and profile image. The dialog
opens from the Navbar avatar button. Fields are pre-filled with the owner's current data
on open. The Navbar avatar updates immediately after a successful save.

No new backend endpoints are required. All API endpoints already exist.
Change password is deferred to Roadmap item 18.

---

## Scope

| # | Change | Layer |
|---|---|---|
| 1 | `src/types/owner.ts` — `OwnerDto`, `UpdateOwnerRequest` | UI types |
| 2 | `src/lib/api.ts` — `getOwnerById`, `updateOwner`, `uploadOwnerProfileImage`, `removeOwnerProfileImage` | UI lib |
| 3 | `AuthContext.tsx` — add `profileImageBlobName`, `setProfileImage()`, update `signIn()` | UI context |
| 4 | `LoginModal.tsx` — fetch owner profile after login, pass blob name to `signIn()` | UI component |
| 5 | `RegisterModal.tsx` — same fetch after register + auto-login | UI component |
| 6 | `UpdateOwnerProfile.tsx` — 2-step dialog | UI component |
| 7 | `Navbar.tsx` — open dialog from avatar button; show profile image thumbnail | UI component |

---

## Part 1 — Types (`src/types/owner.ts`)

```ts
export interface ProfileImageDto {
  blobName: string
  contentType: string
}

export interface OwnerDto {
  id: string
  username: string
  displayName: string | null
  firstName: string
  lastName: string
  email: string
  phoneNumber: string | null
  isVisible: boolean
  profileImage: ProfileImageDto | null
  createdAt: string
}

export interface UpdateOwnerRequest {
  firstName: string
  lastName: string
  email: string
  phoneNumber?: string | null
  displayName?: string | null
}
```

---

## Part 2 — `api.ts` additions

| Function | Endpoint | Notes |
|---|---|---|
| `getOwnerById(id)` | `GET /v1/owners/{id}` | Returns `OwnerDto` |
| `updateOwner(id, data)` | `PUT /v1/owners/{id}` | Returns `void` (204) |
| `uploadOwnerProfileImage(id, file)` | `POST /v1/owners/{id}/profile-image` | Multipart — reuses `requestMultipart` pattern |
| `removeOwnerProfileImage(id)` | `DELETE /v1/owners/{id}/profile-image` | Returns `void` (204) |

---

## Part 3 — `AuthContext.tsx` updates

Add `profileImageBlobName: string | null` to `AuthState`, persisted to `sessionStorage`
under key `barkfest_profile_image`.

Changes:
- `signIn(accountId, accountType, token, profileImageBlobName?)` — optional 4th parameter
- New method: `setProfileImage(blobName: string | null)` — updates state + sessionStorage
- `signOut()` — `sessionStorage.clear()` already wipes everything; no change needed

---

## Part 4 — `LoginModal.tsx` updates

After `login()` resolves:
1. Call `setAuthToken(result.accessToken)` immediately (before React's `useEffect` runs)
2. Call `getOwnerById(result.accountId)` — wrapped in try/catch; login is never blocked
3. Extract `owner.profileImage?.blobName ?? null`
4. Pass it as the 4th argument to `signIn()`

---

## Part 5 — `RegisterModal.tsx` updates

Same pattern after the auto-login that follows `register()`:
1. `setAuthToken(result.accessToken)`
2. `getOwnerById(result.accountId)` — try/catch; new owner will always have `null` profile image
3. Pass `null` (or fetched value) to `signIn()`

---

## Part 6 — `UpdateOwnerProfile.tsx`

Self-contained 2-step modal. Fetches current owner data on open. All state resets on close.

### Step 1 — Personal information

Mirrors `RegisterModal` fields — same visual style, labels, and validation behaviour.
No password fields (deferred). No phone number (not in RegisterModal).

| Field | Type | Notes |
|---|---|---|
| Username | Read-only info line | Not a text input — displayed as a labelled value |
| First name | Text input | Required |
| Last name | Text input | Required |
| Email | Text input | Required |
| Display name | Text input | Optional; availability check fires only when value differs from the owner's current saved display name |

Display name availability check behaviour:
- If the field is empty or matches the owner's current `displayName` → no API call, no indicator shown
- If changed to a new value → debounced `checkDisplayName` fires, showing Checking… / ✓ Available / Already taken

"Next" button disabled until First name, Last name, and Email are filled and no availability check is in flight.

### Step 2 — Profile image

- If the owner already has a profile image: pre-load it as the preview on open
- If no existing image: start blank
- Single image only (no multi-upload)
- No featured star
- Upload is optional — owner can proceed to Save without changing the image
- If the owner clears the pre-loaded image: on Save, `DELETE /v1/owners/{id}/profile-image` is called

### Save flow

1. `PUT /v1/owners/{id}` — always called (personal info)
2. If a new image was selected: `POST /v1/owners/{id}/profile-image`
3. If existing image was cleared: `DELETE /v1/owners/{id}/profile-image`
4. Call `authContext.setProfileImage(newBlobName)` to update the Navbar avatar
5. On full success: show inline success message, then close dialog after a short delay
6. On failure: show inline error message; do not close

### Reset on close

All form fields, image state, loading, and error state reset when the dialog closes.

---

## Part 7 — `Navbar.tsx` updates

- Convert the `UserCircle` `<button>` to open `UpdateOwnerProfile` dialog (same pattern as "Post a Pet" opening `AddPetDialog`)
- When `profileImageBlobName` is set in `AuthContext`: render a circular `<img>` (40×40, `object-cover`, `rounded-full`) resolved via `imageUrl(blobName)` in place of the `UserCircle` icon
- The circular avatar serves as the button to open the dialog

---

## Decisions

See `DECISIONS.md` for rationale on key choices made during the Q&A session.
