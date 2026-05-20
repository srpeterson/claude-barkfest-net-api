targetScope = 'subscription'

@description('Azure region for all resources.')
param location string = 'eastus'

@description('SQL Server administrator login username.')
param sqlAdminLogin string

@description('SQL Server administrator login password. Must meet Azure SQL complexity requirements.')
@secure()
param sqlAdminPassword string

// Stable suffix derived from the subscription ID — ensures globally unique resource names
// without requiring manual input. Same subscription always gets the same suffix.
var nameSuffix = uniqueString(subscription().subscriptionId)
var resourceGroupName = 'rg-barkfest'

resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: resourceGroupName
  location: location
}

module resources 'resources.bicep' = {
  name: 'barkfest-resources'
  scope: resourceGroup
  params: {
    location: location
    sqlAdminLogin: sqlAdminLogin
    sqlAdminPassword: sqlAdminPassword
    nameSuffix: nameSuffix
  }
}

// --- Outputs ---
// Used after provisioning to configure GitHub Secrets.
// See docs/features/azure-pipeline/PLAN.md — Step 2 for full instructions.

@description('App Service name — used as the GitHub Secret API_APP_NAME.')
output apiAppName string = resources.outputs.apiAppName

@description('Static Web App name — used as the GitHub Secret STATIC_WEB_APP_NAME.')
output staticWebAppName string = resources.outputs.staticWebAppName

@description('Application Insights connection string — used as APPINSIGHTS_CONNECTION_STRING.')
output appInsightsConnectionString string = resources.outputs.appInsightsConnectionString

@description('SQL Server fully qualified domain name — used to construct the SQL connection string.')
output sqlServerFqdn string = resources.outputs.sqlServerFqdn

@description('Storage account name — used to retrieve the blob connection string via CLI.')
output storageAccountName string = resources.outputs.storageAccountName
