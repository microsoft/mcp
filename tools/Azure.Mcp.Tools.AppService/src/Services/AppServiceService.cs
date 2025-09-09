// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
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
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ILogger<AppServiceService> _logger = logger;

    public async Task<DatabaseConnectionInfo> AddDatabaseAsync(
        string appName,
        string resourceGroup,
        string databaseType,
        string databaseServer,
        string databaseName,
        string connectionString,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        _logger.LogInformation(
            "Adding database connection to App Service {AppName} in resource group {ResourceGroup}",
            appName, resourceGroup);

        try
        {
            // Validate inputs
            ValidateRequiredParameters(appName, resourceGroup, databaseType, databaseServer, databaseName, subscription);

            // Get Azure resources
            var webApp = await GetWebAppResourceAsync(subscription, resourceGroup, appName, tenant, retryPolicy);

            // Prepare connection string
            var finalConnectionString = PrepareConnectionString(connectionString, databaseType, databaseServer, databaseName);
            var connectionStringName = $"{databaseName}Connection";

            // Update web app configuration
            await UpdateWebAppConnectionStringAsync(webApp, connectionStringName, finalConnectionString, databaseType);

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
        string appName, string? tenant, RetryPolicyOptions? retryPolicy)
    {
        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);

        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);
        if (resourceGroupResource?.Value == null)
        {
            throw new ArgumentException($"Resource group '{resourceGroup}' not found in subscription '{subscription}'.");
        }

        var webApps = resourceGroupResource.Value.GetWebSites();
        var webAppResource = await webApps.GetAsync(appName);
        if (webAppResource?.Value == null)
        {
            throw new ArgumentException($"Web app '{appName}' not found in resource group '{resourceGroup}'.");
        }

        return webAppResource.Value;
    }

    private static string PrepareConnectionString(string? connectionString, string databaseType,
        string databaseServer, string databaseName)
    {
        return string.IsNullOrWhiteSpace(connectionString)
            ? BuildConnectionString(databaseType, databaseServer, databaseName)
            : connectionString;
    }

    private static async Task UpdateWebAppConnectionStringAsync(WebSiteResource webApp, string connectionStringName,
        string connectionString, string databaseType)
    {
        // Get current web app configuration
        var configResource = webApp.GetWebSiteConfig();
        var config = await configResource.GetAsync();

        if (config?.Value?.Data == null)
        {
            throw new InvalidOperationException($"Unable to retrieve configuration for web app '{webApp.Data.Name}'.");
        }

        // Prepare connection strings collection
        var connectionStrings = config.Value.Data.ConnectionStrings?.ToList() ?? new List<ConnStringInfo>();

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

        var updatedConfig = await configResource.CreateOrUpdateAsync(Azure.WaitUntil.Completed, configData);
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

    private static void ValidateDatabaseType(string databaseType)
    {
        var supportedTypes = new[] { "sqlserver", "mysql", "postgresql", "cosmosdb" };
        if (!supportedTypes.Contains(databaseType, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Unsupported database type: {databaseType}. Supported types: {string.Join(", ", supportedTypes)}");
        }
    }

    private static ConnectionStringType GetConnectionStringType(string databaseType)
    {
        return databaseType.ToLowerInvariant() switch
        {
            "sqlserver" => ConnectionStringType.SqlServer,
            "mysql" => ConnectionStringType.MySql,
            "postgresql" => ConnectionStringType.PostgreSql,
            "cosmosdb" => ConnectionStringType.Custom,
            _ => throw new ArgumentException($"Unsupported database type: {databaseType}")
        };
    }

    private static string BuildConnectionString(string databaseType, string databaseServer, string databaseName)
    {
        return databaseType.ToLowerInvariant() switch
        {
            "sqlserver" => $"Server={databaseServer};Database={databaseName};User Id={{username}};Password={{password}};TrustServerCertificate=True;",
            "mysql" => $"Server={databaseServer};Database={databaseName};Uid={{username}};Pwd={{password}};",
            "postgresql" => $"Host={databaseServer};Database={databaseName};Username={{username}};Password={{password}};",
            "cosmosdb" => $"AccountEndpoint=https://{databaseServer}.documents.azure.com:443/;AccountKey={{key}};Database={databaseName};",
            _ => throw new ArgumentException($"Unsupported database type: {databaseType}")
        };
    }
}
