# PROGRESS.md — Consolidate Migrations into a Single InitialCreate

Branch: `infra/consolidate-migrations`
Roadmap item: #17

---

## Milestones

- [x] Delete all files under `src/Barkfest.Persistence/Migrations/`
- [x] Run `dotnet ef migrations add InitialCreate` to generate the single migration (`20260604150555_InitialCreate`)
- [x] Run the full test suite — 754 tests passing, zero failures
- [x] Delete the `barkfest-sql-data` Docker volume locally
- [x] Start Aspire — `MigrateAsync()` applied the single migration cleanly; confirmed one row in `__EFMigrationsHistory`
- [x] Smoke test passed locally
- [x] Drop and recreate the Azure SQL database via Portal (no data loss — database was empty)
- [x] Push branch → merge to `main` → GitHub Actions deployed → `MigrateAsync()` applied on Azure (`0000036` Running)
- [x] Fixed long-standing connection string naming mismatch — aligned Aspire (`barkfest-db`), `DependencyInjection.cs`, and `api.yml`

## Complete
