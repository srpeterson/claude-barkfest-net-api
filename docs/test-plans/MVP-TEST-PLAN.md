# Barkfest — UI Test Plan

**App URL:** https://gray-rock-0394ee50f.7.azurestaticapps.net  
**Scope:** Owner and public-facing UI only  
**Browsers:** Chrome (desktop and mobile)  
**Tester:** Starting from scratch — no existing accounts or data

-

## Setup

Before starting, open Chrome DevTools and keep the **Network** and **Console** tabs accessible throughout testing. Note any unexpected 4xx/5xx responses or console errors as failures even if the UI appears to handle them gracefully.

-

## Legend

| Symbol | Meaning |
|-|-|
| ☐ | Test case — mark ✅ Pass or ❌ Fail |
| **Expected:** | What should happen |
| **Check:** | Additional things to verify beyond the visible result |

-

## 1. Public Browse (No Login Required)

### 1.1 Home Page Load

☐ **Navigate to the app URL.**  
**Expected:** Home page loads. A grid of pet images is displayed. A navbar is visible with "Sign In" and "Join the Barkfest" buttons. No console errors.  
**Check:** Network tab — initial data requests return `200`.

☐ **Verify filter controls are present.**  
**Expected:** "Show me" dropdown (Doggies / Kitties) and "All Breeds" dropdown are visible and functional without login.

☐ **Filter by pet type — select Doggies.**  
**Expected:** Grid updates to show only dogs. Breeds dropdown updates to show dog breeds only.

☐ **Filter by pet type — select Kitties.**  
**Expected:** Grid updates to show only cats. Breeds dropdown updates to show cat breeds only.

☐ **Filter by breed.**  
**Expected:** Grid updates to show only pets of that breed.

☐ **Reset filters.**  
**Expected:** Grid returns to showing all pets.

☐ **Pagination — if more than one page (9 pets saved) of results exists.**  
**Expected:** Pagination controls are visible and navigating between pages works. Page does not scroll to top unexpectedly.

### 1.2 Mobile (Chrome — use DevTools device emulation or a real device)

☐ **Load the home page on mobile viewport.**  
**Expected:** Layout is responsive. No horizontal scrolling. Navbar, filters, and image grid are usable.

-

## 2. Registration

### 2.1 Happy Path

☐ **Click "Join the Barkfest".**  
**Expected:** Registration dialog opens.

☐ **Complete the form with valid data** (username, first name, last name, email, password ≥ 10 characters, confirm password).  
**Expected:** Form submits successfully. User is logged in automatically. Navbar changes to show "Post a Pet", the owner avatar, and "Sign Out". `201` response in Network tab.

### 2.2 Validation — Required Fields

☐ **Submit the form with all fields blank.**  
**Expected:** Validation errors appear for all required fields. No network request is made.

☐ **Submit with username missing.**  
**Expected:** Validation error on the username field.

☐ **Submit with first name missing.**  
**Expected:** Validation error on the first name field.

☐ **Submit with last name missing.**  
**Expected:** Validation error on the last name field.

☐ **Submit with email missing.**  
**Expected:** Validation error on the email field.

☐ **Submit with an invalid email format** (e.g. `notanemail`, `missing@tld`, `space in@example.com`).  
**Expected:** Validation error on the email field.

☐ **Submit with password fewer than 10 characters.**  
**Expected:** Validation error on the password field. Password strength meter reflects weak password.

☐ **Submit with confirm password not matching.**  
**Expected:** Validation error indicating passwords do not match.

### 2.3 Validation — Business Rules

☐ **Submit with a username that is already taken.**  
**Expected:** Error message indicating the username is not available. `400` or `409` in Network tab.

☐ **Submit with an email address that is already registered.**  
**Expected:** Error message indicating the email is already in use.

☐ **Submit with a password longer than 72 characters.**  
**Expected:** Validation error or graceful rejection.

### 2.4 Display Name (Optional)

☐ **Complete registration without entering a display name.**  
**Expected:** Registration succeeds. No display name shown on pet cards attributed to this owner.

☐ **Complete registration with a display name.**  
**Expected:** Registration succeeds. Display name appears on pet cards attributed to this owner.

-

## 3. Login / Logout

### 3.1 Happy Path

☐ **Click "Sign In" and log in with valid credentials.**  
**Expected:** Login succeeds. Navbar updates to authenticated state (Post a Pet, avatar, Sign Out). `200` response in Network tab. JWT `accessToken` is present in the response body. Token is stored in `sessionStorage` (check Application tab → Session Storage).

