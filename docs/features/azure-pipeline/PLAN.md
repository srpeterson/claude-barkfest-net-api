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
| Azure Container Registry | `Microsoft.ContainerRegistry/registries` | Stores Docker images built by GitHub Actions |
| Container Apps Environment | `Microsoft.App/managedEnvironments` | Shared hosting environment for Container Apps |
| Container App | `Microsoft.App/containerApps` | Hosts the .NET 10 API (serverless containers) |
| SQL Server | `Microsoft.Sql/servers` | Production SQL Server instance |
| SQL Database | `Microsoft.Sql/servers/databases` | Production `barkfest` database (Basic SKU) |
| Storage Account | `Microsoft.Storage/storageAccounts` | Production Blob Storage (equivalent to local Azurite) |
| Blob Container | `Microsoft.Storage/storageAccounts/blobServices/containers` | `barkfest-blobs` container |
| Application Insights | `Microsoft.Insights/components` | Telemetry — already wired in the API code |
| Log Analytics Workspace | `Microsoft.OperationalInsights/workspaces` | Backend store for Application Insights + container logs |
| Static Web App | `Microsoft.Web/staticSites` | Hosts the React frontend (Free SKU) |

---

## Folder Structure

```
infra/
└── main.bicep          ← all Azure resources defined here
Dockerfile              ← multi-stage Docker build for Barkfest.API
.github/
└── workflows/
    ├── api.yml         ← .NET API build, test, Docker push, and deploy pipeline
    └── ui.yml          ← React frontend build, test, and deploy pipeline
docs/features/azure-pipeline/
    ├── PLAN.md         ← this file
    ├── PROGRESS.md     ← progress tracking
    └── DECISIONS.md    ← decisions made during this work
```

---

## Step 1 — Bicep Infrastructure (`infra/main.bicep`)

Define all resources as Bicep. Parameters:
- `location` — Azure region (default: `centralus`)
- `sqlAdminLogin` — SQL Server admin username (passed as secure param, never hardcoded)
- `sqlAdminPassword` — SQL Server admin password (secure param)

Outputs (used by GitHub Actions and app config):
- `containerAppName` — Container App name (for deployment target)
- `containerAppFqdn` — Container App fully qualified domain name (for API_URL secret)
- `containerRegistryLoginServer` — ACR login server (for Docker push)
- `staticWebAppName` — Static Web App name (for deployment target)
- `sqlServerFqdn` — SQL Server FQDN (for constructing connection string)
- `storageAccountName` — Storage account name (for retrieving blob connection string)
- `appInsightsConnectionString` — Application Insights connection string

### Provisioning command (run once)

```bash
az login --tenant afb7696f-0b7b-435a-a6ec-1b39afd266f5
az deployment sub create \
  --name barkfest-resources \
  --location centralus \
  --template-file infra/main.bicep \
  --parameters sqlAdminLogin=<username> sqlAdminPassword=<password>
```

### Reading outputs after provisioning

```bash
# Get all outputs
az deployment sub show \
  --name barkfest-resources \
  --query properties.outputs

# Get the Container App FQDN (use as API_URL secret — prefix with https://)
az containerapp show \
  --name ca-barkfest \
  --resource-group rg-barkfest \
  --query properties.configuration.ingress.fqdn \
  --output tsv

# Get ACR credentials (use as REGISTRY_USERNAME and REGISTRY_PASSWORD secrets)
az acr credential show \
  --name <containerRegistryLoginServer prefix> \
  --resource-group rg-barkfest

# Get the Blob Storage connection string
az storage account show-connection-string \
  --name <storageAccountName from output> \
  --resource-group rg-barkfest \
  --query connectionString \
  --output tsv

# Get the Static Web App deployment token (needed for ui.yml GitHub Actions)
az staticwebapp secrets list \
  --name stapp-barkfest \
  --resource-group rg-barkfest \
  --query properties.apiKey \
  --output tsv
```

---

## Step 2 — GitHub Secrets

After provisioning, add the following secrets in GitHub:
**Settings → Secrets and variables → Actions → New repository secret**

