# Refactor Plan — Migrate to `Result<T, Error>` (Depth A)

Branch: `enhancement/code-refactor`

This is a living document. Check off items as phases complete. Delete or fold into
`ROADMAP.md` once the migration merges.

---

## Locked decisions

| Decision | Choice |
|---|---|
| Result library | **CSharpFunctionalExtensions** |
| Error representation | **Closed DU hierarchy** (sealed `Error` base + cases) |
| Error location | **`Barkfest.Domain`** (zero-dependency record hierarchy) |
| Depth | **A** — Result spine through Application/API; the domain keeps throwing `DomainException`, lifted at a single boundary adapter via `Result.Try` |
| Breadth | **Vertical-slice-first** — Foundation → Pets → sweep |

**Invariant:** external HTTP behavior is unchanged (same status codes and bodies).
This is an internal architecture change; the API/integration tests are the safety net
and must stay green at every phase.

**Depth A is reversible to B** later: the exception→Result lift is confined to one
adapter, so upgrading the domain to return `Result` is a localized follow-up.

---

## Phase 0 — Foundation (no feature behavior changes)

- [x] 0.1 Add CSharpFunctionalExtensions to `Barkfest.Application` and `Barkfest.API` only (not Domain).
- [x] 0.2 `Error` DU in `src/Barkfest.Domain/Errors/` — abstract `Error` record + sealed cases
      mirroring today's middleware mapping 1:1 (behavior-preserving):
      `NotFoundError(string Entity, object Key, string? Field = null)` → 404,
      `ValidationError(IReadOnlyDictionary<string,string[]> Failures)` → 400,
      `ForbiddenError(string? Message = null)` → 403,
      `DomainRuleError(string Message)` → 400.
      (`ConflictError`/409 dropped — no current 409, would change behavior; add later if deliberate.)
- [x] 0.3 Boundary adapter `src/Barkfest.Application/Common/DomainResult.cs`:
      `Try<T>(Func<T>)` and `Try(Action)→Result<Unit,Error>` catch `DomainException` → `DomainRuleError`,
      let other exceptions propagate. The ONLY sanctioned try/catch in the app.
- [x] 0.4 ✅ **Spike resolved:** `ValidationBehavior` is now dual-mode — returns a failed
      `Result<T, Error>` (via cached-reflection `ResultFailureFactory`) when `TResponse` is
      `Result<,>`, else throws `ValidationException` (legacy path, removed after the sweep).
      Dual-mode is what lets the migration proceed incrementally without breaking un-migrated
      features. Validated by 3 new tests; all 352 Application tests green.
- [x] 0.5 Controller translation `src/Barkfest.API/Extensions/ResultExtensions.cs`:
      `ToActionResult<T>()` (Ok), `ToActionResult<T>(onSuccess)` (Created/custom), `ToNoContentResult<T>()` (204);
      failure → switch on `Error` case → status (`NotFound`→404, `Validation`→400, `Forbidden`→403, `DomainRule`→400),
      `_ => throw` for unmapped. Exhaustiveness test guards the throw arm. Unit (MediatR) is the no-payload value.
- [x] 0.6 `ExceptionHandlingMiddleware` left in place unchanged; its NotFound/Domain/Forbidden/Validation arms go
      dead during the sweep and are removed at the end (only the 500 handler remains).

**Commit:** "Add Result/Error foundation (CSharpFunctionalExtensions, Error DU, translation layer)"

---

## Phase 1 — Pets slice, non-image (prove + lock the pattern)

- [ ] `GetPetByIdQuery` → `Result<PetDto, Error>` (NotFound; visibility → NotFound)
- [ ] `CreatePetCommand` → `Result<Guid, Error>` (validation; domain lift via `DomainResult.Try`)
- [ ] `UpdatePetCommand` → `Result<Success, Error>` (NotFound, Forbidden, validation)
- [ ] `DeletePetCommand` → `Result<Success, Error>`
- [ ] `IncrementPetLikes` / `DecrementPetLikes` → `Result<int, Error>` (map `LikeUpdateResult.PetExists == false` → `NotFoundError`)
- [ ] MediatR request signatures → `IRequest<Result<…, Error>>`; `PetController` actions use `.ToActionResult()`
- [ ] Rewrite Pets handler unit tests; API/integration tests pass unchanged

**Commit:** "Migrate Pets (non-image) handlers to Result" — review/pattern-lock checkpoint.

---

## Phase 2 — Pets images + fold in blob/DB ordering (#6)

- [ ] `AddPetImages` → `Result<AddPetImagesResult, Error>` (whole-op failures as `Error`; per-image
      moderation failures stay in payload, 207 preserved)
- [ ] `RemovePetImage`, `BatchDeletePetImages` → `Result<Success, Error>`
- [ ] Apply #6 ordering: deletes = **save DB first, then blob**; adds = **upload, save, compensating
      blob-delete on failure**. (Orphan-sweeper is a separate future task, not this branch.)
- [ ] Tests for Result mapping and new ordering

**Commit:** "Migrate Pets image handlers to Result and fix blob/DB ordering (#6)"

---

## Phase 3 — Sweep remaining features

- [ ] Owners (CRUD, profile image, visibility)
- [ ] Auth (Register / Login / AdminLogin / Check queries)
- [ ] Administrators (create / delete / password / list)
- [ ] Browse queries
- [ ] Remove dead arms of `ExceptionHandlingMiddleware` (leave only the 500 handler)

**Commit(s):** one per feature area or one sweep commit (TBD).

---

## Phase 4 — Docs

- [ ] CLAUDE.md: rewrite error-handling section (Result contract, `Error` DU, translation layer,
      the single `DomainResult.Try` adapter, middleware's reduced role); update the Exception Handling
      table and "What NOT to use".
- [ ] README / SPEC: only if behavior changed (it shouldn't — note "no external change").

**Commit:** "Update CLAUDE.md for Result-based error handling"

---

## Open spikes (settle early)

1. **ValidationBehavior** generic failed-`Result` construction (Phase 0).
2. **Image partial-success** mapping for `AddPetImages` (Phase 2).

## Rollback points
Every phase ends green and committed. Domain tests stay untouched under Depth A —
their continued passing is a signal the safety net holds.
