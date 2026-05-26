# Owner Profile Page — Decisions

## D1 — Dialog over page

Profile management is implemented as a modal dialog (`UpdateOwnerProfile`) rather than a
dedicated `/profile` route. The dialog opens from the Navbar avatar button. This keeps the
management flow inline and consistent with `AddPetDialog` — owners never leave the page
they are on.

---

## D2 — No password change in this dialog

Changing a password is a security-sensitive action that requires verifying the current
password first. Adding three extra password fields to an already-busy Step 1 was deemed
too cluttered. Password change is added to the Roadmap as item 18 for a dedicated future
flow.

---

## D3 — Username shown as read-only info line

Username is the login identity and cannot be changed after registration. It is displayed
as a labelled read-only value — not a disabled input — to make it clear it is informational
rather than a field the owner has chosen not to fill in.

---

## D4 — Display name availability check skips current value

In the update flow the owner's current display name would incorrectly show as "Already
taken" if the check fired unconditionally. The check only reaches the API when the typed
value differs from the owner's currently saved display name. All the logic lives in the UI
— no API change is needed.

---

## D5 — Clearing image on Step 2 and saving deletes the blob

If the owner removes the pre-loaded profile image preview and completes the save,
`DELETE /v1/owners/{id}/profile-image` is called. The existing handler already deletes
the blob from Azure Blob Storage before clearing the DB record.

---

## D6 — Profile image blob name stored in AuthContext

`profileImageBlobName` is added to `AuthContext` and persisted to `sessionStorage`. This
allows the Navbar to resolve the avatar URL via `imageUrl()` without an extra network
request on every render.

On login and on register, the owner's profile is fetched immediately after the access
token is received (using `setAuthToken` before the React `useEffect` runs) and the blob
name is passed into `signIn()`. If the fetch fails, login proceeds normally with a `null`
blob name — the `UserCircle` placeholder is shown instead.

---

## D7 — Blob cleanup fixed for DeletePet and BatchDeletePetImages

Discovered during Q&A that `DeletePetCommand` and `BatchDeletePetImagesCommand` did not
delete Azure blobs when removing pets or pet images. Fixed in this branch before building
the profile dialog — `RemovePetImageCommand` and `RemoveOwnerProfileImageCommand` were
already correct.
