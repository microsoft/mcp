// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net.Http.Headers;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Authentication;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.AppService.Commands;
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

        return CreateDatabaseConnectionInfo(databaseType, databaseServer, databaseName, finalConnectionString, connectionStringName);
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
    public async Task<List<DeploymentDetails>> GetDeploymentsAsync(
        string subscription,
        string resourceGroup,
        string appName,
        string? deploymentId = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(resourceGroup), resourceGroup), (nameof(appName), appName));

        var webAppResource = await GetWebAppResourceAsync(subscription, resourceGroup, appName, tenant, retryPolicy, cancellationToken);

        var results = new List<DeploymentDetails>();

        if (deploymentId == null)
        {
            await foreach (var deployment in webAppResource.GetSiteDeployments().GetAllAsync(cancellationToken: cancellationToken))
            {
                results.Add(MapToDeploymentDetails(deployment.Data));
            }
        }
        else
        {
            var deployment = await webAppResource.GetSiteDeploymentAsync(deploymentId, cancellationToken: cancellationToken);
            results.Add(MapToDeploymentDetails(deployment.Value.Data));
        }

        return results;
    }

    private static DeploymentDetails MapToDeploymentDetails(WebAppDeploymentData deployment)
        => new(deployment.Id.Name, deployment.ResourceType.ToString(), deployment.Kind, deployment.IsActive,
            deployment.Status, deployment.Author, deployment.Deployer, deployment.StartOn, deployment.EndOn);

    public async Task<List<DetectorDetails>> ListDetectorsAsync(
        string subscription,
        string resourceGroup,
        string appName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(resourceGroup), resourceGroup), (nameof(appName), appName));

        // TODO (alzimmer): Once https://github.com/Azure/azure-sdk-for-net/issues/51444 is resolved,
        // use WebSiteResource.GetSiteDetectors().GetAllAsync instead of using a direct HttpClient.
        // var results = new List<DetectorDetails>();
        // var webAppResource = await GetWebAppResourceAsync(subscription, resourceGroup, appName, tenant, retryPolicy, cancellationToken);
        // await foreach (var detector = await webAppResource.GetSiteDetectors().GetAllAsync(cancellationToken))
        // {
        //     results.Add(MapToDetectorDetails(detector.Data));
        // }
        return await CallDetectorsAsync(tenant, subscription, resourceGroup, appName, MapToListDetectorDetails, cancellationToken: cancellationToken);
    }

    private static List<DetectorDetails> MapToListDetectorDetails(JsonDocument jsonDocument)
    {
        if (!jsonDocument.RootElement.TryGetProperty("value", out var detectorsArray))
        {
            throw new InvalidOperationException($"Unexpected response format: 'value' property is missing.");
        }

        if (detectorsArray.ValueKind == JsonValueKind.Array)
        {
            var results = new List<DetectorDetails>();
            foreach (var detectorElement in detectorsArray.EnumerateArray())
            {
                results.Add(MapToDetectorDetails(detectorElement.GetProperty("properties").GetProperty("metadata")));
            }

            return results;
        }
        else if (detectorsArray.ValueKind == JsonValueKind.Null)
        {
            return [];
        }
        else
        {
            throw new InvalidOperationException($"Unexpected response format: 'value' property is not an array or null, was '{detectorsArray.ValueKind}'.");
        }
    }

    private static DetectorDetails MapToDetectorDetails(JsonElement metadata)
    {
        var name = metadata.GetProperty("name").GetString()!;
        var type = metadata.GetProperty("type").GetString()!;
        var description = metadata.GetProperty("description").GetString();
        var category = metadata.GetProperty("category").GetString();
        var categories = (metadata.TryGetProperty("analysisTypes", out var analysisTypesElement) && analysisTypesElement.ValueKind == JsonValueKind.Array)
            ? analysisTypesElement.EnumerateArray().Select(at => at.GetString() ?? string.Empty).Where(at => !string.IsNullOrEmpty(at)).ToList()
            : null;

        return new DetectorDetails(name, type, description, category, categories);
    }

    public async Task<DiagnosisResults> DiagnoseDetectorAsync(
        string subscription,
        string resourceGroup,
        string appName,
        string detectorName,
        DateTimeOffset? startTime = null,
        DateTimeOffset? endTime = null,
        string? interval = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(appName), appName),
            (nameof(detectorName), detectorName));

        // TODO (alzimmer): Once https://github.com/Azure/azure-sdk-for-net/issues/51444 is resolved,
        // // use WebSiteResource.GetSiteDetectorAsync instead of using a direct HttpClient.
        // var webAppResource = await GetWebAppResourceAsync(subscription, resourceGroup, appName, tenant, retryPolicy, cancellationToken);
        // var diagnoses = await webAppResource.GetSiteDetectorAsync(detectorName, startTime, endTime, interval, cancellationToken);

        // return new DiagnosesResults(diagnoses.Value.Data.Dataset, diagnoses.Value.Data.Metadata);
        return await CallDetectorsAsync(tenant, subscription, resourceGroup, appName, MapToDiagnosesResults, detectorName: detectorName, cancellationToken: cancellationToken);
    }

    private static DiagnosisResults MapToDiagnosesResults(JsonDocument jsonDocument)
    {
        if (!jsonDocument.RootElement.TryGetProperty("properties", out var properties))
        {
            throw new InvalidOperationException($"Unexpected response format: 'properties' property is missing.");
        }

        var dataset = JsonSerializer.Deserialize(properties.GetProperty("dataset"), AppServiceJsonContext.Default.IListDiagnosticDataset)!;
        var detector = MapToDetectorDetails(properties.GetProperty("metadata"));

        return new DiagnosisResults(dataset, detector);
    }

    private string GetDetectorsEndpoint(string subscriptionId, string resourceGroupName, string siteName, string? detectorName = null)
    {
        string subscriptionPath = string.IsNullOrEmpty(detectorName)
            ? $"subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Web/sites/{siteName}/detectors?api-version=2025-05-01"
            : $"subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Web/sites/{siteName}/detectors/{detectorName}?api-version=2025-05-01";
        return _tenantService.CloudConfiguration.CloudType switch
        {
            AzureCloudConfiguration.AzureCloud.AzurePublicCloud => $"https://management.azure.com/{subscriptionPath}",
            AzureCloudConfiguration.AzureCloud.AzureChinaCloud => $"https://management.chinacloudapi.cn/{subscriptionPath}",
            AzureCloudConfiguration.AzureCloud.AzureUSGovernmentCloud => $"https://management.usgovcloudapi.net/{subscriptionPath}",
            _ => $"https://management.azure.com/{subscriptionPath}"
        };
    }

    private async Task<T> CallDetectorsAsync<T>(
        string? tenant,
        string subscription,
        string resourceGroup,
        string appName,
        Func<JsonDocument, T> mapFunc,
        string? detectorName = null,
        CancellationToken cancellationToken = default)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Get, GetDetectorsEndpoint(subscription, resourceGroup, appName, detectorName));
        var scopes = new string[]
        {
            _tenantService.CloudConfiguration.ArmEnvironment.DefaultScope
        };
        var clientRequestId = "AzMcp" + Guid.NewGuid().ToString();
        var tokenRequestContext = new TokenRequestContext(scopes, clientRequestId);

        var tokenCredential = await _tenantService.GetTokenCredentialAsync(tenant, cancellationToken: cancellationToken);
        var accessToken = await tokenCredential.GetTokenAsync(tokenRequestContext, cancellationToken);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken.Token);
        httpRequest.Headers.Add("User-Agent", UserAgent);
        httpRequest.Headers.Add("x-ms-client-request-id", clientRequestId);
        httpRequest.Headers.Add("x-ms-app", "AzureMCP");
        httpRequest.Headers.Add("x-ms-client-version", "AppService.Client.Light");
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var httpResponse = await TenantService.GetClient().SendAsync(httpRequest, HttpCompletionOption.ResponseContentRead, cancellationToken);
        if (!httpResponse.IsSuccessStatusCode)
        {
            string errorContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Request failed with status code {httpResponse.StatusCode}: {errorContent}");
        }

        using var contentStream = await httpResponse.Content.ReadAsStreamAsync(cancellationToken);
        using var jsonDoc = await JsonDocument.ParseAsync(contentStream, cancellationToken: cancellationToken);

        return mapFunc(jsonDoc);
    }
}
