// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Models;
using Azure.Mcp.Tools.Monitor.Models.ActivityLog;
using Azure.Monitor.Query.Logs;
using Azure.Monitor.Query.Logs.Models;
using Azure.ResourceManager.OperationalInsights;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Mcp.Core.Validation;

namespace Azure.Mcp.Tools.Monitor.Services;

public class MonitorService(IAzureService azureService, IResourceResolverService resourceResolverService, ILogger<MonitorService> logger)
    : BaseAzureService(azureService), IMonitorService
{
    private const string ActivityLogApiVersion = "2017-03-01-preview";
    private readonly ILogger<MonitorService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<List<JsonNode>> QueryResourceLogs(
        string subscription,
        string resourceId,
        string query,
        string table,
        int? hours,
        int? limit,
        string? tenant,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(resourceId), resourceId), (nameof(table), table));

        query = BuildQuery(query, table, limit);
        KqlQueryValidator.ValidateQuerySafety(query);

        if (AzureService.CloudConfiguration.CloudType == AzureCloudConfiguration.AzureCloud.CustomCloud)
        {
            var resourceIdentifier = ResourceIdentifier.Parse(resourceId);
            return await QueryCustomLogsAsync(
                $"v1/{resourceIdentifier.ToString().TrimStart('/')}/query",
                query,
                TimeSpan.FromHours(hours ?? 24),
                tenant,
                cancellationToken);
        }

        var credential = await GetCredential(tenant, cancellationToken);
        var options = AddDefaultPolicies(new LogsQueryClientOptions());
        options.Audience = GetLogsQueryAudience();

        options.Transport = new HttpClientTransport(AzureService.GetClient());
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
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(workspace), workspace), (nameof(query), query));
        KqlQueryValidator.ValidateQuerySafety(query);
        var (workspaceId, _) = await GetWorkspaceInfo(workspace, subscription, tenant, cancellationToken);

        if (AzureService.CloudConfiguration.CloudType == AzureCloudConfiguration.AzureCloud.CustomCloud)
        {
            return await QueryCustomLogsAsync(
                $"v1/workspaces/{Uri.EscapeDataString(workspaceId)}/query",
                query,
                TimeSpan.FromDays(timeSpanDays),
                tenant,
                cancellationToken);
        }

        var credential = await GetCredential(tenant, cancellationToken);
        var options = AddDefaultPolicies(new LogsQueryClientOptions());
        options.Audience = GetLogsQueryAudience();

        options.Transport = new HttpClientTransport(AzureService.GetClient());
        var client = new LogsQueryClient(credential, options);

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

    public async Task<List<string>> ListTables(
        string subscription,
        string resourceGroup,
        string workspace,
        string? tableType,
        string? tenant,
        CancellationToken cancellationToken)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(resourceGroup), resourceGroup), (nameof(workspace), workspace));

        var (_, resolvedWorkspaceName) = await GetWorkspaceInfo(workspace, subscription, tenant, cancellationToken);

        var resourceGroupResource = await AzureService.GetResourceGroupResource(subscription, resourceGroup, tenant, cancellationToken: cancellationToken) ??
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
            .Where(table => string.IsNullOrEmpty(tableType) || table.Data.Schema.TableType.ToString() == tableType)
            .Select(table => table.Data.Name ?? string.Empty) // ensure non-null
            .Where(name => !string.IsNullOrEmpty(name))
            .OrderBy(name => name)];
    }

    public async Task<List<WorkspaceInfo>> ListWorkspaces(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        if (!string.IsNullOrEmpty(resourceGroup))
        {
            var rgResource = await AzureService.GetResourceGroupResource(subscription, resourceGroup, tenant, cancellationToken: cancellationToken)
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

        var subscriptionResource = await AzureService.GetSubscription(subscription, tenant, cancellationToken: cancellationToken);

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
        CancellationToken cancellationToken)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(workspace), workspace), (nameof(table), table));

        var (workspaceId, _) = await GetWorkspaceInfo(workspace, subscription, tenant, cancellationToken);
        query = BuildQuery(query, table, limit);
        ValidateRequiredParameters((nameof(query), query));
        KqlQueryValidator.ValidateQuerySafety(query);

        try
        {
            if (AzureService.CloudConfiguration.CloudType == AzureCloudConfiguration.AzureCloud.CustomCloud)
            {
                return await QueryCustomLogsAsync(
                    $"v1/workspaces/{Uri.EscapeDataString(workspaceId)}/query",
                    query,
                    TimeSpan.FromHours(hours ?? 24),
                    tenant,
                    cancellationToken);
            }

            var credential = await GetCredential(tenant, cancellationToken);
            var options = AddDefaultPolicies(new LogsQueryClientOptions());
            options.Audience = GetLogsQueryAudience();

            options.Transport = new HttpClientTransport(AzureService.GetClient());
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
    private static string BuildQuery(string query, string table, int? limit)
    {
        if (!string.IsNullOrEmpty(query) && s_predefinedQueries.ContainsKey(query.Trim().ToLower()))
        {
            query = s_predefinedQueries[query.Trim().ToLower()];
            query = query.Replace(TablePlaceholder, KqlSanitizer.EscapeIdentifier(table));
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
        CancellationToken cancellationToken)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(resourceGroup), resourceGroup), (nameof(workspace), workspace));

        var (_, resolvedWorkspaceName) = await GetWorkspaceInfo(workspace, subscription, tenant, cancellationToken);

        var resourceGroupResource = await AzureService.GetResourceGroupResource(subscription, resourceGroup, tenant, cancellationToken: cancellationToken)
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
        CancellationToken cancellationToken)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(resourceName), resourceName));

        if (top < 1)
        {
            top = 10;
        }

        // Resolve the resource ID from the resource name
        var resourceIdentifier = await resourceResolverService.ResolveResourceIdAsync(
            subscription, resourceGroup, resourceType, resourceName, tenant, cancellationToken);

        string resourceId = resourceIdentifier.ToString();
        string subscriptionId = resourceIdentifier.SubscriptionId
            ?? throw new ArgumentException($"Unable to extract subscription ID from resource ID: {resourceId}");

        // Get the activity logs from the Azure Management API
        var activityLogs = await CallActivityLogApiAsync(subscriptionId, resourceId, hours, eventLevel, tenant, cancellationToken);

        // Take only the requested number of logs
        return activityLogs.Take(top).ToList();
    }

    private async Task<List<ActivityLogEventData>> CallActivityLogApiAsync(
        string subscriptionId,
        string resourceId,
        double hours,
        ActivityLogEventLevel? eventLevel,
        string? tenant,
        CancellationToken cancellationToken)
    {
        var returnValue = new List<ActivityLogEventData>();

        string endpoint = GetLogActivityEndpointString(subscriptionId);

        // Build the query parameters
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

        var accessToken = await GetArmAccessTokenAsync(tenant, cancellationToken);

        // Make paginated requests
        string? nextRequestUrl = uriBuilder.Uri.ToString();
        do
        {
            ActivityLogListResponse listResponse = await MakeActivityLogRequestAsync(nextRequestUrl, accessToken.Token, cancellationToken);
            returnValue.AddRange(listResponse.Value);
            nextRequestUrl = listResponse.NextLink;
        } while (!string.IsNullOrEmpty(nextRequestUrl));

        return returnValue;
    }

    private async Task<ActivityLogListResponse> MakeActivityLogRequestAsync(string url, string token, CancellationToken cancellationToken)
    {
        using HttpRequestMessage httpRequest = new(HttpMethod.Get, url);
        httpRequest.Headers.Authorization = new("Bearer", token);

        var client = AzureService.GetClient();
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
        CancellationToken cancellationToken = default)
    {
        // If we're given an ID and need an ID, or given a name and need a name, return as is
        bool isId = IsWorkspaceId(workspace);
        var workspaces = await ListWorkspaces(subscription, resourceGroup: null, tenant: tenant, cancellationToken: cancellationToken);

        // Find the workspace
        var matchingWorkspace = workspaces.FirstOrDefault(w =>
            isId ? w.CustomerId.Equals(workspace, StringComparison.OrdinalIgnoreCase)
                : w.Name.Equals(workspace, StringComparisons.ResourceName));

        if (matchingWorkspace == null)
        {
            throw new Exception($"Could not find workspace with {(isId ? "ID" : "name")} {workspace}");
        }

        return (matchingWorkspace.CustomerId, matchingWorkspace.Name);
    }

    private string GetLogActivityEndpointString(string subscriptionId)
    {
        string subscriptionPath = $"subscriptions/{subscriptionId}/providers/Microsoft.Insights/eventtypes/management/values";
        return $"{AzureService.CloudConfiguration.ArmEnvironment.Endpoint.AbsoluteUri.TrimEnd('/')}/{subscriptionPath}";
    }

    private LogsQueryAudience GetLogsQueryAudience()
    {
        return AzureService.CloudConfiguration.CloudType switch
        {
            AzureCloudConfiguration.AzureCloud.AzurePublicCloud => LogsQueryAudience.AzurePublicCloud,
            AzureCloudConfiguration.AzureCloud.AzureChinaCloud => LogsQueryAudience.AzureChina,
            AzureCloudConfiguration.AzureCloud.AzureUSGovernmentCloud => LogsQueryAudience.AzureGovernment,
            _ => throw new NotSupportedException("Log Analytics queries are not supported for custom clouds until the Log Analytics SDK supports custom endpoints.")
        };
    }

    private async Task<List<JsonNode>> QueryCustomLogsAsync(
        string relativePath,
        string query,
        TimeSpan timeRange,
        string? tenant,
        CancellationToken cancellationToken)
    {
        var credential = await GetCredential(tenant, cancellationToken);
        var accessToken = await credential.GetTokenAsync(
            new TokenRequestContext([AzureService.CloudConfiguration.LogAnalyticsScope]),
            cancellationToken);

        var endpoint = new Uri($"{AzureService.CloudConfiguration.LogAnalyticsEndpoint.AbsoluteUri.TrimEnd('/')}/{relativePath}");
        var uriBuilder = new UriBuilder(endpoint);
        uriBuilder.Query = $"timespan={Uri.EscapeDataString(System.Xml.XmlConvert.ToString(timeRange))}";

        using var request = new HttpRequestMessage(HttpMethod.Post, uriBuilder.Uri);
        request.Headers.Authorization = new("Bearer", accessToken.Token);
        request.Content = new StringContent(
            new JsonObject { ["query"] = query }.ToJsonString(),
            System.Text.Encoding.UTF8,
            "application/json");

        using var response = await AzureService.GetClient().SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var statusCode = (int)response.StatusCode;
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
            var errorDetail = !string.IsNullOrWhiteSpace(responseText) ? responseText : response.ReasonPhrase ?? "Unknown Error";
            throw new RequestFailedException(statusCode, $"Log Analytics query request failed with status {statusCode}: {errorDetail}");
        }

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);
        return ParseCustomQueryResults(document.RootElement);
    }

    private static List<JsonNode> ParseCustomQueryResults(JsonElement root)
    {
        if (!root.TryGetProperty("tables", out var tables) || tables.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("The Log Analytics response did not contain a valid tables array.");
        }

        if (tables.GetArrayLength() == 0)
        {
            return [];
        }

        var table = tables[0];
        if (!table.TryGetProperty("columns", out var columns) ||
            !table.TryGetProperty("rows", out var rows) ||
            columns.ValueKind != JsonValueKind.Array ||
            rows.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("The Log Analytics response table was missing columns or rows.");
        }

        var columnNames = columns.EnumerateArray().Select(column =>
        {
            if (!column.TryGetProperty("name", out var name) || name.ValueKind != JsonValueKind.String ||
                string.IsNullOrEmpty(name.GetString()))
            {
                throw new InvalidOperationException("The Log Analytics response contained an invalid column.");
            }

            return name.GetString()!;
        }).ToArray();
        var results = new List<JsonNode>();
        foreach (var row in rows.EnumerateArray())
        {
            var rowObject = new JsonObject();
            for (var index = 0; index < columnNames.Length && index < row.GetArrayLength(); index++)
            {
                rowObject[columnNames[index]] = JsonNode.Parse(row[index].GetRawText());
            }

            results.Add(rowObject);
        }

        return results;
    }

}
