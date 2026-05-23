# Authentication UI — Progress

## Status: Complete

---

## Part 1 — API: HttpOnly Cookie Auth

| # | Milestone | Status |
|---|---|---|
| 1 | Login endpoints set HttpOnly cookie | ✅ Complete |
| 2 | Logout endpoint added | ✅ Complete |
| 3 | JWT read from cookie in `AddJwtBearer` | ✅ Complete |
| 4 | Tests updated and passing | ✅ Complete |

---

## Part 2 — UI: Auth Context and Modals

| # | Milestone | Status |
|---|---|---|
| 1 | `AuthContext.tsx` — auth state, `signIn`, `signOut`, modal state | ✅ Complete |
| 2 | `useAuth.ts` hook | ✅ Complete |
| 3 | `api.ts` — `credentials: 'include'`, `setUnauthorizedHandler`, `login`, `adminLogin`, `logout` | ✅ Complete |
| 4 | `LoginModal.tsx` — fields, admin checkbox (disabled), validation, reset on close | ✅ Complete |
| 5 | `RegisterModal.tsx` — all fields, zxcvbn strength, confirm password, submit guard | ✅ Complete |
| 6 | `Navbar.tsx` — three-state render (unauthenticated / owner / admin) | ✅ Complete |
| 7 | `ProtectedRoute.tsx` — owner-only gate with login modal on redirect | ✅ Complete |
| 8 | `App.tsx` — 401 handler registration | ✅ Complete |
| 9 | TypeScript check clean | ✅ Complete |
| 10 | UI smoke tested | ✅ Complete |
| 11 | Committed and pushed | ✅ Complete |
