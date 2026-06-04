# Barkfest

A .NET 10 Clean Architecture pet management API.
Owners register themselves and their pets. Relational data lives in SQL Server via EF Core.
Pet and owner profile images are stored in Azure Blob Storage.

---

## Documentation

| Document | Purpose |
|---|---|
| [SPEC.md](SPEC.md) | Functional specification - what the application does |
| [ROADMAP.md](ROADMAP.md) | Feature backlog - what's planned next |
| [PLAN.md](PLAN.md) | Initial build plan - phases 1–12 (historical record) |
| [DECISIONS.md](DECISIONS.md) | Initial build decisions - the why behind every key choice (historical record) |
| [PROGRESS.md](PROGRESS.md) | Initial build progress - phase-by-phase summary (historical record) |
| `docs/features/<name>/` | Per-feature plan, progress, and decisions - one folder per feature |

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (Included with Visual Studio 2026)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) - must be running before starting the app
- [Git](https://git-scm.com/downloads)
- [Node.js](https://nodejs.org/) (LTS) - required for the frontend (`barkfest-ui`)
- **pnpm** - package manager used by `barkfest-ui`. Install once via:
  ```bash
  npm install -g pnpm
  ```
- **EF Core CLI tools** (`dotnet ef`) - minimum version **10.0.8**

  The repo includes a `dotnet-tools.json` manifest pinned to `10.0.8`.
  Install the exact version with:
  ```bash
  dotnet tool restore
  ```
  To verify the install:
  ```bash
  dotnet ef --version
  ```
  You should see `10.0.8` (or later). If `dotnet ef` is not recognised, make sure
  `~/.dotnet/tools` is on your `PATH`.

### (Highly Recommended) Pull Docker images before running the app for the first time

> **Do this before running the AppHost.** Do not rely on Aspire to pull images
> automatically on first run - depending on your network and DNS configuration the pull
> can be very slow, hang indefinitely, or fail silently, leaving you with a broken startup
> and no obvious error message.

Run these two commands while Docker Desktop is running:

```bash
docker pull mcr.microsoft.com/mssql/server:2022-latest
docker pull mcr.microsoft.com/azure-storage/azurite:latest
```

Each pull may take a few minutes on first download. When complete, each command prints
either `Status: Downloaded newer image` (new download) or `Status: Image is up to date`
(already cached) - either means the image is ready.

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
all connection strings - no manual configuration is required.

1. Make sure Docker Desktop is running
2. Start the AppHost:
   ```bash
   dotnet run --project src/Barkfest.AppHost
   ```
3. The Aspire dashboard opens in your browser - it shows the status of all services
4. Once the API is healthy, open the Scalar API reference at:
   ```
   https://localhost:{port}/scalar/v1
   ```
   (the port is shown in the Aspire dashboard next to the `barkfest-api` resource)

The database migration is applied automatically on startup. Container data persists across
restarts - SQL Server and Azurite volumes survive `docker stop` and machine reboots.

### Frontend environment variables

No `.env` file is needed for local development. When running via Aspire, `VITE_API_BASE_URL`
is injected automatically. When running Vite manually (`pnpm dev --host`), the built-in proxy
forwards API calls to `https://localhost:7101` without any configuration.

---

## First Login (Local Development Only)

> **This section applies to local development only.** The credentials below are committed
> to source control for developer convenience and are not suitable for any shared or
> production environment.

On startup, `SeedAdminAsync` checks whether an administrator account already exists and, if not,
creates one using the credentials in `appsettings.Development.json`. No manual database setup is
required.

**Default dev credentials:**

| Field | Value |
|---|---|
| Username | `admin` |
| Email | `admin@barkfest.dev` |
| Password | `Admin1234!` |

Once the API is running, authenticate via Scalar or any HTTP client:

```
POST https://localhost:{port}/v1/auth/admin/login
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin1234!"
}
```

The response contains an `accessToken` (JWT). Pass it as a Bearer token on all subsequent requests:

```
Authorization: Bearer <accessToken>
```

It is good practice to create a second administrator account immediately after first login
so that access is never dependent on a single set of credentials. Use Scalar or any HTTP
client with the bearer token from the login step above:

```
POST https://localhost:{port}/v1/admin/admins
Authorization: Bearer <accessToken>
Content-Type: application/json

{
  "username": "yourUsername",
  "name": "Your Name",
  "email": "your@email.com",
  "phoneNumber": "+15555550101",
  "password": "YourPassword1!"
}
```

---

## Observability (Application Insights)

When deployed to Azure, the API sends logs, distributed traces, HTTP dependency calls,
and runtime metrics to **Azure Application Insights** automatically.

No code changes are required - the exporter is already wired into `Barkfest.ServiceDefaults`
and activates when `APPLICATIONINSIGHTS_CONNECTION_STRING` is present in the environment.
In local development the key is absent, so the Aspire dashboard is used instead.

> **Never** commit the connection string to source control or add it to `appsettings.json`.

---

## Deployment

Merging into `main` triggers two GitHub Actions pipelines automatically. The API pipeline
builds, tests, packages a Docker image, pushes it to Azure Container Registry, and deploys
to Azure Container Apps. The frontend pipeline builds the React app and deploys to Azure
Static Web Apps. No manual steps required.

---

## Running Tests

### .NET tests

```bash
dotnet test
```

All tests manage their own infrastructure via Testcontainers (SQL Server and Azurite containers
are started and torn down automatically per test run). Docker Desktop must be running.

### Frontend tests

```bash
pnpm --dir barkfest-ui test
```

Runs Vitest in single-pass mode (no watch). Safe to run in CI and as part of a pre-commit check.

Interactive watch mode during development:

```bash
pnpm --dir barkfest-ui test:watch
```

Visual test UI (opens in browser):

```bash
pnpm --dir barkfest-ui test:ui
```

Frontend tests do **not** require Docker or a running API - they are pure unit and component tests.

---

## Testing on a Mobile Device

Both your development machine and phone must be on the same Wi-Fi network.

Vite proxies all `/v1/...` API calls server-side to `https://localhost:7101`, so the phone never talks to the .NET API directly - no certificate issues, no CORS configuration needed. No `.env` file required.

**1. Start Aspire as normal**

```bash
dotnet run --project src/Barkfest.AppHost
```

Wait until all services are healthy.

**2. Stop the `barkfest-ui` resource in the Aspire dashboard**

Find `barkfest-ui` in the dashboard and stop it. This frees up port 5173 so you can
start Vite manually in the next step. The database, blob storage, and API all keep running.

**3. Find your machine's local IP address**

```bash
ipconfig
```

Look for the **IPv4 Address** under your physical Ethernet or Wi-Fi adapter - something like `192.168.1.45`. Ignore the `vEthernet (WSL)` adapter.

**4. Start Vite manually with `--host`**

```bash
cd barkfest-ui
pnpm dev --host
```

Vite will print two Network URLs - use the one matching your physical adapter IP:

```
  ➜  Network: http://192.168.1.45:5173/
```

**5. Open that URL on your phone**

Enter the Network URL in your mobile browser.

**When you are done**, stop the Vite terminal (Ctrl+C) and click restart on `barkfest-ui` in the Aspire dashboard.

> `pnpm dev --host` works for both desktop (`http://localhost:5173`) and mobile (the Network URL) - no need to run a different command for each.
