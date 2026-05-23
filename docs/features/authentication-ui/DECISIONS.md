# Authentication UI — Decisions

Decisions made during the Authentication UI feature.

---

## HttpOnly cookie for JWT storage

**Decision:** The JWT is set as an HttpOnly cookie on login rather than returned
in the response body.

**Reason:** sessionStorage and localStorage are accessible to any JavaScript running
on the page — an XSS vulnerability anywhere in the frontend can silently exfiltrate
a token stored there. An HttpOnly cookie cannot be read by JavaScript at all; the
browser attaches it to requests automatically. Combined with `Secure` (HTTPS only)
and `SameSite=Strict` this is the correct default for any application handling
authenticated sessions. This decision was established in `docs/DECISIONS.md` during
Phase 12 and is implemented here.
