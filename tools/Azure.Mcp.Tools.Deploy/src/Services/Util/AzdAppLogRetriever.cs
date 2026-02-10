using Azure.Monitor.Query.Logs;
using Azure.Monitor.Query.Logs.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.AppContainers;
using Azure.ResourceManager.AppService;
using Azure.ResourceManager.Resources;

namespace Azure.Mcp.Tools.Deploy.Services.Util;

public class AzdAppLogRetriever(ArmClient armClient, LogsQueryClient logsQueryClient, string subscriptionId, string azdEnvName)
{
    private readonly string _subscriptionId = subscriptionId;
    private readonly string _azdEnvName = azdEnvName;
    private readonly Dictionary<string, string> _apps = new();
    private readonly Dictionary<string, string> _logs = new();
    private readonly List<string> _logAnalyticsWorkspaceIds = new();
    private string _resourceGroupName = string.Empty;

    private readonly ArmClient _armClient = armClient ?? throw new ArgumentNullException(nameof(armClient));
    private readonly LogsQueryClient _queryClient = logsQueryClient ?? throw new ArgumentNullException(nameof(logsQueryClient));

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        _resourceGroupName = await GetResourceGroupNameAsync(cancellationToken);
        if (string.IsNullOrEmpty(_resourceGroupName))
        {
            throw new InvalidOperationException($"No resource group with tag {{\"azd-env-name\": {_azdEnvName}}} found.");
        }
    }

    public async Task GetLogAnalyticsWorkspacesInfoAsync(CancellationToken cancellationToken)
    {
        var subscription = _armClient.GetSubscriptionResource(new($"/subscriptions/{_subscriptionId}"));
        var resourceGroup = await subscription.GetResourceGroupAsync(_resourceGroupName, cancellationToken);

        var filter = "resourceType eq 'Microsoft.OperationalInsights/workspaces'";

        await foreach (var resource in resourceGroup.Value.GetGenericResourcesAsync(filter: filter, cancellationToken: cancellationToken))
        {
            _logAnalyticsWorkspaceIds.Add(resource.Id.ToString());
        }

        if (_logAnalyticsWorkspaceIds.Count == 0)
        {
            throw new InvalidOperationException($"No log analytics workspaces found for resource group {_resourceGroupName}. Logs cannot be retrieved using this tool.");
        }
    }

    public async Task<GenericResource> RegisterAppAsync(ResourceType resourceType, string serviceName, CancellationToken cancellationToken)
    {
        var subscription = _armClient.GetSubscriptionResource(new($"/subscriptions/{_subscriptionId}"));
        var resourceGroup = await subscription.GetResourceGroupAsync(_resourceGroupName, cancellationToken);

        var filter = $"tagName eq 'azd-service-name' and tagValue eq '{serviceName}'";
        var apps = new List<GenericResource>();

        await foreach (var resource in resourceGroup.Value.GetGenericResourcesAsync(filter: filter, cancellationToken: cancellationToken))
        {
            var resourceTypeString = resourceType.GetResourceTypeString();
            var parts = resourceTypeString.Split('|');
            var type = parts[0];
            var kind = parts.Length > 1 ? parts[1] : null;

            if (resource.Data.ResourceType.ToString() == type &&
                (kind == null || resource.Data.Kind?.StartsWith(kind) == true))
            {
                _logs[resource.Id.ToString()] = string.Empty;
                apps.Add(resource);
            }
        }

        return apps.Count switch
        {
            0 => throw new InvalidOperationException($"No resources found for resource type {resourceType} with tag azd-service-name={serviceName}"),
            > 1 => throw new InvalidOperationException($"Multiple resources found for resource type {resourceType} with tag azd-service-name={serviceName}"),
            _ => apps[0]
        };
    }

    private static string GetContainerAppLogsQuery(string containerAppName, int limit) =>
        $"ContainerAppConsoleLogs_CL | where ContainerAppName_s == '{containerAppName}' | order by _timestamp_d desc | project TimeGenerated, Log_s | take {limit}";

    private static string GetAppServiceLogsQuery(string appServiceResourceId, int limit) =>
        $"AppServiceConsoleLogs | where _ResourceId == '{appServiceResourceId.ToLowerInvariant()}' | order by TimeGenerated desc | project TimeGenerated, ResultDescription | take {limit}";

    private static string GetFunctionAppLogsQuery(string functionAppName, int limit) =>
        $"AppTraces | where AppRoleName == '{functionAppName}' | order by TimeGenerated desc | project TimeGenerated, Message | take {limit}";

    public async Task<string> QueryAppLogsAsync(ResourceType resourceType, string serviceName, int? limit = null, CancellationToken cancellationToken = default)
    {
        var app = await RegisterAppAsync(resourceType, serviceName, cancellationToken);
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
                var containerAppResource = _armClient.GetContainerAppResource(app.Id);
                var containerApp = await containerAppResource.GetAsync(cancellationToken);

                await foreach (var revision in containerApp.Value.GetContainerAppRevisions().WithCancellation(cancellationToken))
                {
                    var revisionData = await revision.GetAsync(cancellationToken);
                    if (revisionData.Value.Data.IsActive == true)
                    {
                        lastDeploymentTime = revisionData.Value.Data.CreatedOn;
                        break;
                    }
                }
                break;

            case ResourceType.AppService:
            case ResourceType.FunctionApp:
                var webSiteResource = _armClient.GetWebSiteResource(app.Id);

                await foreach (var deployment in webSiteResource.GetSiteDeployments().WithCancellation(cancellationToken))
                {
                    var deploymentData = await deployment.GetAsync(cancellationToken);
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
                var timeRange = new LogsQueryTimeRange(startTime, endTime);
                var response = await _queryClient.QueryResourceAsync(new(logAnalyticsId), logSearchQuery, timeRange, cancellationToken: cancellationToken);

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

        return $"Console Logs for {serviceName} with resource ID {app.Id} between {startTime} and {endTime}:\n{_logs[app.Id.ToString()]}";
    }

    private async Task<string> GetResourceGroupNameAsync(CancellationToken cancellationToken)
    {
        var subscription = _armClient.GetSubscriptionResource(new($"/subscriptions/{_subscriptionId}"));

        await foreach (var resourceGroup in subscription.GetResourceGroups().WithCancellation(cancellationToken))
        {
            if (resourceGroup.Data.Tags.TryGetValue("azd-env-name", out var envName) && envName == _azdEnvName)
            {
                return resourceGroup.Data.Name;
            }
        }

        return string.Empty;
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
