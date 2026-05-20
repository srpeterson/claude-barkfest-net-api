# Azure Infrastructure & Release Pipeline — Plan

## Goal

Provision all Azure resources for Barkfest and wire up a GitHub Actions release pipeline
so that every merge to `main` automatically deploys to production.

---

## Azure Resources

All resources are defined in `infra/main.bicep` and provisioned with a single command.

| Resource | Type | Purpose |
|---|---|---|
| Resource Group | `Microsoft.Resources/resourceGroups` | Logical container for all Barkfest resources |
| App Service Plan | `Microsoft.Web/serverfarms` | Compute for the .NET API (Linux, B1 SKU) |
| App Service | `Microsoft.Web/sites` | Hosts the .NET 10 API |
| SQL Server | `Microsoft.Sql/servers` | Production SQL Server instance |
| SQL Database | `Microsoft.Sql/servers/databases` | Production `barkfest` database (Basic SKU) |
| Storage Account | `Microsoft.Storage/storageAccounts` | Production Blob Storage (equivalent to local Azurite) |
| Blob Container | `Microsoft.Storage/storageAccounts/blobServices/containers` | `barkfest-blobs` container |
| Application Insights | `Microsoft.Insights/components` | Telemetry — already wired in the API code |
| Log Analytics Workspace | `Microsoft.OperationalInsights/workspaces` | Backend store for Application Insights |
| Static Web App | `Microsoft.Web/staticSites` | Hosts the React frontend (Free SKU) |

---

## Folder Structure

```
infra/
└── main.bicep          ← all Azure resources defined here
.github/
└── workflows/
    ├── api.yml         ← .NET API build, test, and deploy pipeline
    └── ui.yml          ← React frontend build, test, and deploy pipeline
docs/features/azure-pipeline/
    ├── PLAN.md         ← this file
    ├── PROGRESS.md     ← progress tracking
    └── DECISIONS.md    ← decisions made during this work
```

---

## Step 1 — Bicep Infrastructure (`infra/main.bicep`)

Define all resources as Bicep. Parameters:
- `location` — Azure region (default: `eastus`)
- `sqlAdminLogin` — SQL Server admin username (passed as secure param, never hardcoded)
- `sqlAdminPassword` — SQL Server admin password (secure param)

Outputs (used by GitHub Actions and App Service config):
- `apiAppName` — App Service name (for deployment target)
- `staticWebAppName` — Static Web App name (for deployment target)
- `sqlConnectionString` — connection string for the production database
- `blobConnectionString` — connection string for production Blob Storage
- `appInsightsConnectionString` — Application Insights connection string

### Provisioning command (run once)

```bash
az login
az deployment sub create \
  --location eastus \
  --template-file infra/main.bicep \
  --parameters sqlAdminLogin=<username> sqlAdminPassword=<password>
```

---

## Step 2 — GitHub Secrets

After provisioning, add the following secrets in GitHub:
**Settings → Secrets and variables → Actions → New repository secret**

| Secret Name | Value | Where used |
|---|---|---|
| `AZURE_CREDENTIALS` | Service principal JSON (see below) | Both workflows — Azure login |
| `API_APP_NAME` | App Service name output from Bicep | `api.yml` — deployment target |
| `STATIC_WEB_APP_NAME` | Static Web App name from Bicep | `ui.yml` — deployment target |
| `SQL_CONNECTION_STRING` | SQL connection string from Bicep output | App Service config |
| `BLOB_CONNECTION_STRING` | Blob Storage connection string from Bicep output | App Service config |
| `APPINSIGHTS_CONNECTION_STRING` | Application Insights connection string | App Service config |
| `JWT_SECRET_KEY` | Production JWT secret (min 32 chars, random) | App Service config |
| `ADMIN_USERNAME` | Production admin seed username | App Service config |
| `ADMIN_NAME` | Production admin seed name | App Service config |
| `ADMIN_EMAIL` | Production admin seed email | App Service config |
| `ADMIN_PHONE_NUMBER` | Production admin seed phone (E.164) | App Service config |
| `ADMIN_PASSWORD` | Production admin seed password | App Service config |
| `CORS_ALLOWED_ORIGIN` | Production frontend URL (from Static Web App) | App Service config |

### Creating the Service Principal

```bash
az ad sp create-for-rbac \
  --name "barkfest-github-actions" \
  --role contributor \
  --scopes /subscriptions/<subscription-id>/resourceGroups/rg-barkfest \
  --json-auth
```

Copy the JSON output as the value for `AZURE_CREDENTIALS`.

---

## Step 3 — API Pipeline (`.github/workflows/api.yml`)

Triggers: push to `main`

Steps:
1. Checkout code
2. Setup .NET 10
3. `dotnet restore`
4. `dotnet build --no-restore`
5. `dotnet test --no-build` — all 6 projects must pass
6. `dotnet publish` → zip artifact
7. `az login` using `AZURE_CREDENTIALS`
8. Set App Service environment variables from GitHub Secrets:
   - `ConnectionStrings__barkfest-sql`
   - `ConnectionStrings__barkfest-blobs`
   - `APPLICATIONINSIGHTS_CONNECTION_STRING`
   - `Jwt__SecretKey`
   - `Jwt__Issuer` = `barkfest-api`
   - `Jwt__Audience` = `barkfest-client`
   - `Jwt__ExpiryMinutes` = `60`
   - `Admin__Username`, `Admin__Name`, `Admin__Email`, `Admin__PhoneNumber`, `Admin__Password`
   - `Cors__AllowedOrigin`
9. Deploy zip to App Service via `az webapp deploy`

---

## Step 4 — Frontend Pipeline (`.github/workflows/ui.yml`)

Triggers: push to `main`

Steps:
1. Checkout code
2. Setup Node.js (LTS)
3. Setup pnpm
4. `pnpm install --dir barkfest-ui`
5. `pnpm --dir barkfest-ui test` — must pass
6. `pnpm --dir barkfest-ui build` — outputs to `barkfest-ui/dist/`
7. Deploy `dist/` to Azure Static Web Apps via `azure/static-web-apps-deploy` action

Note: `VITE_API_BASE_URL` is set at build time from the App Service URL so the frontend
points at the correct production API endpoint.

---

## Step 5 — Verify

After first deployment:
- [ ] API health check responds at `https://<app-service-name>.azurewebsites.net/health`
- [ ] Scalar UI accessible at `https://<app-service-name>.azurewebsites.net/scalar/v1`
- [ ] Frontend loads at the Static Web App URL
- [ ] Admin login works end-to-end
- [ ] Application Insights receives telemetry

---

## Notes

- The Static Web App Free SKU has a 100 GB/month bandwidth limit — sufficient for a dev/demo project
- App Service B1 SKU costs ~$13/month — upgrade to S1 when you need deployment slots or custom domains with SSL
- SQL Basic SKU (~$5/month) is sufficient for low traffic; upgrade to Standard when DTU limits are hit
- All secrets are stored in GitHub Secrets — never in source control or `appsettings.json`
- The Bicep template is idempotent — running it again updates resources without recreating them
