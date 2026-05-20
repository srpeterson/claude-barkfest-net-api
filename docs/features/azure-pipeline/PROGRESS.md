# Azure Infrastructure & Release Pipeline — Progress

## Step 1 — Bicep Infrastructure (`infra/main.bicep`) 🔄 In progress

- [x] Write `infra/main.bicep` and `infra/resources.bicep`
- [x] Verify Bicep compiles without errors (`az bicep build`)
- [ ] Provision resources to Azure (`az deployment sub create`)
- [ ] Confirm all resources created in Azure Portal

## Step 2 — GitHub Secrets ⬜ Not started

- [ ] Create Service Principal (`az ad sp create-for-rbac`)
- [ ] Add all secrets to GitHub repository Settings → Secrets and variables → Actions

## Step 3 — API Pipeline (`.github/workflows/api.yml`) ✅ Written

- [x] Write workflow file
- [x] Trigger on merge to `main`
- [x] Build, test, publish .NET API
- [x] Set App Service environment variables from secrets
- [x] Deploy to App Service

## Step 4 — Frontend Pipeline (`.github/workflows/ui.yml`) ✅ Written

- [x] Write workflow file
- [x] Trigger on merge to `main`
- [x] Install, test, build React frontend
- [x] Deploy to Azure Static Web Apps

## Step 5 — Verify ⬜ Not started

- [ ] API health check responds
- [ ] Scalar UI accessible
- [ ] Frontend loads
- [ ] Admin login works end-to-end
- [ ] Application Insights receives telemetry

---

## Next

Step 1 — Provision resources to Azure (`az deployment sub create`), then Step 2 — add GitHub Secrets
