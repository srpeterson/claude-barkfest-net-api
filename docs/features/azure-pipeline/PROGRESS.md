# Azure Infrastructure & Release Pipeline — Progress

## Step 1 — Bicep Infrastructure (`infra/main.bicep`) ✅ Complete

- [x] Write `infra/main.bicep` and `infra/resources.bicep` (Container Apps, ACR, SQL, Storage, App Insights, Static Web App)
- [x] Write `Dockerfile` and `.dockerignore`
- [x] Verify Bicep compiles without errors (`az bicep build`)
- [x] Provision resources to Azure (`az deployment sub create`)
- [x] Confirm all resources created in Azure Portal

## Step 2 — GitHub Secrets ✅ Complete

- [x] Create Service Principal (`az ad sp create-for-rbac`) with subscription-level Contributor
- [x] Add all 18 secrets to GitHub repository Settings → Secrets and variables → Actions

## Step 3 — API Pipeline (`.github/workflows/api.yml`) ✅ Complete

- [x] Write workflow file
- [x] Trigger on merge to `main`
- [x] Build, test, Docker build/push to ACR
- [x] Set Container App environment variables from secrets
- [x] Deploy to Container App

## Step 4 — Frontend Pipeline (`.github/workflows/ui.yml`) ✅ Complete

- [x] Write workflow file
- [x] Trigger on merge to `main`
- [x] Install, test, build React frontend
- [x] Deploy to Azure Static Web Apps

## Step 5 — Verify ✅ Complete

- [x] API returns 401 at `https://ca-barkfest.greenisland-212561c8.centralus.azurecontainerapps.io/v1/owners`
- [x] Scalar UI confirmed accessible (temporarily enabled, now restricted to Development)
- [x] Frontend loads at `https://gray-rock-0394ee50f.7.azurestaticapps.net`
- [x] Container App set to minReplicas=1 — always warm, no cold starts
- [ ] Admin login works end-to-end (pending Phase 11 auth feature)
- [ ] Application Insights receives telemetry (verify after traffic)

---

## Notes

- Service Principal requires subscription-level Contributor (not just resource group) for Container Apps
- Microsoft.App provider registration was stuck — fixed via Azure Portal re-register
- `az containerapp` commands require the containerapp CLI extension (`az extension add --name containerapp`)
- Static Web App auto-generated URL: `https://gray-rock-0394ee50f.7.azurestaticapps.net`
- Container App URL: `https://ca-barkfest.greenisland-212561c8.centralus.azurecontainerapps.io`
