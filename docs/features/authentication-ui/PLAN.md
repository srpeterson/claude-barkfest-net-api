# Authentication UI — Plan

## Goal

Implement Login and Register screens in `barkfest-ui`. Switch the API to issue the
JWT via HttpOnly cookie on login (rather than the response body), add a logout endpoint,
and build the frontend auth flow on top of it.

---

## Part 1 — API: HttpOnly Cookie Auth (own commit)

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

## Part 2 — UI: Login and Register screens

_To be planned after Part 1 is committed._