| Secret Name | Value | Where used |
|---|---|---|
| `AZURE_CREDENTIALS` | Service principal JSON (see below) | Both workflows — Azure login |
| `CONTAINER_APP_NAME` | `ca-barkfest` | `api.yml` — deployment target |
| `REGISTRY_LOGIN_SERVER` | ACR login server from Bicep output | `api.yml` — Docker push |
| `REGISTRY_USERNAME` | ACR admin username from `az acr credential show` | `api.yml` — Docker login |
| `REGISTRY_PASSWORD` | ACR admin password from `az acr credential show` | `api.yml` — Docker login |
| `API_URL` | `https://<containerAppFqdn>` from Bicep output | `ui.yml` — VITE_API_BASE_URL at build time |
| `STATIC_WEB_APP_NAME` | Static Web App name from Bicep output | Reference only |
| `AZURE_STATIC_WEB_APPS_API_TOKEN` | Static Web App deployment token (see CLI command above) | `ui.yml` — Static Web Apps deploy action |
| `SQL_CONNECTION_STRING` | SQL connection string (constructed from outputs + password) | `api.yml` — App config |
| `BLOB_CONNECTION_STRING` | Blob Storage connection string from CLI | `api.yml` — App config |
| `APPINSIGHTS_CONNECTION_STRING` | Application Insights connection string from Bicep output | `api.yml` — App config |
| `JWT_SECRET_KEY` | Production JWT secret (min 32 chars, random) | `api.yml` — App config |
| `ADMIN_USERNAME` | Production admin seed username | `api.yml` — App config |
| `ADMIN_NAME` | Production admin seed name | `api.yml` — App config |
| `ADMIN_EMAIL` | Production admin seed email | `api.yml` — App config |
| `ADMIN_PHONE_NUMBER` | Production admin seed phone (E.164) | `api.yml` — App config |
| `ADMIN_PASSWORD` | Production admin seed password | `api.yml` — App config |
| `CORS_ALLOWED_ORIGIN` | Production frontend URL (from Static Web App) | `api.yml` — App config |

### SQL Connection String format

```
Server=tcp:{sqlServerFqdn},1433;Initial Catalog=barkfest;Persist Security Info=False;
User ID={sqlAdminLogin};Password={sqlAdminPassword};MultipleActiveResultSets=False;
Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

### Creating the Service Principal

```bash
az ad sp create-for-rbac \
  --name "barkfest-github-actions" \
  --role contributor \
  --scopes /subscriptions/2f2a7136-ccb9-4e9b-a3a1-c189b812faa4/resourceGroups/rg-barkfest \
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
4. `dotnet build --no-restore --configuration Release`
5. `dotnet test --no-build` — all test projects must pass
6. `az login` using `AZURE_CREDENTIALS`
7. Docker login to ACR using `REGISTRY_LOGIN_SERVER` / `REGISTRY_USERNAME` / `REGISTRY_PASSWORD`
8. Build and push Docker image tagged with `github.sha` and `latest`
9. `az containerapp update` — deploy new image + set all environment variables

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
   - `VITE_API_BASE_URL` set from `API_URL` secret
7. Deploy `dist/` to Azure Static Web Apps via `azure/static-web-apps-deploy` action

---

## Step 5 — Verify

After first deployment:
- [ ] API responds at `https://<containerAppFqdn>/health`
- [ ] Scalar UI accessible at `https://<containerAppFqdn>/scalar/v1`
- [ ] Frontend loads at the Static Web App URL
- [ ] Admin login works end-to-end
- [ ] Application Insights receives telemetry

---

## Notes

- Container Apps scale to zero replicas when idle — no cost when not in use
- Container Apps scale out automatically under load (max 3 replicas)
- Azure Container Registry Basic SKU costs ~$5/month
- SQL Basic SKU (~$5/month) is sufficient for low traffic
- Static Web App Free SKU has a 100 GB/month bandwidth limit — sufficient for a dev/demo project
- All secrets are stored in GitHub Secrets — never in source control or `appsettings.json`
- The Bicep template is idempotent — running it again updates resources without recreating them
