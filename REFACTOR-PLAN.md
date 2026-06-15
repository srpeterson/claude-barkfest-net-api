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

## Phase 1 — Pets slice, non-image (prove + lock the pattern) ✅

- [x] `GetPetByIdQuery` → `Result<PetDto, Error>` (NotFound; visibility → NotFound)
- [x] `CreatePetCommand` → `Result<Guid, Error>` (validation; domain lift via `DomainResult.Try`)
- [x] `UpdatePetCommand` → `Result<Unit, Error>` (NotFound, Forbidden, validation; mutations lifted)
- [x] `DeletePetCommand` → `Result<Unit, Error>` (NotFound, Forbidden; blob ordering unchanged, fixed in Phase 2)
- [x] `IncrementPetLikes` / `DecrementPetLikes` → `Result<int, Error>` (map `LikeUpdateResult.PetExists == false` → `NotFoundError`)
- [x] MediatR request signatures → `IRequest<Result<…, Error>>`; `PetController` actions use `.ToActionResult()` / `.ToNoContentResult()`
- [x] Rewrite Pets handler unit tests; API/integration tests pass unchanged (773 total green)
- [x] Locked idioms: implicit `return new XError(...)` / `return value`; `Result<Unit, Error>` for no-payload commands (204);
      no try/catch in handlers except `DomainResult.Try`; visibility/active checks → `NotFoundError` (no existence leak)
- [x] Naming: renamed Pet command/query params and all `PetController` action params/routes `id`/`Id` → `petId`/`PetId`.
      (Entity & DTO `Id` properties intentionally unchanged — DTO `Id` is the JSON contract; Owner/Admin params handled in Phase 3.)

**Commit:** "Migrate Pets (non-image) handlers to Result" — review/pattern-lock checkpoint.

---

## Phase 2 — Pets images + fold in blob/DB ordering (#6) ✅

- [x] `AddPetImages` → `Result<AddPetImagesResult, Error>` (whole-op failures as `Error`; per-image
      moderation failures stay in payload, 207 preserved; slot-exceeded → `DomainRuleError`)
- [x] `RemovePetImage`, `BatchDeletePetImages`, `SetFeaturedImage` → `Result<Unit, Error>`
- [x] Apply #6 ordering: deletes (`RemovePetImage`, `BatchDeletePetImages`, `DeletePet`) =
      **save DB first, then blob**; `AddPetImages` = **upload, save, compensating blob-delete on save failure**
- [x] Controller: `AddImages` returns `ToActionResult()` on failure, else 207/201 from payload;
      other image actions use `ToNoContentResult()`
- [x] Tests for Result mapping and ordering; integration tests (Azurite) verify end-to-end. 773 total green.

Note: orphan-blob sweeper remains a separate future task (compensation is best-effort), not this branch.

**Commit:** "Migrate Pets image handlers to Result and fix blob/DB ordering (#6)"

---

## Phase 3 — Sweep remaining features (one commit per feature)

- [x] Owners (GetById, GetAll, Update, Delete, ChangePassword, SetVisibility, profile image upload/remove,
      CreateOwner) + GetPetsByOwnerId; params `id`/`Id` → `ownerId`/`OwnerId`; #6 ordering applied to
      owner profile-image upload/remove. `GetAllPets` left as plain list (no failure path, unwired).
- [x] Auth: Register / Login / AdminLogin → Result (DomainRuleError for duplicates; NotFoundError for
      bad credentials; ForbiddenError for inactive). CheckUsername / CheckDisplayName left as plain `bool`
      (infallible). AuthController translates via ToActionResult.
- [x] Administrators: CreateAdministrator, DeleteAdministrator, UpdateAdministratorPassword, SetOwnerActive,
      GetAllAdministrators → Result. Admin gate / self-delete → ForbiddenError; duplicates → DomainRuleError.
      Params `id`/`Id` → `administratorId`/`AdministratorId` (owner-active route → `ownerId`).
- [x] Browse queries: **no change** — `GetBrowseImages`, `GetBrowseBreeds`, `GetBrowsePetTypes` are infallible
      (invalid input yields an empty result, never an error), so they stay plain returns.
- [x] Middleware: removed the dead `NotFoundError`/`ForbiddenError` arms (zero throw sites remain) and
      **deleted** the now-unused `NotFoundException` and `ForbiddenException` classes. Kept the
      `DomainException` arm (backstop for any escape past `DomainResult.Try`), the `ValidationException` arm
      (behavior's legacy non-Result path), and the 500 catch-all.
- [x] Test-naming cleanup: renamed 16 handler-test methods `..._Throws_{NotFound,Forbidden}Exception` →
      `..._Returns_{NotFound,Forbidden}Error` to match the new return-based behavior (CLAUDE.md convention).

Note: `CreateOwnerCommand` was dead code (unwired — no controller route; used `new Owner()` instead of
the `Owner.Create()` factory; never set username/password) and has been **deleted** (command, validator,
and both test files). `CLAUDE.md` examples that referenced it were repointed to `CreatePetCommand`.

**Commit(s):** one per feature area.

---

## Phase 4 — Docs ✅

- [x] CLAUDE.md: rewrote the Exception Handling section into an **Error Handling** section (Result
      contract, `Error` DU table, `DomainResult.Try`, `ResultExtensions` translation, dual-mode
      `ValidationBehavior`, middleware-as-backstop); updated the MediatR section (Result return types +
      "infallible queries stay plain"), the Validation section, the Command/Query type-convention
      examples, the Shouldly examples + test-naming note, the "What NOT To Use" table, and the
      authorization/business-rule lines that referenced the deleted exceptions.
- [x] README / SPEC: **no change** — external HTTP behavior is unchanged (same status codes/bodies).

**Commit:** "Update CLAUDE.md for Result-based error handling"

---

## Open spikes (settle early)

1. **ValidationBehavior** generic failed-`Result` construction (Phase 0).
2. **Image partial-success** mapping for `AddPetImages` (Phase 2).

## Rollback points
Every phase ends green and committed. Domain tests stay untouched under Depth A —
their continued passing is a signal the safety net holds.
