# UI Redesign — Progress

## Status: In Progress

## Milestones

- [x] Feature docs created (`docs/features/ui-redesign/`)
- [x] BarkfestMark component (`src/components/BarkfestMark.tsx`)
- [x] useIsMobile hook (`src/hooks/useIsMobile.ts`)
- [x] Animations added to index.css (`dialog-appear`, `sheet-in`, `drawer-in`, `backdrop-in`)
- [x] Navbar redesign — floating orange pill, BarkfestMark inverted, avatar dropdown (My Pets → /manage, Edit Profile → dialog, Sign Out), guest → /login /register
- [x] App.tsx routing updates — /login, /register, /pets/:petId, /manage
- [x] LoginPage redesign — split-panel (42% brand / 58% form), navigate → /
- [x] RegisterPage redesign — split-panel, displayName + debounced availability check, confirmPassword, navigate → /
- [x] PetCard navigation — cursor-pointer + onClick → /pets/:petId
- [x] New API endpoints — getPetDetail, updatePet, deletePet, likePet, unlikePet, getOwnerPets, changePassword
- [x] PetDetailPage (`src/features/pets/PetDetailPage.tsx`) — magazine layout, full-bleed hero, floating info card, likes, lightbox, owner kebab menu
- [x] FilterBar mobile bottom sheet — useIsMobile + pet type chips + breed search + Show results CTA
- [x] ManagePetsPage (`src/features/pets/ManagePetsPage.tsx`) — table layout, select-all + indeterminate, bulk delete bar, action icons
- [x] Dialog button styling pass — Cancel/Back → outlined (border + transparent bg) in AddPetDialog + UpdateOwnerProfileDialog
- [x] BarkfestMark in dialog headers (AddPetDialog, UpdateOwnerProfileDialog, ChangePasswordDialog)
- [x] ChangePasswordDialog (`src/components/ChangePasswordDialog.tsx`) — zxcvbn strength, sign out on success, navigate → /login
- [x] "Change password →" link in UpdateOwnerProfileDialog Step 1
- [x] All 9 UI tests passing
- [x] TypeScript build clean (tsc + vite)

## Remaining

- [ ] Git commit + PR
- Note: EditPetModal (pre-filled 2-step dialog) deferred — referenced by kebab menu on PetDetail and pencil on ManagePets but not yet implemented
