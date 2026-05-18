# Barkfest

A .NET 10 Clean Architecture pet management API.
Owners register themselves and their pets. Relational data lives in SQL Server via EF Core.
Pet and owner profile images are stored in Azure Blob Storage.

---

## Documentation

| Document | Purpose |
|---|---|
| [docs/SPEC.md](docs/SPEC.md) | Functional specification — what the application does |
| [docs/PLAN.md](docs/PLAN.md) | Build plan — phased implementation checklist |
| [docs/DECISIONS.md](docs/DECISIONS.md) | Architecture decisions — the why behind every key choice |

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (Included with Visual Studio 2026)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) — must be running before starting the app
- [Git](https://git-scm.com/downloads)
- **EF Core CLI tools** (`dotnet ef`) — minimum version **10.0.7**

  The repo includes a `dotnet-tools.json` manifest pinned to `10.0.7`.
  Install the exact version with:
  ```bash
  dotnet tool restore
  ```
  To verify the install:
  ```bash
  dotnet ef --version
  ```
  You should see `10.0.7` (or later). If `dotnet ef` is not recognised, make sure
  `~/.dotnet/tools` is on your `PATH`.

### (Highly Recommended) Pull Docker images before running the app for the first time

> **Do this before running the AppHost.** Do not rely on Aspire to pull images
> automatically on first run — depending on your network and DNS configuration the pull
> can be very slow, hang indefinitely, or fail silently, leaving you with a broken startup
> and no obvious error message.

Run these two commands while Docker Desktop is running:

```bash
docker pull mcr.microsoft.com/mssql/server:2022-latest
docker pull mcr.microsoft.com/azure-storage/azurite:latest
```

Each pull may take a few minutes on first download. When complete, each command prints
either `Status: Downloaded newer image` (new download) or `Status: Image is up to date`
(already cached) — either means the image is ready.

To verify both images are available locally:

```bash
docker images
```

You should see `mssql/server` and `azurite` in the list. Once they are present, Aspire
will start the containers immediately on every subsequent run without any network download.

> **Note:** If the SQL Server image pull hangs or fails with a network error, try disabling
> IPv6 on your Windows network adapter: Control Panel → Network and Internet → Network
> Connections → right-click your active adapter → Properties → uncheck
> **Internet Protocol Version 6 (TCP/IPv6)** → OK.

---

## Running Locally

This project uses [.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/) to orchestrate
local infrastructure. Aspire starts SQL Server and Azurite containers automatically and injects
all connection strings — no manual configuration is required.

1. Make sure Docker Desktop is running
2. Start the AppHost:
   ```bash
   dotnet run --project src/Barkfest.AppHost
   ```
3. The Aspire dashboard opens in your browser — it shows the status of all services
4. Once the API is healthy, open the Scalar API reference at:
   ```
   https://localhost:{port}/scalar/v1
   ```
   (the port is shown in the Aspire dashboard next to the `barkfest-api` resource)

The database migration is applied automatically on startup. Container data persists across
restarts — SQL Server and Azurite volumes survive `docker stop` and machine reboots.

---

## Running Tests

```bash
dotnet test
```

All tests manage their own infrastructure via Testcontainers (SQL Server and Azurite containers
are started and torn down automatically per test run). Docker Desktop must be running.
