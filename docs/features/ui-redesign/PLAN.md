# UI Redesign — Plan

## Overview

Full visual redesign from the Claude Design handoff package. All existing functionality
preserved; only visual presentation changes. Core infrastructure files (AuthContext, useAuth,
api.ts core, index.css tokens, shadcn/ui components) are unchanged.

## Work Items (priority order)

1. **BarkfestMark component** — SVG logo with `inverted` prop (orange bg / white bg variants)
2. **Navbar redesign** — floating orange pill, BarkfestMark inverted, avatar dropdown, guest → /login /register
3. **App.tsx routing** — add /login, /register, /pets/:petId, /manage routes
4. **LoginPage redesign** — split-panel (42% brand / 58% form), navigate → /
5. **RegisterPage redesign** — split-panel, add displayName + confirmPassword, navigate → /
6. **PetCard update** — cursor-pointer + onClick → /pets/:petId
7. **PetDetailPage** — magazine layout, full-bleed hero, floating info card, likes, gallery lightbox
8. **FilterBar mobile** — useIsMobile hook + bottom sheet on mobile
9. **ManagePetsPage** — table layout, bulk delete, edit/delete actions
10. **Dialog button styling** — Cancel/Back → outlined, "Change password →" link in UpdateOwnerProfileDialog
11. **ChangePasswordDialog** — current + new + confirm password, sign out on success
12. **useIsMobile hook** — window.matchMedia maxWidth 768

## Key Decisions

- Post a Pet stays as inline AddPetDialog (not a route)
- Bearer token auth unchanged; split-panel pages use existing login/register logic
- Only api.ts endpoint functions may be added; core infrastructure frozen
