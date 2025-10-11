// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Http;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Models;
using Azure.Mcp.Tools.Monitor.Models.ActivityLog;
using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;
using Azure.ResourceManager.OperationalInsights;

namespace Azure.Mcp.Tools.Monitor.Services;

public class MonitorService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    IResourceGroupService resourceGroupService,
    IResourceResolverService resourceResolverService,
    IHttpClientService httpClientService) : BaseAzureService(tenantService), IMonitorService
{
    private const string ActivityLogApiVersion = "2017-03-01-preview";
    private const string ActivityLogEndpointFormat
        = "https://management.azure.com/subscriptions/{0}/providers/Microsoft.Insights/eventtypes/management/values";

    // Token caching fields
    private string? _cachedManagementToken;
    private DateTimeOffset _managementTokenExpiryTime;
    private const int TokenExpirationBufferSeconds = 300; // 5 minutes buffer

    public async Task<List<JsonNode>> QueryResourceLogs(
        string subscription,
        string resourceId,
        string query,
        string table,
        int? hours,
        int? limit,
        string? tenant,
        RetryPolicyOptions? retryPolicy)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(resourceId), resourceId), (nameof(table), table));
        query = BuildQuery(query, table, limit);

        var credential = await GetCredential(tenant);
        var options = AddDefaultPolicies(new LogsQueryClientOptions());

        if (retryPolicy != null)
        {
            options.Retry.Delay = TimeSpan.FromSeconds(retryPolicy.DelaySeconds);
            options.Retry.MaxDelay = TimeSpan.FromSeconds(retryPolicy.MaxDelaySeconds);
            options.Retry.MaxRetries = retryPolicy.MaxRetries;
            options.Retry.Mode = retryPolicy.Mode;
            options.Retry.NetworkTimeout = TimeSpan.FromSeconds(retryPolicy.NetworkTimeoutSeconds);
        }
        var client = new LogsQueryClient(credential, options);
        var timeRange = new QueryTimeRange(TimeSpan.FromHours(hours ?? 24));

        try
        {
            var response = await client.QueryResourceAsync(
                ResourceIdentifier.Parse(resourceId),
                query,
                timeRange);
            return ParseQueryResults(response.Value.Table);
        }
        catch (Exception ex)
        {
            string errorMessage = ex switch
            {
                RequestFailedException rfe => $"Azure request failed: {rfe.Status} - {rfe.Message}",
                TimeoutException => "The query timed out. Try simplifying your query or reducing the time range.",
                _ => $"Error querying resource logs: {ex.Message}"
            };
            throw new Exception(errorMessage, ex);
        }
    }

    private const string TablePlaceholder = "{tableName}";

    private static readonly Dictionary<string, string> s_predefinedQueries = new()
    {
        ["recent"] = """
            {tableName}
            | order by TimeGenerated desc
            """,
        ["errors"] = """
            {tableName}
            | where Level == "ERROR"
            | order by TimeGenerated desc
            """
    };

    public async Task<List<JsonNode>> QueryWorkspace(
        string subscription,
        string workspace,
        string query,
        int timeSpanDays = 1,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(workspace), workspace), (nameof(query), query));

        var credential = await GetCredential(tenant);
        var options = AddDefaultPolicies(new LogsQueryClientOptions());

        if (retryPolicy != null)
        {
            options.Retry.Delay = TimeSpan.FromSeconds(retryPolicy.DelaySeconds);
            options.Retry.MaxDelay = TimeSpan.FromSeconds(retryPolicy.MaxDelaySeconds);
            options.Retry.MaxRetries = retryPolicy.MaxRetries;
            options.Retry.Mode = retryPolicy.Mode;
            options.Retry.NetworkTimeout = TimeSpan.FromSeconds(retryPolicy.NetworkTimeoutSeconds);
        }
        var client = new LogsQueryClient(credential, options);

        try
        {
            var (workspaceId, _) = await GetWorkspaceInfo(workspace, subscription, tenant, retryPolicy);

            var response = await client.QueryWorkspaceAsync(
                workspaceId,
                query,
                new QueryTimeRange(TimeSpan.FromDays(timeSpanDays))
            );

            var results = new List<JsonNode>();
            if (response.Value.Table != null)
            {
                var rows = response.Value.Table.Rows;
                var columns = response.Value.Table.Columns;

                if (rows != null && columns != null && rows.Any())
                {
                    foreach (var row in rows)
                    {
                        var rowDict = new JsonObject();
                        for (int i = 0; i < columns.Count; i++)
                        {
                            rowDict[columns[i].Name] = JsonValue.Create(row[i]?.ToString() ?? "null");
                        }
                        results.Add(rowDict);
                    }
                }
            }
            return results;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error querying workspace: {ex.Message}", ex);
        }
    }

    public async Task<List<string>> ListTables(
        string subscription,
        string resourceGroup,
        string workspace,
        string? tableType,
        string? tenant,
        RetryPolicyOptions? retryPolicy)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(resourceGroup), resourceGroup), (nameof(workspace), workspace));

        try
        {
            var (_, resolvedWorkspaceName) = await GetWorkspaceInfo(workspace, subscription, tenant, retryPolicy);

            var resourceGroupResource = await resourceGroupService.GetResourceGroupResource(subscription, resourceGroup, tenant, retryPolicy) ??
                throw new Exception($"Resource group {resourceGroup} not found in subscription {subscription}");
            var workspaceResponse = await resourceGroupResource.GetOperationalInsightsWorkspaceAsync(resolvedWorkspaceName)
                .ConfigureAwait(false);

            if (workspaceResponse?.Value == null)
            {
                throw new Exception($"Workspace {resolvedWorkspaceName} not found in resource group {resourceGroup}");
            }

            var workspaceResource = workspaceResponse.Value;
            var tableOperations = workspaceResource.GetOperationalInsightsTables();
            var tables = await tableOperations.GetAllAsync()
                .ToListAsync()
                .ConfigureAwait(false);

            return [.. tables
                .Where(table => string.IsNullOrEmpty(tableType) || table.Data.Schema.TableType.ToString() == tableType)
                .Select(table => table.Data.Name ?? string.Empty) // ensure non-null
                .Where(name => !string.IsNullOrEmpty(name))
                .OrderBy(name => name)];
        }
        catch (Exception ex)
        {
            throw new Exception($"Error listing tables for workspace {workspace}: {ex.Message}", ex);
        }
    }

    public async Task<List<WorkspaceInfo>> ListWorkspaces(
        string subscription,
        string? tenant,
        RetryPolicyOptions? retryPolicy)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        try
        {
            var subscriptionResource = await subscriptionService.GetSubscription(subscription, tenant, retryPolicy);

            var workspaces = await subscriptionResource
                .GetOperationalInsightsWorkspacesAsync()
                .Select(workspace => new WorkspaceInfo
                {
                    Name = workspace.Data.Name,
                    CustomerId = workspace.Data.CustomerId?.ToString() ?? string.Empty,
                })
                .ToListAsync()
                .ConfigureAwait(false);

            return workspaces;
        }
        catch (Exception ex) when (ex is not ArgumentNullException)
        {
            throw new Exception($"Error retrieving Log Analytics workspaces: {ex.Message}", ex);
        }
    }
    public async Task<List<JsonNode>> QueryWorkspaceLogs(
        string subscription,
        string workspace,
        string query,
        string table,
        int? hours,
        int? limit,
        string? tenant,
        RetryPolicyOptions? retryPolicy)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(workspace), workspace), (nameof(table), table));

        var (workspaceId, _) = await GetWorkspaceInfo(workspace, subscription, tenant, retryPolicy);
        query = BuildQuery(query, table, limit);
        ValidateRequiredParameters((nameof(query), query));

        try
        {
            var credential = await GetCredential(tenant);
            var options = AddDefaultPolicies(new LogsQueryClientOptions());

            if (retryPolicy != null)
            {
                options.Retry.Delay = TimeSpan.FromSeconds(retryPolicy.DelaySeconds);
                options.Retry.MaxDelay = TimeSpan.FromSeconds(retryPolicy.MaxDelaySeconds);
                options.Retry.MaxRetries = retryPolicy.MaxRetries;
                options.Retry.Mode = retryPolicy.Mode;
                options.Retry.NetworkTimeout = TimeSpan.FromSeconds(retryPolicy.NetworkTimeoutSeconds);
            }
            var client = new LogsQueryClient(credential, options);
            var timeRange = new QueryTimeRange(TimeSpan.FromHours(hours ?? 24));

            var response = await client.QueryWorkspaceAsync(
                workspaceId,
                query,
                timeRange);

            return ParseQueryResults(response.Value.Table);
        }
        catch (Exception ex)
        {
            // Provide a more specific error message based on the exception type
            string errorMessage = ex switch
            {
                RequestFailedException rfe => $"Azure request failed: {rfe.Status} - {rfe.Message}",
                TimeoutException => "The query timed out. Try simplifying your query or reducing the time range.",
                _ => $"Error querying logs: {ex.Message}"
            };

            throw new Exception(errorMessage, ex);
        }
    }

    // Helper to build the query string with table and limit
    private static string BuildQuery(string query, string table, int? limit)
    {
        if (!string.IsNullOrEmpty(query) && s_predefinedQueries.ContainsKey(query.Trim().ToLower()))
        {
            query = s_predefinedQueries[query.Trim().ToLower()];
            query = query.Replace(TablePlaceholder, table);
        }
        // Add limit if not present
        if (limit.HasValue && !query.Contains("limit", StringComparison.CurrentCultureIgnoreCase))
        {
            query = $"{query}\n| limit {limit}";
        }
        return query;
    }

    // Helper to parse query results from a LogsTable
    private static List<JsonNode> ParseQueryResults(LogsTable? table)
    {
        var results = new List<JsonNode>();
        if (table != null)
        {
            var rows = table.Rows;
            var columns = table.Columns;
            if (rows != null && columns != null && rows.Any())
            {
                foreach (var row in rows)
                {
                    var rowDict = new JsonObject();
                    for (int i = 0; i < columns.Count; i++)
                    {
                        rowDict[columns[i].Name] = JsonValue.Create(row[i]?.ToString() ?? "null");
                    }
                    results.Add(rowDict);
                }
            }
        }
        return results;
    }

    public async Task<List<string>> ListTableTypes(
        string subscription,
        string resourceGroup,
        string workspace,
        string? tenant,
        RetryPolicyOptions? retryPolicy)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(resourceGroup), resourceGroup), (nameof(workspace), workspace));
        try
        {
            var (_, resolvedWorkspaceName) = await GetWorkspaceInfo(workspace, subscription, tenant, retryPolicy);

            var resourceGroupResource = await resourceGroupService.GetResourceGroupResource(subscription, resourceGroup, tenant, retryPolicy)
                ?? throw new Exception($"Resource group {resourceGroup} not found in subscription {subscription}");
            var workspaceResponse = await resourceGroupResource.GetOperationalInsightsWorkspaceAsync(resolvedWorkspaceName)
                .ConfigureAwait(false);

            if (workspaceResponse?.Value == null)
            {
                throw new Exception($"Workspace {resolvedWorkspaceName} not found in resource group {resourceGroup}");
            }

            var workspaceResource = workspaceResponse.Value;
            var tableOperations = workspaceResource.GetOperationalInsightsTables();
            var tables = await tableOperations.GetAllAsync().ToListAsync().ConfigureAwait(false);

            var tableTypes = tables
                .Select(table => table.Data.Schema.TableType?.ToString() ?? string.Empty)
                .Where(type => !string.IsNullOrEmpty(type))
                .Distinct()
                .OrderBy(type => type)
                .ToList();

            return tableTypes;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error listing table types for workspace {workspace}: {ex.Message}", ex);
        }
    }

    public async Task<List<ActivityLogEventData>> ListActivityLogs(
        string subscription,
        string resourceName,
        string? resourceGroup,
        string? resourceType,
        double hours,
        ActivityLogEventLevel? eventLevel,
        int top,
        string? tenant,
        RetryPolicyOptions? retryPolicy)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(resourceName), resourceName));

        if (top < 1)
        {
            top = 10;
        }

        try
        {
            // Resolve the resource ID from the resource name
            var resourceIdentifier = await resourceResolverService.ResolveResourceIdAsync(
                subscription, resourceGroup, resourceType, resourceName, tenant, retryPolicy);

            string resourceId = resourceIdentifier.ToString();
            string subscriptionId = resourceIdentifier.SubscriptionId
                ?? throw new ArgumentException($"Unable to extract subscription ID from resource ID: {resourceId}");

            // Get the activity logs from the Azure Management API
            var activityLogs = await CallActivityLogApiAsync(subscriptionId, resourceId, hours, eventLevel, tenant, retryPolicy);

            // Take only the requested number of logs
            return activityLogs.Take(top).ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving activity logs for resource '{resourceName}': {ex.Message}", ex);
        }
    }

    private async Task<List<ActivityLogEventData>> CallActivityLogApiAsync(
        string subscriptionId,
        string resourceId,
        double hours,
        ActivityLogEventLevel? eventLevel,
        string? tenant,
        RetryPolicyOptions? retryPolicy)
    {
        var returnValue = new List<ActivityLogEventData>();

        string endpoint = string.Format(ActivityLogEndpointFormat, subscriptionId);
        var uriBuilder = new UriBuilder(endpoint);

        // Build the query parameters
        string query = $"api-version={ActivityLogApiVersion}";

        // Create the time filter
        DateTimeOffset startDate = DateTimeOffset.UtcNow.AddHours(-hours).ToUniversalTime();
        DateTimeOffset endDate = DateTimeOffset.UtcNow;
        string filter = $"eventTimestamp ge '{startDate:yyyy-MM-ddTHH:mm:ss.fffZ}' " +
                       $"and eventTimestamp le '{endDate:yyyy-MM-ddTHH:mm:ss.fffZ}' " +
                       $"and resourceId eq '{resourceId}'";

        if (eventLevel != null)
        {
            filter += $" and levels eq '{eventLevel}'";
        }

        query += $"&$filter={Uri.EscapeDataString(filter)}";
        uriBuilder.Query = query;

        // Get cached access token
        var tokenString = await GetCachedManagementTokenAsync(tenant);

        // Make paginated requests
        string? nextRequestUrl = uriBuilder.Uri.ToString();
        do
        {
            ActivityLogListResponse listResponse = await MakeActivityLogRequestAsync(nextRequestUrl, tokenString);
            returnValue.AddRange(listResponse.Value);
            nextRequestUrl = listResponse.NextLink;
        } while (!string.IsNullOrEmpty(nextRequestUrl));

        return returnValue;
    }

    private async Task<ActivityLogListResponse> MakeActivityLogRequestAsync(string url, string token)
    {
        using HttpRequestMessage httpRequest = new(HttpMethod.Get, url);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using HttpResponseMessage response = await httpClientService.DefaultClient.SendAsync(httpRequest);

        if (response.IsSuccessStatusCode)
        {
            using Stream responseStream = await response.Content.ReadAsStreamAsync();
            ActivityLogListResponse? responseObject = await JsonSerializer.DeserializeAsync(
                responseStream,
                MonitorJsonContext.Default.ActivityLogListResponse);
            return responseObject ?? new ActivityLogListResponse();
        }
        else
        {
            string responseString = await response.Content.ReadAsStringAsync();
            string errorMessage;
            if (!string.IsNullOrEmpty(responseString))
            {
                errorMessage = responseString;
            }
            else if (!string.IsNullOrEmpty(response.ReasonPhrase))
            {
                errorMessage = response.ReasonPhrase;
            }
            else
            {
                errorMessage = "Unknown Error";
            }
            throw new HttpRequestException($"Activity Log API returned error {response.StatusCode}: {errorMessage}");
        }
    }

    private static bool IsWorkspaceId(string workspace)
    {
        // Workspace IDs are GUIDs
        return Guid.TryParse(workspace, out _);
    }

    private async Task<(string id, string name)> GetWorkspaceInfo(
        string workspace,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        // If we're given an ID and need an ID, or given a name and need a name, return as is
        bool isId = IsWorkspaceId(workspace);
        var workspaces = await ListWorkspaces(subscription, tenant, retryPolicy);

        // Find the workspace
        var matchingWorkspace = workspaces.FirstOrDefault(w =>
            isId ? w.CustomerId.Equals(workspace, StringComparison.OrdinalIgnoreCase)
                : w.Name.Equals(workspace, StringComparison.OrdinalIgnoreCase));

        if (matchingWorkspace == null)
        {
            throw new Exception($"Could not find workspace with {(isId ? "ID" : "name")} {workspace}");
        }

        return (matchingWorkspace.CustomerId, matchingWorkspace.Name);
    }

    private async Task<string> GetCachedManagementTokenAsync(string? tenant)
    {
        return await GetCachedTokenAsync(
            "https://management.azure.com",
            () => _cachedManagementToken,
            token => _cachedManagementToken = token,
            () => _managementTokenExpiryTime,
            expiry => _managementTokenExpiryTime = expiry,
            tenant,
            TokenExpirationBufferSeconds);
    }
}
