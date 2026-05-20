# Azure Infrastructure & Release Pipeline — Decisions

---

## Decision: GitHub Actions for CI/CD
**Choice:** GitHub Actions for both the API and frontend release pipelines.

**Reason:** The repository is already on GitHub. GitHub Actions requires no additional
tooling, is free for public repositories, and has first-class Azure deployment actions
maintained by Microsoft (`azure/webapps-deploy`, `azure/static-web-apps-deploy`).
Azure DevOps Pipelines would require a separate service and additional setup for no
meaningful benefit at this project scale.

---

## Decision: Azure App Service (Linux, B1) for the .NET API
**Choice:** Azure App Service on Linux with the B1 SKU to host the .NET 10 API.

**Reason:** App Service is the simplest production host for a .NET API — no Docker
knowledge required, built-in deployment slots (S1+), automatic TLS, and straightforward
GitHub Actions integration. B1 (~$13/month) is sufficient for a dev/demo project.
Azure Container Apps was considered but adds container build complexity that is not
warranted until the app needs horizontal scaling or multiple services.

---

## Decision: Azure Static Web Apps (Free SKU) for the React frontend
**Choice:** Azure Static Web Apps Free SKU to host the Vite/React frontend.

**Reason:** Static Web Apps is purpose-built for SPAs — global CDN, automatic TLS,
free custom domain support, and a GitHub Actions deployment action that handles the
entire build and deploy in one step. The Free SKU (100 GB/month bandwidth) is more
than sufficient for a dev/demo project. Hosting the frontend as static files from
the App Service was considered but rejected — it couples frontend and backend deployments
and loses the CDN benefit.

---

## Decision: Azure SQL Database (Basic SKU) for production data
**Choice:** Azure SQL Server + Database on the Basic SKU (~$5/month).

**Reason:** The application already uses SQL Server locally via Aspire/Testcontainers.
Azure SQL is the natural production equivalent — same engine, same EF Core migrations,
no driver changes. Basic SKU (5 DTUs) is sufficient for low traffic. The `MigrateAsync()`
call on API startup applies any pending migrations automatically on first deploy.

---

## Decision: Azure Blob Storage for production images
**Choice:** Azure Blob Storage with a `barkfest-blobs` container, mirroring the local
Azurite setup.

**Reason:** The application is already built around `IBlobStorageService` backed by
the Azure SDK. The production Blob Storage connection string replaces the Azurite
connection string via an App Service environment variable — no code changes required.

---

## Decision: `eastus` as the Azure region
**Choice:** East US (Virginia) as the deployment region for all resources.

**Reason:** `eastus` is the most widely used Azure region in the US, has the broadest
service availability, lowest prices on several services, and is the default assumed by
most Azure documentation and tutorials. Geographically reasonable for a project based
in the US — latency is not a concern for a dev/demo project.

---

## Decision: Bicep for infrastructure as code
**Choice:** A single `infra/main.bicep` file defines all Azure resources.

**Reason:** Infrastructure as code means the entire production environment is
reproducible from a single command. If resources are accidentally deleted or a new
environment is needed, `az deployment sub create` recreates everything identically.
Bicep is the modern Azure-native IaC language — simpler than ARM templates, no
third-party tooling required (unlike Terraform). A single file is sufficient at this
scale; separate modules would add unnecessary complexity.

---

## Decision: All secrets stored in GitHub Secrets — never in source control
**Choice:** All production credentials (SQL password, JWT secret, admin seed credentials,
Azure service principal) are stored as GitHub repository secrets and injected into the
pipeline at runtime.

**Reason:** Non-negotiable security practice. Secrets in source control are permanently
compromised — even after deletion, they exist in git history. GitHub Secrets are
encrypted at rest, masked in logs, and only accessible to workflow runs on the correct
branch. App Service environment variables receive the secrets at deploy time so the
running application never reads them from a config file.
