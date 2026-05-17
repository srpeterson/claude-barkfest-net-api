# Barkfest — Documentation

| Document | Purpose |
|---|---|
| [SPEC.md](SPEC.md) | Functional specification — what the application does |
| [PLAN.md](PLAN.md) | Build plan — phased implementation checklist |
| [DECISIONS.md](DECISIONS.md) | Architecture decisions — the why behind every key choice |

## Quick Start

1. Clone the repo
2. Set User Secrets:
   ```
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your-sql-server-connection-string>" --project src/Barkfest.API
   dotnet user-secrets set "ConnectionStrings:AzureBlobStorage" "<your-blob-storage-connection-string>" --project src/Barkfest.API
   ```
3. Run the API — the database migration is applied automatically on startup:
   ```
   dotnet run --project src/Barkfest.API
   ```
4. Open the Scalar API reference at `https://localhost:{port}/scalar/v1`

## Running Tests

```
dotnet test
```

Integration tests require Docker for Testcontainers (SQL Server and Azurite containers).
