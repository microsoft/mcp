using Azure.Core;
using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.AppContainers;
using Azure.ResourceManager.AppService;
using Azure.ResourceManager.Resources;

namespace Azure.Mcp.Tools.Deploy.Services.Util;

public class AppLogRetriever(TokenCredential credential, string subscriptionId, string resourceGroupName)
{
    private readonly string _subscriptionId = subscriptionId;
    private readonly string _resourceGroupName = resourceGroupName;
    private readonly Dictionary<string, string> _apps = new();
    private readonly Dictionary<string, string> _logs = new();
    private readonly List<string> _logAnalyticsWorkspaceIds = new();

    private ArmClient? _armClient;
    private LogsQueryClient? _queryClient;

    public async Task InitializeAsync()
    {
        _armClient = new ArmClient(credential, _subscriptionId);
        _queryClient = new LogsQueryClient(credential);

        // Verify resource group exists
        var subscription = _armClient!.GetSubscriptionResource(new($"/subscriptions/{_subscriptionId}"));
        var resourceGroupResponse = await subscription.GetResourceGroupAsync(_resourceGroupName);
        if (resourceGroupResponse?.Value == null)
        {
            throw new InvalidOperationException($"Resource group '{_resourceGroupName}' not found in subscription '{_subscriptionId}'.");
        }
    }

    public async Task GetLogAnalyticsWorkspacesInfoAsync()
    {
        var subscription = _armClient!.GetSubscriptionResource(new($"/subscriptions/{_subscriptionId}"));
        var resourceGroup = await subscription.GetResourceGroupAsync(_resourceGroupName);

        var filter = "resourceType eq 'Microsoft.OperationalInsights/workspaces'";

        await foreach (var resource in resourceGroup.Value.GetGenericResourcesAsync(filter: filter))
        {
            _logAnalyticsWorkspaceIds.Add(resource.Id.ToString());
        }

        if (_logAnalyticsWorkspaceIds.Count == 0)
        {
            throw new InvalidOperationException($"No log analytics workspaces found for resource group {_resourceGroupName}. Logs cannot be retrieved using this tool.");
        }
    }

    private static bool IsResourceTypeMatch(GenericResource resource, ResourceType resourceType)
    {
        var resourceTypeString = resourceType.GetResourceTypeString();
        var parts = resourceTypeString.Split('|');
        var expectedType = parts[0];
        var expectedKind = parts.Length > 1 ? parts[1] : null;

        return resource.Data.ResourceType.ToString() == expectedType &&
               (expectedKind == null || resource.Data.Kind?.StartsWith(expectedKind) == true);
    }

    public async Task<List<(string name, ResourceType type)>> GetAllSupportedResourcesAsync()
    {
        var subscription = _armClient!.GetSubscriptionResource(new($"/subscriptions/{_subscriptionId}"));
        var resourceGroup = await subscription.GetResourceGroupAsync(_resourceGroupName);

        var resources = new List<(string name, ResourceType type)>();

        await foreach (var resource in resourceGroup.Value.GetGenericResourcesAsync())
        {
            foreach (ResourceType resourceType in Enum.GetValues<ResourceType>())
            {
                if (IsResourceTypeMatch(resource, resourceType))
                {
                    resources.Add((resource.Data.Name, resourceType));
                    break;
                }
            }
        }

        return resources;
    }

    public async Task<GenericResource> RegisterAppAsync(ResourceType resourceType, string resourceName)
    {
        var subscription = _armClient!.GetSubscriptionResource(new($"/subscriptions/{_subscriptionId}"));
        var resourceGroup = await subscription.GetResourceGroupAsync(_resourceGroupName);

        var apps = new List<GenericResource>();

        await foreach (var resource in resourceGroup.Value.GetGenericResourcesAsync())
        {
            if (resource.Data.Name.Equals(resourceName, StringComparison.OrdinalIgnoreCase))
            {
                if (IsResourceTypeMatch(resource, resourceType))
                {
                    _logs[resource.Id.ToString()] = string.Empty;
                    apps.Add(resource);
                }
            }
        }

        return apps.Count switch
        {
            0 => throw new InvalidOperationException($"No resources found for resource type {resourceType} in resource {resourceName}"),
            > 1 => throw new InvalidOperationException($"Multiple resources found for resource type {resourceType} in resource {resourceName}"),
            _ => apps[0]
        };
    }

    private static string GetContainerAppLogsQuery(string containerAppName, int limit) =>
        $"ContainerAppConsoleLogs_CL | where ContainerAppName_s == '{containerAppName}' | order by _timestamp_d desc | project TimeGenerated, Log_s | take {limit}";