### 3.2 Negative Paths

☐ **Attempt login with correct username but wrong password.**  
**Expected:** Error message is displayed. `401` in Network tab. User remains unauthenticated.

☐ **Attempt login with a username that does not exist.**  
**Expected:** Error message is displayed. `401` in Network tab.

☐ **Submit the login form with both fields blank.**  
**Expected:** Validation errors appear. No network request is made.

### 3.3 Logout

☐ **Click "Sign Out" from the profile menu.**  
**Expected:** User is signed out. Navbar returns to unauthenticated state (Sign In, Join the Barkfest). `sessionStorage` is cleared (check Application tab — no token present).

### 3.4 Session Behaviour

☐ **Log in, then close the browser tab and reopen the app.**  
**Expected:** User is not logged in. `sessionStorage` does not persist across tab close.

☐ **Log in, open a second tab to the same URL.**  
**Expected:** Second tab reflects the logged-in state (token is tab-scoped via `sessionStorage` — behaviour may vary; note what you observe).

-

## 4. Owner Profile

### 4.1 Update Personal Info

☐ **Click the owner avatar in the navbar.**  
**Expected:** Update Owner Profile dialog opens showing current owner details pre-filled.

☐ **Update first name, last name, email, and/or phone number with valid values and save.**  
**Expected:** Changes are saved successfully. `200` in Network tab.

☐ **Submit with first name or last name blank.**  
**Expected:** Validation error. Request is not sent.

☐ **Submit with an invalid email format.**  
**Expected:** Validation error on the email field.

☐ **Submit with a phone number in an invalid format** (not E.164, e.g. `12345`).  
**Expected:** Validation error on the phone number field.

☐ **Submit with an email already used by another account.**  
**Expected:** Error message returned from the server. `400` or `409` in Network tab.

### 4.2 Profile Image Upload

☐ **Upload a valid profile image (JPEG or PNG, under 10 MB).**  
**Expected:** Image uploads successfully. Avatar in the navbar updates to show the new profile photo.

☐ **Upload an unsupported file type** (e.g. `.gif`, `.webp`, `.pdf`).  
**Expected:** Upload is rejected. Error message displayed. No request sent or `400` returned.

☐ **Upload a file over 10 MB.**  
**Expected:** Upload is rejected with a file size error before or after submission.

☐ **Upload a new profile image when one already exists.**  
**Expected:** Previous image is replaced. New image appears in the navbar avatar.

☐ **Remove the profile image.**  
**Expected:** Profile image is deleted. Navbar avatar reverts to the placeholder. `204` or `200` in Network tab.

-

## 5. Pet Management

### 5.1 Add Pet — Happy Path

☐ **Click "Post a Pet" and complete Step 1** (name, pet type, breed; optionally description, date of birth).  
**Expected:** Step 1 validates and advances to Step 2 (image upload).

☐ **Upload at least one image in Step 2 and submit.**  
**Expected:** Pet is created. Dialog closes. New pet appears in the public browse grid. `201` in Network tab. First image uploaded is automatically set as the featured image.

☐ **Add a pet with date of birth set to today.**  
**Expected:** Pet is created. Age displays as 0 or "less than a year" (note actual UI behaviour).

### 5.2 Add Pet — Validation

☐ **Submit Step 1 with pet name blank.**  
**Expected:** Validation error on the name field.

☐ **Submit Step 1 without selecting a pet type.**  
**Expected:** Validation error on the pet type field.

☐ **Submit Step 1 without selecting a breed.**  
**Expected:** Validation error on the breed field.

☐ **Select "Dog" as pet type, then select a cat breed (if the UI allows).**  
**Expected:** Either the breed list only shows dog breeds (preventing this), or the server rejects the mismatch with a `400`.

☐ **Submit Step 1 with a date of birth in the future.**  
**Expected:** Validation error — date of birth cannot be in the future.

☐ **Attempt to upload an unsupported file type in Step 2** (e.g. `.gif`, `.webp`).  
**Expected:** Upload is rejected. Error message displayed.

☐ **Attempt to upload a file over 10 MB in Step 2.**  
**Expected:** Upload is rejected with a file size error.

### 5.3 Edit Pet

☐ **Edit an existing pet — change name, description, or date of birth and save.**  
**Expected:** Changes are saved. Updated details are reflected on the pet's detail page. `200` in Network tab.

☐ **Edit a pet and set the name to blank.**  
**Expected:** Validation error. Request not sent.

