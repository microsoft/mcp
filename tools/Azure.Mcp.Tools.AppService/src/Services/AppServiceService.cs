// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Authentication;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.AppService.Commands.Webapp.Settings;
using Azure.Mcp.Tools.AppService.Models;
using Azure.ResourceManager.AppService;
using Azure.ResourceManager.AppService.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AppService.Services;

public class AppServiceService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ILogger<AppServiceService> logger) : BaseAzureService(tenantService), IAppServiceService
{
    private readonly ITenantService _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ILogger<AppServiceService> _logger = logger;

    private static readonly string[] supportedTypes = ["sqlserver", "mysql", "postgresql", "cosmosdb"];

    public async Task<DatabaseConnectionInfo> AddDatabaseAsync(
        string appName,
        string resourceGroup,
        string databaseType,
        string databaseServer,
        string databaseName,
        string connectionString,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Adding database connection to App Service {AppName} in resource group {ResourceGroup}",
            appName, resourceGroup);

        try
        {
            // Validate inputs
            ValidateRequiredParameters(
                (nameof(appName), appName),
                (nameof(resourceGroup), resourceGroup),
                (nameof(databaseType), databaseType),
                (nameof(databaseServer), databaseServer),
                (nameof(databaseName), databaseName),
                (nameof(subscription), subscription));

            // Get Azure resources
            var webApp = await GetWebAppResourceAsync(subscription, resourceGroup, appName, tenant, retryPolicy, cancellationToken);

            // Prepare connection string
            var finalConnectionString = PrepareConnectionString(connectionString, databaseType, databaseServer, databaseName);
            var connectionStringName = $"{databaseName}Connection";

            // Update web app configuration
            await UpdateWebAppConnectionStringAsync(webApp, connectionStringName, finalConnectionString, databaseType, cancellationToken);

            _logger.LogInformation(
                "Successfully added database connection {ConnectionName} to App Service {AppName}",
                connectionStringName, appName);

            return CreateDatabaseConnectionInfo(
                databaseType, databaseServer, databaseName, finalConnectionString, connectionStringName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to add database connection to App Service {AppName} in resource group {ResourceGroup}",
                appName, resourceGroup);
            throw;
        }
    }

    private async Task<WebSiteResource> GetWebAppResourceAsync(string subscription, string resourceGroup,
        string appName, string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);

        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
        if (resourceGroupResource?.Value == null)
        {
            throw new ArgumentException($"Resource group '{resourceGroup}' not found in subscription '{subscription}'.");
        }

        var webApps = resourceGroupResource.Value.GetWebSites();
        var webAppResource = await webApps.GetAsync(appName, cancellationToken);
        if (webAppResource?.Value == null)
        {
            throw new ArgumentException($"Web app '{appName}' not found in resource group '{resourceGroup}'.");
        }

        return webAppResource.Value;
    }

    private string PrepareConnectionString(string? connectionString, string databaseType,
        string databaseServer, string databaseName)
    {
        return string.IsNullOrWhiteSpace(connectionString)
            ? BuildConnectionString(databaseType, databaseServer, databaseName)
            : connectionString;
    }

    private static async Task UpdateWebAppConnectionStringAsync(WebSiteResource webApp, string connectionStringName,
        string connectionString, string databaseType, CancellationToken cancellationToken)
    {
        // Get current web app configuration
        var configResource = webApp.GetWebSiteConfig();
        var config = await configResource.GetAsync(cancellationToken);

        if (config?.Value?.Data == null)
        {
            throw new InvalidOperationException($"Unable to retrieve configuration for web app '{webApp.Data.Name}'.");
        }

        // Prepare connection strings collection
        var connectionStrings = config.Value.Data.ConnectionStrings?.ToList() ?? [];

        // Remove existing connection string with the same name if it exists
        connectionStrings.RemoveAll(cs =>
            string.Equals(cs.Name, connectionStringName, StringComparison.OrdinalIgnoreCase));

        // Add the new connection string
        connectionStrings.Add(new ConnStringInfo
        {
            Name = connectionStringName,
            ConnectionString = connectionString,
            ConnectionStringType = GetConnectionStringType(databaseType)
        });

        // Update the web app configuration
        var configData = config.Value.Data;
        configData.ConnectionStrings = connectionStrings;

        var updatedConfig = await configResource.CreateOrUpdateAsync(WaitUntil.Completed, configData, cancellationToken);
        if (updatedConfig?.Value == null)
        {
            throw new InvalidOperationException($"Failed to update configuration for web app '{webApp.Data.Name}'.");
        }
    }

    private static DatabaseConnectionInfo CreateDatabaseConnectionInfo(string databaseType, string databaseServer,
        string databaseName, string connectionString, string connectionStringName)
    {
        return new DatabaseConnectionInfo
        {
            DatabaseType = databaseType,
            DatabaseServer = databaseServer,
            DatabaseName = databaseName,
            ConnectionString = connectionString,
            ConnectionStringName = connectionStringName,
            IsConfigured = true,
            ConfiguredAt = DateTime.UtcNow
        };
    }

    private static ConnectionStringType GetConnectionStringType(string databaseType)
    {
        return databaseType.ToLowerInvariant() switch
        {
            "sqlserver" => ConnectionStringType.SqlServer,
            "mysql" => ConnectionStringType.MySql,
            "postgresql" => ConnectionStringType.PostgreSql,
            "cosmosdb" => ConnectionStringType.Custom,
            _ => throw new ArgumentException($"Unsupported database type: {databaseType}. Supported types: {string.Join(", ", supportedTypes)}")
        };
    }

    private string BuildConnectionString(string databaseType, string databaseServer, string databaseName)
    {
        return databaseType.ToLowerInvariant() switch
        {
            "sqlserver" => $"Server={databaseServer};Database={databaseName};User Id={{username}};Password={{password}};TrustServerCertificate=True;",
            "mysql" => $"Server={databaseServer};Database={databaseName};Uid={{username}};Pwd={{password}};",
            "postgresql" => $"Host={databaseServer};Database={databaseName};Username={{username}};Password={{password}};",
            "cosmosdb" => BuildCosmosConnectionString(databaseServer, databaseName),
            _ => throw new ArgumentException($"Unsupported database type: {databaseType}")
        };
    }

    private string BuildCosmosConnectionString(string databaseServer, string databaseName)
    {
        return _tenantService.CloudConfiguration.CloudType switch
        {
            AzureCloudConfiguration.AzureCloud.AzurePublicCloud =>
                $"AccountEndpoint=https://{databaseServer}.documents.azure.com:443/;AccountKey={{key}};Database={databaseName};",
            AzureCloudConfiguration.AzureCloud.AzureChinaCloud =>
                $"AccountEndpoint=https://{databaseServer}.documents.azure.cn:443/;AccountKey={{key}};Database={databaseName};",
            AzureCloudConfiguration.AzureCloud.AzureUSGovernmentCloud =>
                $"AccountEndpoint=https://{databaseServer}.documents.azure.us:443/;AccountKey={{key}};Database={databaseName};",
            _ => $"AccountEndpoint=https://{databaseServer}.documents.azure.com:443/;AccountKey={{key}};Database={databaseName};"
        };
    }

    public async Task<List<WebappDetails>> GetWebAppsAsync(
        string subscription,
        string? resourceGroup = null,
        string? appName = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);

        var results = new List<WebappDetails>();

        if (!string.IsNullOrWhiteSpace(appName))
        {
            ValidateRequiredParameters((nameof(resourceGroup), resourceGroup));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            if (resourceGroupResource?.Value == null)
            {
                throw new ArgumentException($"Resource group '{resourceGroup}' not found in subscription '{subscription}'.");
            }

            var webAppCollection = resourceGroupResource.Value.GetWebSites();
            var webApp = await webAppCollection.GetAsync(appName, cancellationToken: cancellationToken);
            if (webApp != null)
            {
                results.Add(MapToWebappDetails(webApp.Value.Data));
            }
        }
        else if (!string.IsNullOrWhiteSpace(resourceGroup))
        {
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            if (resourceGroupResource?.Value == null)
            {
                throw new ArgumentException($"Resource group '{resourceGroup}' not found in subscription '{subscription}'.");
            }

            var webAppCollection = resourceGroupResource.Value.GetWebSites();
            await foreach (var webapp in webAppCollection.GetAllAsync(cancellationToken: cancellationToken))
            {
                results.Add(MapToWebappDetails(webapp.Data));
            }
        }
        else
        {
            await foreach (var webapp in subscriptionResource.GetWebSitesAsync(cancellationToken))
            {
                results.Add(MapToWebappDetails(webapp.Data));
            }
        }

        return results;
    }

    private static WebappDetails MapToWebappDetails(WebSiteData webapp)
        => new(webapp.Name, webapp.ResourceType.ToString(), webapp.Location.Name, webapp.Kind, webapp.IsEnabled,
            webapp.State, webapp.ResourceGroup, webapp.HostNames, webapp.LastModifiedTimeUtc, webapp.Sku);

    public async Task<IDictionary<string, string>> GetAppSettingsAsync(
        string subscription,
        string resourceGroup,
        string appName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(resourceGroup), resourceGroup), (nameof(appName), appName));

        var webAppResource = await GetWebAppResourceAsync(subscription, resourceGroup, appName, tenant, retryPolicy, cancellationToken);
        var configResource = await webAppResource.GetApplicationSettingsAsync(cancellationToken: cancellationToken);

        return configResource.Value.Properties;
    }

    public async Task<string> UpdateAppSettingsAsync(
        string subscription,
        string resourceGroup,
        string appName,
        string settingName,
        string settingUpdateType,
        string? settingValue = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(appName), appName),
            (nameof(settingName), settingName),
            (nameof(settingUpdateType), settingUpdateType));

        if (!AppSettingsUpdateCommand.ValidateUpdateType(settingUpdateType, out var errorMessage))
        {
            throw new ArgumentException(errorMessage);
        }

        if (!AppSettingsUpdateCommand.ValidateSettingValue(settingUpdateType, settingValue, out errorMessage))
        {
            throw new ArgumentException(errorMessage);
        }

        var webAppResource = await GetWebAppResourceAsync(subscription, resourceGroup, appName, tenant, retryPolicy, cancellationToken);
        var configResource = await webAppResource.GetApplicationSettingsAsync(cancellationToken: cancellationToken);

        // Don't worry about an else case here because validation should have already caught invalid update types
        string updateResultMessage = string.Empty;
        if ("add".Equals(settingUpdateType, StringComparison.OrdinalIgnoreCase))
        {
            if (!configResource.Value.Properties.TryAdd(settingName, settingValue!))
            {
                // Can early out here because the setting already exists.
                return $"Failed to add application setting '{settingName}' because it already exists.";
            }

            updateResultMessage = $"Application setting '{settingName}' added successfully.";
        }
        else if ("set".Equals(settingUpdateType, StringComparison.OrdinalIgnoreCase))
        {
            configResource.Value.Properties[settingName] = settingValue!;
            updateResultMessage = $"Application setting '{settingName}' set successfully.";
        }
        else if ("delete".Equals(settingUpdateType, StringComparison.OrdinalIgnoreCase))
        {
            if (!configResource.Value.Properties.Remove(settingName))
            {
                // Can early out here because the setting doesn't exist.
                return $"Application setting '{settingName}' doesn't exist, deletion is skipped.";
            }
            updateResultMessage = $"Application setting '{settingName}' deleted successfully.";
        }

        await webAppResource.UpdateApplicationSettingsAsync(configResource.Value, cancellationToken: cancellationToken);

        return updateResultMessage;
    }
}
