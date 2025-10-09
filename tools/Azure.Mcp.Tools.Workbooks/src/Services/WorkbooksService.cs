// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Workbooks.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Workbooks.Services;

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.ResourceManager.ApplicationInsights;
using Azure.ResourceManager.ApplicationInsights.Models;
using Azure.ResourceManager.ResourceGraph;
using Azure.ResourceManager.ResourceGraph.Models;

public class WorkbooksService(ISubscriptionService _subscriptionService, ITenantService tenantService, ILogger<WorkbooksService> logger) : BaseAzureService(tenantService), IWorkbooksService
{
    private readonly ILogger<WorkbooksService> _logger = logger;
    private readonly ITenantService _tenantService = tenantService;

    public async Task<List<WorkbookInfo>> ListWorkbooks(string subscription, string resourceGroupName, WorkbookFilters? filters = null, RetryPolicyOptions? retryPolicy = null, string? tenant = null)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(resourceGroupName), resourceGroupName));

        try
        {
            // Resolve subscription to get the actual subscription ID for the query
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
            var subscriptionId = subscriptionResource.Data.SubscriptionId;

            var armClient = await CreateArmClientAsync(tenant, retryPolicy);

            var tenants = await _tenantService.GetTenants();
            var currentTenant = tenants.FirstOrDefault() ?? throw new InvalidOperationException("No accessible tenants found");

            var queryText = BuildWorkbooksQuery(subscriptionId, resourceGroupName, filters);
            var query = new ResourceQueryContent(queryText);

            var resources = await currentTenant.GetResourcesAsync(query);

            var workbooksInRg = new List<WorkbookInfo>();

            // The Resource Graph API returns data directly as a JSON array
            var resourceGraphData = resources.Value.Data;
            using JsonDocument document = JsonDocument.Parse(resourceGraphData.ToStream());
            JsonElement resourcesArray = document.RootElement;

            foreach (JsonElement resource in resourcesArray.EnumerateArray())
            {
                // Each resource is a complete JSON object with id, name, location, tags, properties
                var resourceId = resource.GetProperty("id").GetString() ?? "";
                var resourceName = resource.GetProperty("name").GetString() ?? "";
                var location = resource.GetProperty("location").GetString() ?? "";
                var kind = resource.GetProperty("kind").GetString() ?? "";
                var tags = resource.TryGetProperty("tags", out var tagsElement) ? tagsElement : default;
                var properties = resource.GetProperty("properties");

                workbooksInRg.Add(new WorkbookInfo(
                    WorkbookId: resourceId,
                    DisplayName: properties.TryGetProperty("displayName", out var displayName) ? displayName.GetString() : null,
                    Description: properties.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                    Category: properties.TryGetProperty("category", out var cat) ? cat.GetString() : null,
                    Location: location,
                    Kind: kind,
                    Tags: tags.ValueKind != JsonValueKind.Undefined && tags.ValueKind != JsonValueKind.Null ? ConvertTagsToString(tags) : null,
                    SerializedData: properties.TryGetProperty("serializedData", out var data) ? data.GetString() : null,
                    Version: properties.TryGetProperty("version", out var ver) ? ver.GetString() : null,
                    TimeModified: properties.TryGetProperty("timeModified", out var modified) ? modified.GetDateTimeOffset() : null,
                    UserId: properties.TryGetProperty("userId", out var user) ? user.GetString() : null,
                    SourceId: properties.TryGetProperty("sourceId", out var source) ? source.GetString() : null
                ));
            }

            return workbooksInRg;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list workbooks in resource group '{ResourceGroup}' for subscription '{Subscription}'", resourceGroupName, subscription);
            throw new Exception($"Failed to list workbooks: {ex.Message}", ex);
        }
    }

    public async Task<WorkbookInfo?> GetWorkbook(string workbookId, RetryPolicyOptions? retryPolicy = null, string? tenant = null)
    {
        if (string.IsNullOrEmpty(workbookId))
        {
            _logger.LogWarning("Null or empty workbook ID provided");
            return null;
        }

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy);

            // Parse the workbook resource ID to get the workbook directly
            var workbookResourceId = new ResourceIdentifier(workbookId);
            var workbookResource = armClient.GetApplicationInsightsWorkbookResource(workbookResourceId) ?? throw new Exception($"Workbook with ID '{workbookId}' not found");

            // Get the workbook
            var workbookResponse = await workbookResource.GetAsync(true);
            var workbook = workbookResponse.Value;

            if (workbook?.Data == null)
            {
                _logger.LogWarning("Workbook data is null for ID {WorkbookId}", workbookId);
                return null;
            }

            var workbookInfo = new WorkbookInfo(
                WorkbookId: workbook.Id?.ToString() ?? workbookId,
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
                SourceId: workbook.Data.SourceId
            );

            _logger.LogInformation("Successfully retrieved workbook with ID: {WorkbookId}", workbookId);
            return workbookInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workbook with ID: {WorkbookId}", workbookId);
            throw new Exception($"Failed to get workbook: {ex.Message}", ex);
        }
    }

    public async Task<WorkbookInfo?> UpdateWorkbook(string workbookId, string? displayName = null, string? serializedContent = null, RetryPolicyOptions? retryPolicy = null, string? tenant = null)
    {
        if (string.IsNullOrEmpty(workbookId))
        {
            _logger.LogWarning("Null or empty workbook ID provided");
            return null;
        }

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy);

            // Parse the workbook resource ID to get the workbook directly
            var workbookResourceId = new ResourceIdentifier(workbookId);
            var workbookResource = armClient.GetApplicationInsightsWorkbookResource(workbookResourceId) ?? throw new Exception($"Workbook with ID '{workbookId}' not found");

            // Get the current workbook data
            var workbookResponse = await workbookResource.GetAsync(true);
            var workbook = workbookResponse.Value;

            if (workbook?.Data == null)
            {
                _logger.LogWarning("Workbook data is null for ID {WorkbookId}", workbookId);
                return null;
            }

            // Create a patch document with the updated properties
            var patchData = new ApplicationInsightsWorkbookPatch();

            if (!string.IsNullOrEmpty(displayName))
            {
                patchData.DisplayName = displayName;
            }

            if (!string.IsNullOrEmpty(serializedContent))
            {
                patchData.SerializedData = serializedContent;
            }

            // If not set, won't be able to save?
            patchData.Kind = "shared";

            // Update the workbook
            var updateResponse = await workbookResource.UpdateAsync(patchData);
            var updatedWorkbook = updateResponse.Value;

            _logger.LogInformation("Successfully updated workbook with ID: {WorkbookId}", workbookId);

            return new WorkbookInfo(
                WorkbookId: updatedWorkbook.Id?.ToString() ?? workbookId,
                DisplayName: updatedWorkbook.Data.DisplayName,
                Description: updatedWorkbook.Data.Description,
                Category: updatedWorkbook.Data.Category,
                Location: updatedWorkbook.Data.Location.ToString(),
                Kind: updatedWorkbook.Data.Kind?.ToString(),
                Tags: ConvertTagsToString(updatedWorkbook.Data.Tags),
                SerializedData: updatedWorkbook.Data.SerializedData,
                Version: updatedWorkbook.Data.Version,
                TimeModified: updatedWorkbook.Data.ModifiedOn,
                UserId: updatedWorkbook.Data.UserId,
                SourceId: updatedWorkbook.Data.SourceId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating workbook with ID: {WorkbookId}", workbookId);
            throw new Exception($"Failed to update workbook: {ex.Message}", ex);
        }
    }

    public async Task<WorkbookInfo?> CreateWorkbook(string subscription, string resourceGroupName, string displayName, string serializedData, string sourceId, RetryPolicyOptions? retryPolicy = null, string? tenant = null)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroupName), resourceGroupName),
            (nameof(displayName), displayName),
            (nameof(serializedData), serializedData),
            (nameof(sourceId), sourceId));

        try
        {
            // Get the subscription resource
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy) ?? throw new Exception($"Subscription '{subscription}' not found");
            // Get the resource group
            var resourceGroupResource = await subscriptionResource.GetResourceGroups().GetAsync(resourceGroupName);
            if (resourceGroupResource?.Value == null)
            {
                throw new Exception($"Resource group '{resourceGroupName}' not found in subscription '{subscription}'");
            }

            // Create the workbook data
            var workbookData = new ApplicationInsightsWorkbookData(resourceGroupResource.Value.Data.Location)
            {
                DisplayName = displayName,
                SerializedData = serializedData,
                Category = "workbook",
                Kind = "shared",
                SourceId = new ResourceIdentifier(sourceId)
            };


            // Generate a unique name for the workbook
            var workbookName = Guid.NewGuid().ToString();

            // Create the workbook
            var workbookCollection = resourceGroupResource.Value.GetApplicationInsightsWorkbooks();
            var createOperation = await workbookCollection.CreateOrUpdateAsync(WaitUntil.Completed, workbookName, workbookData);
            var createdWorkbook = createOperation.Value;

            _logger.LogInformation("Successfully created workbook with name: {WorkbookName} in resource group: {ResourceGroup}", workbookName, resourceGroupName);

            return new WorkbookInfo(
                WorkbookId: createdWorkbook.Id?.ToString() ?? "",
                DisplayName: createdWorkbook.Data.DisplayName ?? displayName,
                Description: createdWorkbook.Data.Description,
                Category: createdWorkbook.Data.Category,
                Location: createdWorkbook.Data.Location.ToString(),
                Kind: createdWorkbook.Data.Kind?.ToString(),
                Tags: ConvertTagsToString(createdWorkbook.Data.Tags),
                SerializedData: createdWorkbook.Data.SerializedData,
                Version: createdWorkbook.Data.Version,
                TimeModified: createdWorkbook.Data.ModifiedOn,
                UserId: createdWorkbook.Data.UserId,
                SourceId: createdWorkbook.Data.SourceId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workbook '{DisplayName}' in resource group '{ResourceGroup}'", displayName, resourceGroupName);
            throw new Exception($"Failed to create workbook: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteWorkbook(string workbookId, RetryPolicyOptions? retryPolicy = null, string? tenant = null)
    {
        ValidateRequiredParameters((nameof(workbookId), workbookId));

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy);

            // Parse the workbook resource ID to get the workbook directly
            var workbookResourceId = new ResourceIdentifier(workbookId);
            var workbookResource = armClient.GetApplicationInsightsWorkbookResource(workbookResourceId) ?? throw new Exception($"Workbook with ID '{workbookId}' not found");

            // Delete the workbook
            var response = await workbookResource.DeleteAsync(WaitUntil.Completed);

            _logger.LogInformation("Successfully deleted workbook with ID: {WorkbookId}", workbookId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting workbook with ID: {WorkbookId}", workbookId);
            throw new Exception($"Failed to delete workbook: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Converts a JsonElement containing tags to a comma-separated string representation.
    /// This helps keep the output flat for the model.
    /// </summary>
    private static string? ConvertTagsToString(JsonElement tagsElement)
    {
        if (tagsElement.ValueKind != JsonValueKind.Object)
            return null;

        var tags = new List<string>();
        foreach (var tag in tagsElement.EnumerateObject())
        {
            var value = tag.Value.GetString() ?? "";
            tags.Add($"{tag.Name}={value}");
        }

        return tags.Count > 0 ? string.Join(", ", tags) : null;
    }

    /// <summary>
    /// Converts a dictionary of tags to a comma-separated string representation.
    /// This helps keep the output flat for the model.
    /// </summary>
    private static string? ConvertTagsToString(IDictionary<string, string>? tags)
    {
        return tags?.Count > 0 ? string.Join(", ", tags.Select(kvp => $"{kvp.Key}={kvp.Value}")) : null;
    }

    /// <summary>
    /// Builds a KQL query for retrieving workbooks with optional filters.
    /// </summary>
    private static string BuildWorkbooksQuery(string subscriptionIdentifier, string resourceGroupName, WorkbookFilters? filters)
    {
        var queryText = $@"
            resources
            | where type == 'microsoft.insights/workbooks'
            | where resourceGroup =~ '{EscapeKqlString(resourceGroupName)}'
            | where subscriptionId =~ '{EscapeKqlString(subscriptionIdentifier)}'";

        // Add optional filters if provided
        if (filters?.HasFilters == true)
        {
            if (!string.IsNullOrEmpty(filters.Kind))
            {
                queryText += $@"
            | where kind =~ '{EscapeKqlString(filters.Kind)}'";
            }

            if (!string.IsNullOrEmpty(filters.Category))
            {
                queryText += $@"
            | where properties.category =~ '{EscapeKqlString(filters.Category)}'";
            }

            if (!string.IsNullOrEmpty(filters.SourceId))
            {
                queryText += $@"
            | where properties.sourceId =~ '{EscapeKqlString(filters.SourceId)}'";
            }
        }

        queryText += @"
            | project id, kind, name, location, tags, properties";

        return queryText;
    }
}