☐ **Edit a pet and set a date of birth in the future.**  
**Expected:** Validation error.

### 5.4 Delete Pet

☐ **Delete a pet.**  
**Expected:** Pet is removed. It no longer appears in the public browse grid. All associated images are deleted. `204` or `200` in Network tab.

-

## 6. Pet Images

### 6.1 Upload Images

☐ **Upload images up to the 6-image limit.**  
**Expected:** Each batch uploads successfully up to 6 total. Upload controls are disabled or hidden once the limit is reached.

☐ **Attempt to upload images that would exceed the 6-image limit** (e.g. pet has 5 images, attempt to upload 2 more).  
**Expected:** Entire batch is rejected with an error before any processing. `400` in Network tab. Existing images are unaffected.

☐ **Upload multiple images in a single batch.**  
**Expected:** All images in the batch are uploaded successfully. `201` in Network tab.

### 6.2 Featured Image

> Images are managed via the "Manage photos" dialog (Photos tab). Tap an image thumbnail to set it as the featured/cover image — it will display an orange star. Changes are not saved until "Save pet!" is clicked.

☐ **Open the Manage photos dialog and tap a non-featured image to set it as the cover.**  
**Expected:** An orange star appears on the tapped image. The previously featured image loses its star. Only one image has a star at a time.

☐ **Click "Save pet!" after changing the featured image.**  
**Expected:** Changes are saved. `200` in Network tab. The public browse grid and pet detail page immediately reflect the new featured image.

☐ **Change the featured image but click "Back" instead of "Save pet!".**  
**Expected:** Changes are discarded. The featured image remains as it was before.

☐ **Delete the currently featured image (click its black X) and click "Save pet!".**  
**Expected:** Image is deleted. No image is auto-promoted to featured. Pet detail page reflects no featured image. `200` in Network tab.

### 6.3 Delete Images

> Click the black "X" on an image thumbnail to mark it for removal. Click "Save pet!" to confirm. Changes are not applied until saved.

☐ **Click the black X on a single image and click "Save pet!".**  
**Expected:** Image is removed. Remaining images are unaffected. `200` in Network tab. The pet detail page reflects the change immediately.

☐ **Click the black X on multiple images and click "Save pet!".**  
**Expected:** All marked images are removed. `200` in Network tab.

☐ **Click the black X on an image then click "Back" without saving.**  
**Expected:** Changes are discarded. The image is not deleted.

-

## 7. Pet Likes

☐ **While logged out, attempt to like a pet.**  
**Expected:** Like action is not available or is blocked. User is prompted to log in.

☐ **Log in and like another owner's pet.**  
**Expected:** Like count increments by 1. `200` or `201` in Network tab.

☐ **Log in and attempt to like your own pet.**  
**Expected:** Like action is not available on your own pets. No request is sent or the server rejects it.

☐ **Unlike another owner's pet that you have previously liked.**  
**Expected:** Like count decrements by 1. `200` or `204` in Network tab.

☐ **Unlike a pet that has 0 likes.**  
**Expected:** Like count stays at 0. No error displayed. `200` or `204` in Network tab (floors at zero — no exception).

☐ **Like the same pet multiple times.**  
**Expected:** Each click increments the count. The API does not enforce uniqueness — this is by design. Note whether the UI prevents rapid repeated clicks.

-

## 8. Pet Detail Page

### 8.1 Public Viewer (Not Logged In)

☐ **Click on a pet image from the browse grid while not logged in.**  
**Expected:** Pet detail page loads with name, pet type, breed, description, images, age (if date of birth set), and like count. `200` in Network tab. No edit or delete controls are visible.

☐ **Navigate to a pet detail page directly via URL while not logged in.**  
**Expected:** Page loads successfully. No redirect to login.

☐ **Navigate to a pet detail page with an invalid/non-existent pet ID.**  
**Expected:** A 404 or "not found" state is shown. No unhandled error.

### 8.2 Logged-In Owner Viewing Another Owner's Pet

☐ **While logged in, click on a pet that belongs to a different owner.**  
**Expected:** Pet detail page loads as normal. No edit or delete controls are visible. Like controls are available.

### 8.3 Logged-In Owner Viewing Their Own Pet

☐ **While logged in, click on one of your own pets.**  
**Expected:** Pet detail page loads. The main image displays a circle with 3 vertical dots (options menu).

☐ **Click the 3-dot menu on your own pet's main image.**  
**Expected:** A menu appears with "Edit" and "Delete" options.

