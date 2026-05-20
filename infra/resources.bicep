@description('Azure region for all resources.')
param location string

@description('SQL Server administrator login username.')
param sqlAdminLogin string

@description('SQL Server administrator login password.')
@secure()
param sqlAdminPassword string

@description('Unique suffix for globally unique resource names.')
param nameSuffix string

// --- Resource names ---
var containerRegistryName    = 'crbarkfest${nameSuffix}'  // max 50 chars: 11 + 13 = 24
var containerAppName         = 'ca-barkfest'
var containerAppsEnvName     = 'cae-barkfest'
var sqlServerName            = 'sql-barkfest-${nameSuffix}'
var sqlDatabaseName          = 'barkfest'
var storageAccountName       = 'stbarkfest${nameSuffix}'  // max 24 chars: 10 + 13 = 23
var appInsightsName          = 'appi-barkfest'
var logAnalyticsName         = 'log-barkfest'
var staticWebAppName         = 'stapp-barkfest'

// --- Log Analytics Workspace ---
// Required backend store for workspace-based Application Insights.
// Classic (non-workspace) Application Insights is being retired by Microsoft.
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: logAnalyticsName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'  // pay-per-GB — near zero cost at low volume
    }
    retentionInDays: 30
  }
}

// --- Application Insights ---
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}

// --- Azure Container Registry (Basic ~$5/month) ---
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' = {
  name: containerRegistryName
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: true  // required for GitHub Actions to push images
  }
}

// --- Container Apps Environment ---
// Shared hosting environment for Container Apps. Wired to Log Analytics so
// container logs flow into the same workspace as Application Insights.
resource containerAppsEnv 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: containerAppsEnvName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

// --- Container App (.NET 10 API) ---
// Starts with a placeholder hello-world image. The GitHub Actions api.yml
// workflow replaces this image on every push to main.
resource containerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: containerAppName
  location: location
  properties: {
    managedEnvironmentId: containerAppsEnv.id
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
        transport: 'auto'
      }
      registries: [
        {
          server: containerRegistry.properties.loginServer
          username: containerRegistry.name
          passwordSecretRef: 'registry-password'
        }
      ]
      secrets: [
        {
          name: 'registry-password'
          value: containerRegistry.listCredentials().passwords[0].value
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'barkfest-api'
          // Placeholder image — replaced by api.yml on first deployment
          image: 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
        }
      ]
      scale: {
        minReplicas: 0  // scales to zero when idle — no cost when not in use
        maxReplicas: 3
      }
    }
  }
}

// --- SQL Server ---
resource sqlServer 'Microsoft.Sql/servers@2022-08-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    version: '12.0'
    minimalTlsVersion: '1.2'
  }
}

// Allow Azure services (including Container Apps) to connect to SQL Server.
// The 0.0.0.0 / 0.0.0.0 rule is an Azure-specific sentinel that enables
// the "Allow Azure services" toggle — it does not open the server to the public internet.
resource sqlFirewallAzureServices 'Microsoft.Sql/servers/firewallRules@2022-08-01-preview' = {
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// --- SQL Database (Basic SKU ~$5/month) ---
resource sqlDatabase 'Microsoft.Sql/servers/databases@2022-08-01-preview' = {
  parent: sqlServer
  name: sqlDatabaseName
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
    capacity: 5
  }
}

// --- Storage Account (Standard LRS) ---
resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    allowBlobPublicAccess: false    // images are served via the API, never directly
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

// Blob service resource required as parent for the container
resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2022-09-01' = {
  name: 'default'
  parent: storageAccount
}

// Blob container — mirrors the 'barkfest-blobs' container used locally via Azurite
resource blobContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2022-09-01' = {
  name: 'barkfest-blobs'
  parent: blobService
  properties: {
    publicAccess: 'None'
  }
}

// --- Static Web App (Free SKU) ---
// Static Web Apps have limited region availability. eastus2 is used regardless of
// the primary location parameter as it has full support and pairs well with centralus.
resource staticWebApp 'Microsoft.Web/staticSites@2022-09-01' = {
  name: staticWebAppName
  location: 'eastus2'
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {}
}

// --- Outputs ---
output containerAppName string = containerApp.name
output containerAppFqdn string = containerApp.properties.configuration.ingress.fqdn
output containerRegistryLoginServer string = containerRegistry.properties.loginServer
output staticWebAppName string = staticWebApp.name
output appInsightsConnectionString string = appInsights.properties.ConnectionString
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output storageAccountName string = storageAccount.name