    private static string GetAppServiceLogsQuery(string appServiceResourceId, int limit) =>
        $"AppServiceConsoleLogs | where _ResourceId == '{appServiceResourceId.ToLowerInvariant()}' | order by TimeGenerated desc | project TimeGenerated, ResultDescription | take {limit}";

    private static string GetFunctionAppLogsQuery(string functionAppName, int limit) =>
        $"AppTraces | where AppRoleName == '{functionAppName}' | order by TimeGenerated desc | project TimeGenerated, Message | take {limit}";

    public async Task<string> QueryAppLogsAsync(ResourceType resourceType, string resourceName, int? limit = null)
    {
        var app = await RegisterAppAsync(resourceType, resourceName);
        var getLogErrors = new List<string>();
        var getLogSuccess = false;
        var logSearchQuery = string.Empty;
        DateTimeOffset? lastDeploymentTime = null;

        var actualLimit = limit ?? 200;
        DateTimeOffset endTime = DateTime.UtcNow;
        DateTimeOffset startTime = endTime.AddHours(-4);

        switch (resourceType)
        {
            case ResourceType.ContainerApps:
                logSearchQuery = GetContainerAppLogsQuery(app.Data.Name, actualLimit);
                // Get last deployment time for container apps
                var containerAppResource = _armClient!.GetContainerAppResource(app.Id);
                var containerApp = await containerAppResource.GetAsync();

                await foreach (var revision in containerApp.Value.GetContainerAppRevisions())
                {
                    var revisionData = await revision.GetAsync();
                    if (revisionData.Value.Data.IsActive == true)
                    {
                        lastDeploymentTime = revisionData.Value.Data.CreatedOn;
                        break;
                    }
                }
                break;

            case ResourceType.AppService:
            case ResourceType.FunctionApp:
                var webSiteResource = _armClient!.GetWebSiteResource(app.Id);

                await foreach (var deployment in webSiteResource.GetSiteDeployments())
                {
                    var deploymentData = await deployment.GetAsync();
                    if (deploymentData.Value.Data.IsActive == true)
                    {
                        lastDeploymentTime = deploymentData.Value.Data.StartOn;
                        break;
                    }
                }

                logSearchQuery = resourceType == ResourceType.AppService
                    ? GetAppServiceLogsQuery(app.Id.ToString(), actualLimit)
                    : GetFunctionAppLogsQuery(app.Data.Name, actualLimit);
                break;

            default:
                throw new ArgumentException($"Unsupported resource type: {resourceType}");
        }

        // startTime is now, endTime is 1 hour ago

        if (lastDeploymentTime.HasValue && lastDeploymentTime > startTime)
        {
            startTime = lastDeploymentTime ?? startTime;
        }

        foreach (var logAnalyticsId in _logAnalyticsWorkspaceIds)
        {
            try
            {
                var timeRange = new QueryTimeRange(startTime, endTime);
                var response = await _queryClient!.QueryResourceAsync(new(logAnalyticsId), logSearchQuery, timeRange);

                if (response.Value.Status == LogsQueryResultStatus.Success)
                {
                    foreach (var table in response.Value.AllTables)
                    {
                        foreach (var row in table.Rows)
                        {
                            _logs[app.Id.ToString()] += $"[{row[0]}] {row[1]}\n";
                        }
                    }
                    getLogSuccess = true;
                    break;
                }
            }
            catch (Exception ex)
            {
                getLogErrors.Add($"Error retrieving logs for {app.Data.Name} from {logAnalyticsId}: {ex.Message}");
            }
        }

        if (!getLogSuccess)
        {
            throw new InvalidOperationException($"Errors: {string.Join(", ", getLogErrors)}");
        }

        return $"Console Logs for {resourceName} with resource ID {app.Id} between {startTime} and {endTime}:\n{_logs[app.Id.ToString()]}";
    }
}

public enum ResourceType
{
    AppService,
    ContainerApps,
    FunctionApp
}

public static class ResourceTypeExtensions
{
    private static readonly Dictionary<string, ResourceType> HostToResourceType = new()
    {
        { "containerapp", ResourceType.ContainerApps },
        { "appservice", ResourceType.AppService },
        { "function", ResourceType.FunctionApp }
    };

    private static readonly Dictionary<ResourceType, string> ResourceTypeToString = new()
    {
        { ResourceType.AppService, "Microsoft.Web/sites|app" },
        { ResourceType.ContainerApps, "Microsoft.App/containerApps" },
        { ResourceType.FunctionApp, "Microsoft.Web/sites|functionapp" }
    };

    public static ResourceType GetResourceTypeFromHost(string host)
    {
        return HostToResourceType.TryGetValue(host, out var resourceType)
            ? resourceType
            : throw new ArgumentException($"Unknown host type: {host}");
    }

    public static string GetResourceTypeString(this ResourceType resourceType)
    {
        return ResourceTypeToString[resourceType];
    }
}
