using Azure.Core;

namespace Azure.Mcp.Tools.Deploy.Services.Util;

public static class ResourceLogService
{
    public static async Task<string> GetResourceLogsAsync(
        TokenCredential credential,
        string subscriptionId,
        string resourceGroupName,
        int? limit = null)
    {
        var toolErrorLogs = new List<string>();
        var appLogs = new List<string>();

        try
        {
            var appLogRetriever = new AppLogRetriever(credential, subscriptionId, resourceGroupName);
            await appLogRetriever.InitializeAsync();
            await appLogRetriever.GetLogAnalyticsWorkspacesInfoAsync();

            // Auto-discover all supported resources in the resource group
            var resources = await appLogRetriever.GetAllSupportedResourcesAsync();

            if (resources.Count == 0)
            {
                return "No supported resources found in resource group.";
            }

            foreach (var (resourceName, resourceType) in resources)
            {
                try
                {
                    var logs = await appLogRetriever.QueryAppLogsAsync(resourceType, resourceName, limit);
                    appLogs.Add(logs);
                }
                catch (Exception ex)
                {
                    toolErrorLogs.Add($"Error finding app logs for resource {resourceName}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            toolErrorLogs.Add(ex.Message);
        }

        if (appLogs.Count > 0)
        {
            return $"App logs retrieved:\n{string.Join("\n\n", appLogs)}";
        }

        if (toolErrorLogs.Count > 0)
        {
            return $"Error during retrieval of app logs:\n{string.Join("\n", toolErrorLogs)}";
        }

        return "No logs found.";
    }

}