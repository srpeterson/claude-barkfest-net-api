# Barkfest

A .NET 10 Clean Architecture pet management API.
Owners register themselves and their pets. Relational data lives in SQL Server via EF Core.
Pet and owner profile images are stored in Azure Blob Storage.
The frontend is a React + TypeScript app built with Vite, located in the `barkfest-ui` directory.

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
- [Git](https://git-scm.com/downloads) (v2.23 or later) - for cloning the repository and version control
- [Node.js](https://nodejs.org/) (LTS, v20 or later) - required for the frontend (`barkfest-ui`)

  **Check if Node.js and npm are installed correctly:**
  ```bash
  node --version
  npm --version
  ```
  Both commands should print a version number. If either is not recognised, install Node.js from the link above — npm is included automatically.

- **pnpm** - package manager used by `barkfest-ui`. Install once via:
  ```bash
  npm install -g pnpm
  ```
- **EF Core CLI tools** (`dotnet ef`) — used to create and manage database migrations.

  **Step 1 — Check what you already have:**
  ```bash
  dotnet ef --version
  ```
  - If a version number is printed, you have a global install. That is fine — the next step installs a separate local version scoped to this repo and will not touch your global install.
  - If you get "command not found" or "is not recognised", you have nothing installed globally. That is also fine — the next step handles everything.

  **Step 2 — Install the repo's pinned version:**

  The repo includes a `.config/dotnet-tools.json` manifest that pins the exact version required. Run this once from the **repo root**:
  ```bash
  dotnet tool restore
  ```
  This installs `dotnet ef 10.0.8` locally, scoped to this repo only. It will not affect any other projects on your machine.

  **Step 3 — Verify:**
  ```bash
  dotnet ef --version
  ```
  You should see `10.0.8`. If `dotnet ef` is still not recognised, make sure `~/.dotnet/tools` is on your `PATH`.

  > **Will this affect other projects using an older version of `dotnet ef`?**
  > No. `dotnet tool restore` installs the tool locally inside this repo — it is completely isolated from any global install and from other projects. Each project manages its own local tool versions independently.

### (Highly Recommended) Pull Docker images before running the app for the first time

> **Do this before running the AppHost.** Do not rely on Aspire to pull images
> automatically on first run - depending on your network and DNS configuration the pull
> can be very slow, hang indefinitely, or fail silently, leaving you with a broken startup
> and no obvious error message.

Run these two commands from any terminal while Docker Desktop is running (directory does not matter):

```bash
docker pull mcr.microsoft.com/mssql/server:2022-latest
docker pull mcr.microsoft.com/azure-storage/azurite:latest
```

Each pull may take a few minutes on first download. When complete, each command prints
either `Status: Downloaded newer image` (new download) or `Status: Image is up to date`
(already cached) - either means the image is ready.

To verify both images are available locally (again, any terminal):

```bash
docker images
```

You should see `mssql/server` and `azurite` in the list. Once they are present, Aspire
will start the containers immediately on every subsequent run without any network download.

> **Note:** If the SQL Server image pull hangs or fails with a network error, try disabling
> IPv6 on your Windows network adapter: Control Panel → Network and Internet → Network
> Connections → right-click your active adapter → Properties → uncheck
> **Internet Protocol Version 6 (TCP/IPv6)** → OK.
>
> Once the image has downloaded successfully, go back and re-enable IPv6 using the same steps — leaving it disabled long-term can affect other software on your machine.

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

## First Steps (Local Development)

> Aspire is configured to start all the services including the UI. Normally all you need to do is run Aspire and follow the link shown next to `barkfest-ui`. From there you can do all the functions that call back to the API such as registering a new owner.
>
> The steps below are for developers who want to test the API directly using Scalar, Bruno, or Postman. You still need Aspire running to get the `{url}:{port}` that the API is running on.

### Register as an owner

```
POST https://localhost:{port}/v1/auth/register
Content-Type: application/json

{
  "username": "yourUsername",
  "firstName": "Your",
  "lastName": "Name",
  "email": "your@email.com",
  "password": "YourPassword1!",
  "phoneNumber": "+15555550101",
  "displayName": "Your Display Name"
}
```

`phoneNumber` and `displayName` are optional. A successful registration returns `201 Created`.

### Log in

```
POST https://localhost:{port}/v1/auth/login
Content-Type: application/json

{
  "username": "yourUsername",
  "password": "YourPassword1!"
}
```

The response contains an `accessToken` (JWT), your `accountId`, and `expiresAt`. Pass the token as a Bearer token on all subsequent requests:

```
Authorization: Bearer <accessToken>
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

**Before running tests, make sure Docker Desktop is running.** The test suite uses Testcontainers — a library that automatically spins up real SQL Server and Azurite containers for tests that need them, then tears them down when the run is complete. You do not need to set anything up manually; Docker just needs to be available. This applies whether you are running tests from Visual Studio or the command line.

The first run may be slower if the container images have not been pulled yet (see the Docker images section above). Subsequent runs will be faster.

The primary way to run tests is via **Visual Studio's Test Explorer** — open it from View → Test Explorer, then run all tests, a specific project, or an individual test with a single click.

**From the command line** (used by CI, or if you prefer the terminal):
```bash
dotnet test
```
Run from the **repo root**. A passing run prints:
```
Passed! - Failed: 0, Passed: X, Skipped: 0, Total: X
```

### Frontend tests

All frontend test commands are run from the **repo root** — the `--dir barkfest-ui` flag targets the correct directory automatically.

**Single-pass (use this before committing):**
```bash
pnpm --dir barkfest-ui test
```
Runs all tests once and exits. Safe to run in CI.

**Watch mode (use this during development):**
```bash
pnpm --dir barkfest-ui test:watch
```
Re-runs tests automatically whenever you save a file. Useful when actively writing or changing tests.

**Visual UI (opens in browser):**
```bash
pnpm --dir barkfest-ui test:ui
```
Opens an interactive test runner in your browser — lets you see which tests passed or failed, filter by file, and re-run individual tests.

Frontend tests do **not** require Docker or a running API — they are pure unit and component tests.

---

## Viewing UI on a Mobile Device

Both your development machine and phone must be on the same Wi-Fi network.

The `barkfest-ui` dev server intercepts all `/v1/...` API calls and forwards them from your machine to `https://localhost:7101`. The phone only ever talks to the dev server — no certificate issues, no CORS configuration needed. No `.env` file required.

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

**4. Start the `barkfest-ui` dev server with `--host`**

```bash
cd barkfest-ui
pnpm dev --host
```

The dev server will print one or more Network URLs. Use the one that matches the IP you found in ipconfig, for example:

```
  ➜  Network: http://192.168.1.45:5173/
```

**5. Open that URL on your phone**

Enter the Network URL in your mobile browser.

**When you are done**, stop the Vite terminal (Ctrl+C) and click restart on `barkfest-ui` in the Aspire dashboard.

> `pnpm dev --host` works for both desktop (`http://localhost:5173`) and mobile (the Network URL) - no need to run a different command for each.
