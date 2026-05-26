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

---

## D8 — No success screen; dialog closes immediately on save

Both `UpdateOwnerProfileDialog` and `AddPetDialog` previously showed an intermediate
success screen for ~1.5 seconds before closing. This was removed in favour of closing
the dialog immediately when the save completes. The "Saving…" / "Making it official…"
spinner state on the submit button is the only loading feedback — sufficient for actions
that complete in under a second.

---

## D9 — flushSync used before async submit handlers

React 18's automatic batching can delay a `setIsSubmitting(true)` render until after a
fast localhost API response has already returned, making the spinner invisible. Both
submit handlers use `flushSync(() => { setIsSubmitting(true); ... })` to force a
synchronous DOM update before the first `await`. This ensures the spinner is always
visible regardless of how fast the server responds.

---

## D10 — "German Shepherd Dog" renamed to "German Shepherd" in DogBreed enum

The display string in `DogBreed.GermanShepherdDog` was shortened to "German Shepherd".
The C# field name is unchanged to avoid breaking references across the codebase.
