# Authentication UI — Plan

## Goal

Implement Login and Register screens in `barkfest-ui`. Switch the API to issue the
JWT via HttpOnly cookie on login (rather than the response body), add a logout endpoint,
and build the frontend auth flow on top of it.

---

## Part 1 — API: HttpOnly Cookie Auth

### 1.1 — Switch login to set HttpOnly cookie

- `AuthController` — `POST /v1/auth/login`: after successful login, set the JWT
  via `Response.Cookies.Append()` with `HttpOnly = true`, `Secure = true`,
  `SameSite = Strict`; return owner ID and expiry only (no token in body)
- `AuthController` — `POST /v1/auth/admin/login`: same cookie approach for admin JWT
- `AuthTokenDto` — remove `AccessToken` field; return only `AccountId` and `ExpiresAt`

### 1.2 — Add logout endpoint

- `AuthController` — `POST /v1/auth/logout`: clear the cookie by setting it expired;
  no auth required (clearing an absent cookie is a no-op)

### 1.3 — Wire JWT from cookie

- `AddJwtBearer` configuration — add `OnMessageReceived` event to read the token
  from the cookie instead of the `Authorization` header

---

## Part 2 — UI: Auth Context and Modals

### 2.1 — Auth context

- `AuthContext.tsx` — React context providing `isAuthenticated`, `accountId`,
  `accountType ('owner' | 'admin' | null)`, `signIn`, `signOut`, modal state
- `signIn(accountId, accountType)` — writes to `sessionStorage` (auth flags only, not the token)
- `signOut()` — clears `sessionStorage`; calls `POST /v1/auth/logout` to clear the cookie
- `useAuth.ts` — thin wrapper hook over `useAuthContext()`

### 2.2 — `api.ts` updates

- All requests use `credentials: 'include'` so the browser sends the HttpOnly cookie cross-origin
- `setUnauthorizedHandler(fn)` — registers a callback fired on any 401 response
- `login()` and `adminLogin()` — call respective auth endpoints; response has `accountId` and `expiresAt` only
- `logout()` — calls `POST /v1/auth/logout`
- Error handling — parses JSON problem details (`detail` || `title`), falls back to raw text

### 2.3 — Login modal

- `LoginModal.tsx` — username + password inputs with show/hide toggle
- Submit disabled until both fields have content
- Admin checkbox ("I am an Administrator") — present but **disabled** until admin UI phase
- On success: `signIn(result.accountId, 'owner' | 'admin')` + `closeModal()`
- On failure: generic "Invalid username or password."
- Fields + error reset on modal close via `useEffect`; `isAdmin` checkbox state persists

### 2.4 — Register modal

- `RegisterModal.tsx` — first name, last name, email, username, password, confirm password
- Password strength meter via `zxcvbn`; score ≥ 2 required to submit
- Confirm password field; submit blocked when passwords do not match
- Submit disabled until all required fields are filled
- On success: `register()` → `login()` → `signIn()` + `closeModal()`
- On failure: generic "Woof! Something went wrong! Check your details and try again."
- All fields reset on modal close via `useEffect`

### 2.5 — Navbar

- Three states: unauthenticated → Sign In button; owner → Post a Pet + Sign Out;
  admin → "Logged in as Administrator" + Sign Out

### 2.6 — Protected route

- `ProtectedRoute.tsx` — redirects to `/` unless `isAuthenticated && accountType === 'owner'`
- Admin users are not permitted on owner routes (admin UI is a separate future feature)
- Opens login modal via `useEffect` if unauthenticated

### 2.7 — 401 interception

- `App.tsx` registers `setUnauthorizedHandler(() => { signOut(); openLoginModal() })`
- Any expired token automatically signs the user out and prompts re-login
