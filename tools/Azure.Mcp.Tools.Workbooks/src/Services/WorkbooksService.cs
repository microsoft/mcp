// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Workbooks.Models;
using Azure.ResourceManager.ApplicationInsights;
using Azure.ResourceManager.ApplicationInsights.Models;
using Azure.ResourceManager.ResourceGraph;
using Azure.ResourceManager.ResourceGraph.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Workbooks.Services;

public class WorkbooksService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ILogger<WorkbooksService> logger)
    : BaseAzureResourceService(subscriptionService, tenantService), IWorkbooksService
{
    private const int MaxConcurrency = 10;
    private const string WorkbooksResourceType = "microsoft.insights/workbooks";
    private readonly ISubscriptionService _subscriptionService = subscriptionService;
    private readonly ILogger<WorkbooksService> _logger = logger;
    // Static semaphore intentionally shared across all requests to enforce API rate limiting.
    // In HTTP mode (remote MCP server), this limits concurrent Azure API calls to MaxConcurrency
    // across all users, preventing throttling from Azure Resource Manager.
    private static readonly SemaphoreSlim _throttle = new(MaxConcurrency);

    public async Task<WorkbookListResult> ListWorkbooksAsync(
        IReadOnlyList<string>? subscriptions = null,
        IReadOnlyList<string>? resourceGroups = null,
        WorkbookFilters? filters = null,
        int maxResults = 50,
        bool includeTotalCount = true,
        OutputFormat outputFormat = OutputFormat.Standard,
        RetryPolicyOptions? retryPolicy = null,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get accessible tenants
            var tenants = await TenantService.GetTenants(cancellationToken);
            var currentTenant = tenants.FirstOrDefault()
                ?? throw new InvalidOperationException("No accessible tenants found");

            // Build the query with optional scope filtering
            var queryText = BuildWorkbooksQuery(resourceGroups, filters, maxResults, outputFormat);
            var query = new ResourceQueryContent(queryText);

            // If subscriptions are specified, add them to the query scope
            if (subscriptions?.Count > 0)
            {
                foreach (var sub in subscriptions)
                {
                    // Resolve subscription name to ID if needed
                    var subscriptionResource = await _subscriptionService.GetSubscription(sub, tenant, retryPolicy, cancellationToken);
                    query.Subscriptions.Add(subscriptionResource.Data.SubscriptionId);
                }
            }

            ResourceQueryResult resources = await currentTenant.GetResourcesAsync(query, cancellationToken);

            var workbooks = new List<WorkbookInfo>();

            if (resources != null && resources.Count > 0 && resources.Data != null)
            {
                using JsonDocument document = JsonDocument.Parse(resources.Data);
                JsonElement resourcesArray = document.RootElement;

                if (resourcesArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement resource in resourcesArray.EnumerateArray())
                    {
                        workbooks.Add(ParseWorkbookFromResourceGraph(resource, outputFormat));
                    }
                }
            }

            // Get total count if requested
            int? totalCount = null;
            if (includeTotalCount)
            {
                totalCount = await GetTotalCountAsync(subscriptions, resourceGroups, filters, tenant, retryPolicy, cancellationToken);
            }

            return new WorkbookListResult(
                workbooks,
                totalCount,
                ContinuationToken: null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list workbooks");
            throw new InvalidOperationException($"Failed to list workbooks: {ex.Message}", ex);
        }
    }

    public async Task<WorkbookBatchResult> GetWorkbooksAsync(
        IReadOnlyList<string> workbookIds,
        RetryPolicyOptions? retryPolicy = null,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        if (workbookIds == null || workbookIds.Count == 0)
        {
            throw new ArgumentException("At least one workbook ID is required", nameof(workbookIds));
        }

        var succeeded = new List<WorkbookInfo>();
        var failed = new List<WorkbookError>();

        var tasks = workbookIds.Select(async id =>
        {
            await _throttle.WaitAsync(cancellationToken);
            try
            {
                var workbook = await GetSingleWorkbookAsync(id, retryPolicy, tenant, cancellationToken);
                return (Workbook: workbook, Error: (WorkbookError?)null);
            }
            catch (Exception ex)
            {
                return (Workbook: (WorkbookInfo?)null, Error: HandleWorkbookException(id, ex));
            }
            finally
            {
                _throttle.Release();
            }
        });

        var results = await Task.WhenAll(tasks);

        foreach (var result in results)
        {
            if (result.Workbook != null)
            {
                succeeded.Add(result.Workbook);
            }
            else if (result.Error != null)
            {
                failed.Add(result.Error);
            }
        }

        return new WorkbookBatchResult(succeeded, failed);
    }

    public async Task<WorkbookInfo?> CreateWorkbookAsync(
        string subscription,
        string resourceGroupName,
        string displayName,
        string serializedData,
        string sourceId,
        RetryPolicyOptions? retryPolicy = null,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroupName), resourceGroupName),
            (nameof(displayName), displayName),
            (nameof(serializedData), serializedData),
            (nameof(sourceId), sourceId));

        ValidateSerializedData(serializedData);

        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken)
                ?? throw new InvalidOperationException($"Subscription '{subscription}' not found");

            var resourceGroupResource = await subscriptionResource.GetResourceGroups().GetAsync(resourceGroupName, cancellationToken);
            if (resourceGroupResource?.Value == null)
            {
                throw new InvalidOperationException($"Resource group '{resourceGroupName}' not found in subscription '{subscription}'");
            }

            var workbookData = new ApplicationInsightsWorkbookData(resourceGroupResource.Value.Data.Location)
            {
                DisplayName = displayName,
                SerializedData = serializedData,
                Category = "workbook",
                Kind = "shared",
                SourceId = new ResourceIdentifier(sourceId)
            };

            var workbookName = Guid.NewGuid().ToString();

            var workbookCollection = resourceGroupResource.Value.GetApplicationInsightsWorkbooks();
            var createOperation = await workbookCollection.CreateOrUpdateAsync(WaitUntil.Completed, workbookName, workbookData, cancellationToken: cancellationToken);
            var createdWorkbook = createOperation.Value;

            _logger.LogInformation("Successfully created workbook with name: {WorkbookName} in resource group: {ResourceGroup}", workbookName, resourceGroupName);

            return CreateWorkbookInfo(createdWorkbook, displayName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workbook '{DisplayName}' in resource group '{ResourceGroup}'", displayName, resourceGroupName);
            throw new InvalidOperationException($"Failed to create workbook: {ex.Message}", ex);
        }
    }

    public async Task<WorkbookInfo?> UpdateWorkbookAsync(
        string workbookId,
        string? displayName = null,
        string? serializedContent = null,
        RetryPolicyOptions? retryPolicy = null,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateWorkbookId(workbookId);

        if (serializedContent != null)
        {
            ValidateSerializedData(serializedContent);
        }

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);

            var workbookResourceId = new ResourceIdentifier(workbookId);
            var workbookResource = armClient.GetApplicationInsightsWorkbookResource(workbookResourceId)
                ?? throw new InvalidOperationException($"Workbook with ID '{workbookId}' not found");

            var workbookResponse = await workbookResource.GetAsync(true, cancellationToken);
            var workbook = workbookResponse.Value;

            if (workbook?.Data == null)
            {
                _logger.LogWarning("Workbook data is null for ID {WorkbookId}", workbookId);
                return null;
            }

            var patchData = new ApplicationInsightsWorkbookPatch { Kind = "shared" };

            if (!string.IsNullOrEmpty(displayName))
            {
                patchData.DisplayName = displayName;
            }

            if (!string.IsNullOrEmpty(serializedContent))
            {
                patchData.SerializedData = serializedContent;
            }

            var updateResponse = await workbookResource.UpdateAsync(patchData, cancellationToken: cancellationToken);
            var updatedWorkbook = updateResponse.Value;

            _logger.LogInformation("Successfully updated workbook with ID: {WorkbookId}", workbookId);

            return CreateWorkbookInfo(updatedWorkbook, workbookId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating workbook with ID: {WorkbookId}", workbookId);
            throw new InvalidOperationException($"Failed to update workbook: {ex.Message}", ex);
        }
    }

    public async Task<WorkbookDeleteBatchResult> DeleteWorkbooksAsync(
        IReadOnlyList<string> workbookIds,
        RetryPolicyOptions? retryPolicy = null,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        if (workbookIds == null || workbookIds.Count == 0)
        {
            throw new ArgumentException("At least one workbook ID is required", nameof(workbookIds));
        }

        var succeeded = new List<string>();
        var failed = new List<WorkbookError>();

        var tasks = workbookIds.Select(async id =>
        {
            await _throttle.WaitAsync(cancellationToken);
            try
            {
                await DeleteSingleWorkbookAsync(id, retryPolicy, tenant, cancellationToken);
                return (Id: id, Error: (WorkbookError?)null);
            }
            catch (Exception ex)
            {
                var error = HandleWorkbookException(id, ex) as WorkbookError;
                return (Id: id, Error: error);
            }
            finally
            {
                _throttle.Release();
            }
        });

        var results = await Task.WhenAll(tasks);

        foreach (var result in results)
        {
            if (result.Error == null)
            {
                succeeded.Add(result.Id);
            }
            else
            {
                failed.Add(result.Error);
            }
        }

        return new WorkbookDeleteBatchResult(succeeded, failed);
    }

    private async Task<WorkbookInfo?> GetSingleWorkbookAsync(
        string workbookId,
        RetryPolicyOptions? retryPolicy,
        string? tenant,
        CancellationToken cancellationToken)
    {
        ValidateWorkbookId(workbookId);

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);

        var workbookResourceId = new ResourceIdentifier(workbookId);
        var workbookResource = armClient.GetApplicationInsightsWorkbookResource(workbookResourceId)
            ?? throw new InvalidOperationException($"Workbook with ID '{workbookId}' not found");

        // Always include content - the purpose of show/get is to retrieve full workbook details
        var workbookResponse = await workbookResource.GetAsync(canFetchContent: true, cancellationToken);
        var workbook = workbookResponse.Value;

        if (workbook?.Data == null)
        {
            _logger.LogWarning("Workbook data is null for ID {WorkbookId}", workbookId);
            return null;
        }

        return CreateWorkbookInfo(workbook, workbookId);
    }

    private async Task DeleteSingleWorkbookAsync(
        string workbookId,
        RetryPolicyOptions? retryPolicy,
        string? tenant,
        CancellationToken cancellationToken)
    {
        ValidateWorkbookId(workbookId);

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);

        var workbookResourceId = new ResourceIdentifier(workbookId);
        var workbookResource = armClient.GetApplicationInsightsWorkbookResource(workbookResourceId)
            ?? throw new InvalidOperationException($"Workbook with ID '{workbookId}' not found");

        await workbookResource.DeleteAsync(WaitUntil.Completed, cancellationToken);
        _logger.LogInformation("Successfully deleted workbook with ID: {WorkbookId}", workbookId);
    }

    private async Task<int?> GetTotalCountAsync(
        IReadOnlyList<string>? subscriptions,
        IReadOnlyList<string>? resourceGroups,
        WorkbookFilters? filters,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken)
    {
        try
        {
            var tenants = await TenantService.GetTenants(cancellationToken);
            var currentTenant = tenants.FirstOrDefault();
            if (currentTenant == null)
            {
                return null;
            }

            var countQuery = BuildCountQuery(resourceGroups, filters);
            var query = new ResourceQueryContent(countQuery);

            if (subscriptions?.Count > 0)
            {
                foreach (var sub in subscriptions)
                {
                    var subscriptionResource = await _subscriptionService.GetSubscription(sub, tenant, retryPolicy, cancellationToken);
                    query.Subscriptions.Add(subscriptionResource.Data.SubscriptionId);
                }
            }

            ResourceQueryResult resources = await currentTenant.GetResourcesAsync(query, cancellationToken);

            if (resources == null || resources.Count == 0 || resources.Data == null)
            {
                return null;
            }

            using JsonDocument document = JsonDocument.Parse(resources.Data);
            var result = document.RootElement;

            if (result.ValueKind == JsonValueKind.Array && result.GetArrayLength() > 0)
            {
                var firstItem = result[0];
                if (firstItem.TryGetProperty("totalCount", out var countElement))
                {
                    return countElement.GetInt32();
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get total count for workbooks query");
            return null;
        }
    }

    private static string BuildWorkbooksQuery(
        IReadOnlyList<string>? resourceGroups,
        WorkbookFilters? filters,
        int maxResults,
        OutputFormat outputFormat)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append($"resources | where type =~ '{WorkbooksResourceType}'");

        // Resource group filter
        if (resourceGroups?.Count > 0)
        {
            var rgFilter = string.Join(", ", resourceGroups.Select(rg => $"'{EscapeKqlString(rg)}'"));
            sb.Append($" | where resourceGroup in~ ({rgFilter})");
        }

        // Apply optional filters
        if (filters?.HasFilters == true)
        {
            if (!string.IsNullOrEmpty(filters.Kind))
            {
                sb.Append($" | where kind =~ '{EscapeKqlString(filters.Kind)}'");
            }

            if (!string.IsNullOrEmpty(filters.Category))
            {
                sb.Append($" | where properties.category =~ '{EscapeKqlString(filters.Category)}'");
            }

            if (!string.IsNullOrEmpty(filters.SourceId))
            {
                sb.Append($" | where properties.sourceId =~ '{EscapeKqlString(filters.SourceId)}'");
            }

            if (!string.IsNullOrEmpty(filters.NameContains))
            {
                sb.Append($" | where properties.displayName contains '{EscapeKqlString(filters.NameContains)}'");
            }

            if (filters.ModifiedAfter.HasValue)
            {
                sb.Append($" | where properties.timeModified >= datetime('{filters.ModifiedAfter.Value:o}')");
            }
        }

        // Limit results
        sb.Append($" | limit {Math.Min(maxResults, 1000)}");

        // Project fields based on output format
        sb.Append(outputFormat switch
        {
            OutputFormat.Summary => " | project id, name, resourceGroup, subscriptionId",
            OutputFormat.Standard => " | project id, name, kind, resourceGroup, subscriptionId, location, tags, properties",
            OutputFormat.Full => " | project id, name, kind, resourceGroup, subscriptionId, location, tags, properties",
            _ => " | project id, name, kind, resourceGroup, subscriptionId, location, tags, properties"
        });

        return sb.ToString();
    }

    private static string BuildCountQuery(
        IReadOnlyList<string>? resourceGroups,
        WorkbookFilters? filters)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append($"resources | where type =~ '{WorkbooksResourceType}'");

        if (resourceGroups?.Count > 0)
        {
            var rgFilter = string.Join(", ", resourceGroups.Select(rg => $"'{EscapeKqlString(rg)}'"));
            sb.Append($" | where resourceGroup in~ ({rgFilter})");
        }

        if (filters?.HasFilters == true)
        {
            if (!string.IsNullOrEmpty(filters.Kind))
            {
                sb.Append($" | where kind =~ '{EscapeKqlString(filters.Kind)}'");
            }

            if (!string.IsNullOrEmpty(filters.Category))
            {
                sb.Append($" | where properties.category =~ '{EscapeKqlString(filters.Category)}'");
            }

            if (!string.IsNullOrEmpty(filters.SourceId))
            {
                sb.Append($" | where properties.sourceId =~ '{EscapeKqlString(filters.SourceId)}'");
            }

            if (!string.IsNullOrEmpty(filters.NameContains))
            {
                sb.Append($" | where properties.displayName contains '{EscapeKqlString(filters.NameContains)}'");
            }

            if (filters.ModifiedAfter.HasValue)
            {
                sb.Append($" | where properties.timeModified >= datetime('{filters.ModifiedAfter.Value:o}')");
            }
        }

        sb.Append(" | summarize totalCount = count()");

        return sb.ToString();
    }

    private static WorkbookInfo ParseWorkbookFromResourceGraph(JsonElement resource, OutputFormat outputFormat)
    {
        var resourceId = resource.GetProperty("id").GetString() ?? "";
        var resourceName = resource.GetProperty("name").GetString() ?? "";

        if (outputFormat == OutputFormat.Summary)
        {
            return new WorkbookInfo(
                WorkbookId: resourceId,
                DisplayName: resourceName,
                Description: null,
                Category: null,
                Location: null,
                Kind: null,
                Tags: null,
                SerializedData: null,
                Version: null,
                TimeModified: null,
                UserId: null,
                SourceId: null);
        }

        var location = resource.TryGetProperty("location", out var loc) ? loc.GetString() : "";
        var kind = resource.TryGetProperty("kind", out var k) ? k.GetString() : "";
        var tags = resource.TryGetProperty("tags", out var tagsElement) ? tagsElement : default;
        var properties = resource.TryGetProperty("properties", out var props) ? props : default;

        return new WorkbookInfo(
            WorkbookId: resourceId,
            DisplayName: properties.ValueKind != JsonValueKind.Undefined && properties.TryGetProperty("displayName", out var displayName)
                ? displayName.GetString() : null,
            Description: properties.ValueKind != JsonValueKind.Undefined && properties.TryGetProperty("description", out var desc)
                ? desc.GetString() : null,
            Category: properties.ValueKind != JsonValueKind.Undefined && properties.TryGetProperty("category", out var cat)
                ? cat.GetString() : null,
            Location: location,
            Kind: kind,
            Tags: tags.ValueKind != JsonValueKind.Undefined && tags.ValueKind != JsonValueKind.Null
                ? ConvertTagsToString(tags) : null,
            SerializedData: outputFormat == OutputFormat.Full && properties.ValueKind != JsonValueKind.Undefined && properties.TryGetProperty("serializedData", out var data)
                ? data.GetString() : null,
            Version: properties.ValueKind != JsonValueKind.Undefined && properties.TryGetProperty("version", out var ver)
                ? ver.GetString() : null,
            TimeModified: properties.ValueKind != JsonValueKind.Undefined && properties.TryGetProperty("timeModified", out var modified)
                ? modified.GetDateTimeOffset() : null,
            UserId: properties.ValueKind != JsonValueKind.Undefined && properties.TryGetProperty("userId", out var user)
                ? user.GetString() : null,
            SourceId: properties.ValueKind != JsonValueKind.Undefined && properties.TryGetProperty("sourceId", out var source)
                ? source.GetString() : null);
    }

    private static WorkbookInfo CreateWorkbookInfo(ApplicationInsightsWorkbookResource workbook, string fallbackId)
    {
        return new WorkbookInfo(
            WorkbookId: workbook.Id?.ToString() ?? fallbackId,
            DisplayName: workbook.Data.DisplayName,
            Description: workbook.Data.Description,
            Category: workbook.Data.Category,
            Location: workbook.Data.Location.ToString(),
            Kind: workbook.Data.Kind?.ToString(),
            Tags: ConvertTagsToString(workbook.Data.Tags),
            SerializedData: workbook.Data.SerializedData,
            Version: workbook.Data.Version,
            TimeModified: workbook.Data.ModifiedOn,
            UserId: workbook.Data.UserId,
            SourceId: workbook.Data.SourceId?.ToString());
    }

    private static WorkbookError HandleWorkbookException(string resourceId, Exception ex)
    {
        return ex switch
        {
            RequestFailedException { Status: 404 } =>
                new WorkbookError(resourceId, 404, $"Workbook not found. Verify the ID exists and you have access: {resourceId}"),

            RequestFailedException { Status: 403 } reqEx =>
                new WorkbookError(resourceId, 403, $"Authorization denied. Required role: Workbook Contributor. Details: {reqEx.Message}"),

            RequestFailedException { Status: 409 } =>
                new WorkbookError(resourceId, 409, "Conflict: Workbook was modified by another process. Retry with updated etag."),

            Azure.Identity.AuthenticationFailedException =>
                new WorkbookError(resourceId, 401, "Authentication failed. Run 'az login' to authenticate."),

            ArgumentException argEx =>
                new WorkbookError(resourceId, 400, $"Invalid parameter: {argEx.Message}"),

            _ => new WorkbookError(resourceId, 500, $"Unexpected error: {ex.Message}")
        };
    }

    private static void ValidateWorkbookId(string? id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id, nameof(id));

        if (!id.StartsWith("/subscriptions/", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Invalid resource ID format. Expected ARM resource ID starting with '/subscriptions/'.", nameof(id));
        }
    }

    private static void ValidateSerializedData(string? data)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(data, nameof(data));

        try
        {
            using var doc = JsonDocument.Parse(data);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Invalid JSON in serialized data: {ex.Message}", nameof(data));
        }
    }

    private static string? ConvertTagsToString(JsonElement tagsElement)
    {
        if (tagsElement.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var tags = new List<string>();
        foreach (var tag in tagsElement.EnumerateObject())
        {
            var value = tag.Value.GetString() ?? "";
            tags.Add($"{tag.Name}={value}");
        }

        return tags.Count > 0 ? string.Join(", ", tags) : null;
    }

    private static string? ConvertTagsToString(IDictionary<string, string>? tags)
    {
        return tags?.Count > 0 ? string.Join(", ", tags.Select(kvp => $"{kvp.Key}={kvp.Value}")) : null;
    }
}