☐ **Click "Edit" from the 3-dot menu.**  
**Expected:** The edit pet dialog opens pre-filled with the pet's current details.

☐ **Click "Delete" from the 3-dot menu.**  
**Expected:** Pet is deleted. User is returned to the browse grid. The deleted pet no longer appears. `204` or `200` in Network tab.

☐ **Verify the 3-dot menu does not appear on another owner's pet while logged in.**  
**Expected:** No options menu is visible on pets you do not own.

-

## 9. My Pets Page (`/manage`)

### 9.1 Page Load

☐ **Select "My Pets" from the profile menu while logged in.**  
**Expected:** My Pets page loads. A table is displayed with columns: Name (with thumbnail), Type, Age, and Actions. A pet count ("X pets in your profile") is shown. `200` in Network tab.

☐ **Verify the "Back to Barkfest" link.**  
**Expected:** Clicking it returns the user to the public browse home page.

☐ **Attempt to navigate directly to `/manage` while not logged in.**  
**Expected:** User is redirected to the home page and prompted to log in.

### 9.2 Add Pet

☐ **Click the "+ Add Pet" button.**  
**Expected:** The Add Pet dialog opens (same two-step flow as elsewhere). On success the new pet appears in the table and the pet count increments.

### 9.3 View Pet

☐ **Click the eye icon on a pet row.**  
**Expected:** Navigates to that pet's detail page.

### 9.4 Edit Pet

☐ **Click the pencil icon on a pet row.**  
**Expected:** The Edit Pet dialog opens pre-filled with that pet's current details.

☐ **Make a valid change and save.**  
**Expected:** Changes are saved. Updated details are reflected in the table row. `200` in Network tab.

☐ **Open the edit dialog and submit with the pet name blank.**  
**Expected:** Validation error. Request is not sent.

### 9.5 Delete Pet (Single)

☐ **Click the trash icon on a pet row.**  
**Expected:** A confirmation prompt appears. On confirmation, the pet is deleted and removed from the table. Pet count decrements. `204` or `200` in Network tab.

☐ **Cancel the delete confirmation.**  
**Expected:** Pet is not deleted. Table is unchanged.

### 9.6 Batch Delete

☐ **Select multiple pets using the checkboxes and delete.**  
**Expected:** All selected pets are deleted atomically. They are removed from the table. Pet count updates. `200` in Network tab.

☐ **Select all pets using the header checkbox (if available).**  
**Expected:** All rows are selected. Batch delete removes all pets.

☐ **Select a pet via checkbox then deselect it.**  
**Expected:** Checkbox state toggles correctly. Deselected pet is excluded from any batch action.

### 9.7 Show Pets in Gallery Toggle

☐ **Toggle "Show pets in gallery" off.**  
**Expected:** Toggle changes to off state ("Hidden from everyone" or similar). All of your pets are no longer visible in the public browse grid. `200` in Network tab.

☐ **Toggle "Show pets in gallery" back on.**  
**Expected:** Toggle returns to on state ("Visible to everyone"). Your pets reappear in the public browse grid. `200` in Network tab.

☐ **Verify the toggle state persists after navigating away and returning.**  
**Expected:** Toggle reflects the last saved state when the My Pets page is reopened.

-

## 10. Access Control

☐ **While logged out, attempt to navigate to a protected route directly** (e.g. the manage pets page, if it has a direct URL).  
**Expected:** User is redirected to the home page and prompted to log in.

☐ **Log in as Owner A, then attempt to edit or delete a pet belonging to Owner B** (requires knowing Owner B's pet ID — can be obtained from the Network tab during browse).  
**Expected:** Request is rejected. `403` or `404` in Network tab. Owner A cannot modify Owner B's resources.

-

## 11. General / Cross-Cutting

☐ **Check the browser console throughout the entire test session.**  
**Expected:** No unhandled JavaScript errors or unexpected warnings at any point.

☐ **Check the Network tab for any unexpected `500` responses.**  
**Expected:** No `500` Internal Server Errors at any point.

☐ **Test on mobile viewport (Chrome DevTools device emulation — iPhone or Pixel).**  
**Expected:** All dialogs, forms, buttons, and the browse grid are usable on a small screen. No layout overflow or broken interactions.

☐ **Test the app with a slow network (Chrome DevTools → Network → Slow 3G).**  
**Expected:** Loading states are shown where appropriate. The app does not break or show raw errors during slow responses.
