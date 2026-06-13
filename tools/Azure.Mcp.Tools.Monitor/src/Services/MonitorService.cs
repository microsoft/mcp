// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Models;
using Azure.Mcp.Tools.Monitor.Models.ActivityLog;
using Azure.Monitor.Query.Logs;
using Azure.Monitor.Query.Logs.Models;
using Azure.ResourceManager.OperationalInsights;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Mcp.Core.Validation;

namespace Azure.Mcp.Tools.Monitor.Services;

public class MonitorService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    IResourceGroupService resourceGroupService,
    IResourceResolverService resourceResolverService,
    IHttpClientFactory httpClientFactory,
    ILogger<MonitorService> logger) : BaseAzureService(tenantService), IMonitorService
{
    private const string ActivityLogApiVersion = "2017-03-01-preview";
    private readonly ITenantService _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly ILogger<MonitorService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<List<JsonNode>> QueryResourceLogs(
        string subscription,
        string resourceId,
        string query,
        string table,
        int? hours,
        int? limit,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(resourceId), resourceId), (nameof(table), table));

        query = BuildQuery(query, table, limit);
        KqlQueryValidator.ValidateQuerySafety(query);

        var credential = await GetCredential(tenant, cancellationToken);
        var options = AddDefaultPolicies(new LogsQueryClientOptions());
        options.Audience = GetLogsQueryAudience();

        options.ConfigureRetryOptions(retryPolicy);
        options.Transport = new HttpClientTransport(_httpClientFactory.CreateClient());
        var client = new LogsQueryClient(credential, options);
        var timeRange = new LogsQueryTimeRange(TimeSpan.FromHours(hours ?? 24));

        try
        {
            var response = await client.QueryResourceAsync(
                ResourceIdentifier.Parse(resourceId),
                query,
                timeRange,
                options: null,
                cancellationToken);
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
            _logger.LogError(ex, errorMessage);
            throw;
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
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(workspace), workspace), (nameof(query), query));
        KqlQueryValidator.ValidateQuerySafety(query);

        var credential = await GetCredential(tenant, cancellationToken);
        var options = AddDefaultPolicies(new LogsQueryClientOptions());
        options.Audience = GetLogsQueryAudience();

        options.ConfigureRetryOptions(retryPolicy);
        options.Transport = new HttpClientTransport(_httpClientFactory.CreateClient());
        var client = new LogsQueryClient(credential, options);

        var (workspaceId, _) = await GetWorkspaceInfo(workspace, subscription, tenant, retryPolicy, cancellationToken);

        try
        {
            var response = await client.QueryWorkspaceAsync(
                workspaceId,
                query,
                new(TimeSpan.FromDays(timeSpanDays)),
                options: null,
                cancellationToken
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
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            throw new KeyNotFoundException($"Workspace '{workspaceId}' not found.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying workspace '{WorkspaceId}'. Query: {Query}", workspaceId, query);
            throw;
        }
    }

    public async Task<List<string>> ListTables(
        string subscription,
        string resourceGroup,
        string workspace,
        string? tableType,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(resourceGroup), resourceGroup), (nameof(workspace), workspace));

        var (_, resolvedWorkspaceName) = await GetWorkspaceInfo(workspace, subscription, tenant, retryPolicy, cancellationToken);

        var resourceGroupResource = await resourceGroupService.GetResourceGroupResource(subscription, resourceGroup, tenant, retryPolicy, cancellationToken) ??
            throw new Exception($"Resource group {resourceGroup} not found in subscription {subscription}");
        var workspaceResponse = await resourceGroupResource.GetOperationalInsightsWorkspaceAsync(resolvedWorkspaceName, cancellationToken)
            .ConfigureAwait(false);

        if (workspaceResponse?.Value == null)
        {
            throw new Exception($"Workspace {resolvedWorkspaceName} not found in resource group {resourceGroup}");
        }

        var workspaceResource = workspaceResponse.Value;
        var tableOperations = workspaceResource.GetOperationalInsightsTables();
        var tables = await tableOperations.GetAllAsync(cancellationToken)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return [.. tables
            .Where(table => string.IsNullOrEmpty(tableType) ||
                string.Equals(table.Data.Schema.TableType?.ToString(), tableType, StringComparison.OrdinalIgnoreCase))
            .Select(table => table.Data.Name ?? string.Empty)
            .Where(name => !string.IsNullOrEmpty(name))
            .OrderBy(name => name)];
    }

    public async Task<List<WorkspaceInfo>> ListWorkspaces(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        if (!string.IsNullOrEmpty(resourceGroup))
        {
            var rgResource = await resourceGroupService.GetResourceGroupResource(subscription, resourceGroup, tenant, retryPolicy, cancellationToken)
                ?? throw new Exception($"Resource group '{resourceGroup}' not found in subscription '{subscription}'.");

            return await rgResource
                .GetOperationalInsightsWorkspaces()
                .GetAllAsync(cancellationToken)
                .Select(workspace => new WorkspaceInfo
                {
                    Name = workspace.Data.Name,
                    CustomerId = workspace.Data.CustomerId?.ToString() ?? string.Empty,
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        var subscriptionResource = await subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);

        var workspaces = await subscriptionResource
            .GetOperationalInsightsWorkspacesAsync(cancellationToken)
            .Select(workspace => new WorkspaceInfo
            {
                Name = workspace.Data.Name,
                CustomerId = workspace.Data.CustomerId?.ToString() ?? string.Empty,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return workspaces;
    }
    public async Task<List<JsonNode>> QueryWorkspaceLogs(
        string subscription,
        string workspace,
        string query,
        string table,
        int? hours,
        int? limit,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(workspace), workspace), (nameof(table), table));

        var (workspaceId, _) = await GetWorkspaceInfo(workspace, subscription, tenant, retryPolicy, cancellationToken);
        query = BuildQuery(query, table, limit);
        ValidateRequiredParameters((nameof(query), query));
        KqlQueryValidator.ValidateQuerySafety(query);

        try
        {
            var credential = await GetCredential(tenant, cancellationToken);
            var options = AddDefaultPolicies(new LogsQueryClientOptions());
            options.Audience = GetLogsQueryAudience();

            options.ConfigureRetryOptions(retryPolicy);
            options.Transport = new HttpClientTransport(_httpClientFactory.CreateClient());
            var client = new LogsQueryClient(credential, options);
            var timeRange = new LogsQueryTimeRange(TimeSpan.FromHours(hours ?? 24));

            var response = await client.QueryWorkspaceAsync(
                workspaceId,
                query,
                timeRange,
                options: null,
                cancellationToken);

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

            _logger.LogError(ex, errorMessage);
            throw;
        }
    }

    // Helper to build the query string with table and limit
    // Helper to build the query string with table and limit; internal for testability
    internal static string BuildQuery(string query, string table, int? limit)
    {
        if (!string.IsNullOrEmpty(query) && s_predefinedQueries.ContainsKey(query.Trim().ToLower()))
        {
            query = s_predefinedQueries[query.Trim().ToLower()];
            query = query.Replace(TablePlaceholder, KqlSanitizer.EscapeIdentifier(table));
        }
        // Add limit if not present; use OrdinalIgnoreCase to avoid locale-sensitive comparison (e.g. Turkish locale)
        if (limit.HasValue && !query.Contains("limit", StringComparison.OrdinalIgnoreCase))
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
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(resourceGroup), resourceGroup), (nameof(workspace), workspace));

        var (_, resolvedWorkspaceName) = await GetWorkspaceInfo(workspace, subscription, tenant, retryPolicy, cancellationToken);

        var resourceGroupResource = await resourceGroupService.GetResourceGroupResource(subscription, resourceGroup, tenant, retryPolicy, cancellationToken)
            ?? throw new Exception($"Resource group {resourceGroup} not found in subscription {subscription}");
        var workspaceResponse = await resourceGroupResource.GetOperationalInsightsWorkspaceAsync(resolvedWorkspaceName, cancellationToken)
            .ConfigureAwait(false);

        if (workspaceResponse?.Value == null)
        {
            throw new Exception($"Workspace {resolvedWorkspaceName} not found in resource group {resourceGroup}");
        }

        var workspaceResource = workspaceResponse.Value;
        var tableOperations = workspaceResource.GetOperationalInsightsTables();
        var tables = await tableOperations.GetAllAsync(cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);

        var tableTypes = tables
            .Select(table => table.Data.Schema.TableType?.ToString() ?? string.Empty)
            .Where(type => !string.IsNullOrEmpty(type))
            .Distinct()
            .OrderBy(type => type)
            .ToList();

        return tableTypes;
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
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(resourceName), resourceName));

        if (top < 1)
        {
            top = 10;
        }

        // Resolve the resource ID from the resource name
        var resourceIdentifier = await resourceResolverService.ResolveResourceIdAsync(
            subscription, resourceGroup, resourceType, resourceName, tenant, retryPolicy, cancellationToken);

        string resourceId = resourceIdentifier.ToString();
        string subscriptionId = resourceIdentifier.SubscriptionId
            ?? throw new ArgumentException($"Unable to extract subscription ID from resource ID: {resourceId}");

        // Get the activity logs from the Azure Management API — top is passed to limit server-side pagination
        var activityLogs = await CallActivityLogApiAsync(subscriptionId, resourceId, hours, eventLevel, top, tenant, retryPolicy, cancellationToken);

        return activityLogs;
    }

    private async Task<List<ActivityLogEventData>> CallActivityLogApiAsync(
        string subscriptionId,
        string resourceId,
        double hours,
        ActivityLogEventLevel? eventLevel,
        int top,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken)
    {
        var returnValue = new List<ActivityLogEventData>();

        string endpoint = GetLogActivityEndpointString(subscriptionId);

        // Build the query parameters — include $top to constrain server-side results and reduce unnecessary pages
        var uriBuilder = new UriBuilder(endpoint);
        string query = $"api-version={ActivityLogApiVersion}&$top={top}";

        // Create the time filter
        DateTimeOffset startDate = DateTimeOffset.UtcNow.AddHours(-hours).ToUniversalTime();
        DateTimeOffset endDate = DateTimeOffset.UtcNow;
        string filter = $"eventTimestamp ge '{startDate:yyyy-MM-ddTHH:mm:ss.fffZ}' " +
                       $"and eventTimestamp le '{endDate:yyyy-MM-ddTHH:mm:ss.fffZ}' " +
                       $"and resourceId eq '{resourceId}'";

        if (eventLevel != null)
        {
            filter += $" and level eq '{eventLevel}'";
        }

        query += $"&$filter={Uri.EscapeDataString(filter)}";
        uriBuilder.Query = query;

        var accessToken = await GetArmAccessTokenAsync(tenant, cancellationToken);

        // Make paginated requests, stopping as soon as the requested count is reached
        string? nextRequestUrl = uriBuilder.Uri.ToString();
        do
        {
            ActivityLogListResponse listResponse = await MakeActivityLogRequestAsync(nextRequestUrl, accessToken.Token, cancellationToken);
            returnValue.AddRange(listResponse.Value);
            if (returnValue.Count >= top)
            {
                break;
            }
            nextRequestUrl = listResponse.NextLink;
        } while (!string.IsNullOrEmpty(nextRequestUrl));

        return returnValue.Take(top).ToList();
    }

    private async Task<ActivityLogListResponse> MakeActivityLogRequestAsync(string url, string token, CancellationToken cancellationToken)
    {
        using HttpRequestMessage httpRequest = new(HttpMethod.Get, url);
        httpRequest.Headers.Authorization = new("Bearer", token);

        var client = _httpClientFactory.CreateClient();
        using HttpResponseMessage response = await client.SendAsync(httpRequest, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            using Stream responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            ActivityLogListResponse? responseObject = await JsonSerializer.DeserializeAsync(
                responseStream,
                MonitorJsonContext.Default.ActivityLogListResponse,
                cancellationToken);
            return responseObject ?? new();
        }
        else
        {
            string responseString = await response.Content.ReadAsStringAsync(cancellationToken);
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

    // Workspace IDs are GUIDs
    private static bool IsWorkspaceId(string workspace) => Guid.TryParse(workspace, out _);

    private async Task<(string id, string name)> GetWorkspaceInfo(
        string workspace,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        // If we're given an ID and need an ID, or given a name and need a name, return as is
        bool isId = IsWorkspaceId(workspace);
        var workspaces = await ListWorkspaces(subscription, resourceGroup: null, tenant, retryPolicy, cancellationToken);

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

    private string GetLogActivityEndpointString(string subscriptionId)
    {
        string subscriptionPath = $"subscriptions/{subscriptionId}/providers/Microsoft.Insights/eventtypes/management/values";
        return _tenantService.CloudConfiguration.CloudType switch
        {
            AzureCloudConfiguration.AzureCloud.AzurePublicCloud => $"https://management.azure.com/{subscriptionPath}",
            AzureCloudConfiguration.AzureCloud.AzureChinaCloud => $"https://management.chinacloudapi.cn/{subscriptionPath}",
            AzureCloudConfiguration.AzureCloud.AzureUSGovernmentCloud => $"https://management.usgovcloudapi.net/{subscriptionPath}",
            _ => $"https://management.azure.com/{subscriptionPath}"
        };
    }

    private LogsQueryAudience GetLogsQueryAudience()
    {
        return _tenantService.CloudConfiguration.CloudType switch
        {
            AzureCloudConfiguration.AzureCloud.AzurePublicCloud => LogsQueryAudience.AzurePublicCloud,
            AzureCloudConfiguration.AzureCloud.AzureChinaCloud => LogsQueryAudience.AzureChina,
            AzureCloudConfiguration.AzureCloud.AzureUSGovernmentCloud => LogsQueryAudience.AzureGovernment,
            _ => LogsQueryAudience.AzurePublicCloud
        };
    }

}
