# Azure Infrastructure & Release Pipeline — Progress

## Step 1 — Bicep Infrastructure (`infra/main.bicep`) ⬜ Not started

- [ ] Write `infra/main.bicep` defining all Azure resources
- [ ] Verify Bicep compiles without errors (`az bicep build`)
- [ ] Provision resources to Azure (`az deployment sub create`)
- [ ] Confirm all resources created in Azure Portal

## Step 2 — GitHub Secrets ⬜ Not started

- [ ] Create Service Principal (`az ad sp create-for-rbac`)
- [ ] Add all secrets to GitHub repository Settings → Secrets and variables → Actions

## Step 3 — API Pipeline (`.github/workflows/api.yml`) ⬜ Not started

- [ ] Write workflow file
- [ ] Trigger on merge to `main`
- [ ] Build, test, publish .NET API
- [ ] Set App Service environment variables from secrets
- [ ] Deploy to App Service

## Step 4 — Frontend Pipeline (`.github/workflows/ui.yml`) ⬜ Not started

- [ ] Write workflow file
- [ ] Trigger on merge to `main`
- [ ] Install, test, build React frontend
- [ ] Deploy to Azure Static Web Apps

## Step 5 — Verify ⬜ Not started

- [ ] API health check responds
- [ ] Scalar UI accessible
- [ ] Frontend loads
- [ ] Admin login works end-to-end
- [ ] Application Insights receives telemetry

---

## Next

Step 1 — Write `infra/main.bicep`
