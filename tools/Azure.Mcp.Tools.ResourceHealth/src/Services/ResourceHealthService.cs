// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.ResourceHealth.Models;
using Azure.Mcp.Tools.ResourceHealth.Models.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResourceHealth.Services;

public class ResourceHealthService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    IHttpClientFactory httpClientFactory,
    ILogger<ResourceHealthService> logger)
    : BaseAzureService(tenantService), IResourceHealthService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ITenantService _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly ILogger<ResourceHealthService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private const string ResourceHealthApiVersion = "2025-05-01";

    public async Task<AvailabilityStatus> GetAvailabilityStatusAsync(
        string resourceId,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(resourceId), resourceId));

        // Parse and validate resource ID format using Azure SDK
        var parsedResourceId = ResourceIdentifier.Parse(resourceId);

        var managementEndpoint = _tenantService.CloudConfiguration.ArmEnvironment.Endpoint ?? throw new InvalidOperationException("Management endpoint is not configured.");

        var token = await GetArmAccessTokenAsync(null, cancellationToken);

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", token.Token);

        // Construct URL safely using Uri to ensure path is relative to base
        var relativePath = $"{parsedResourceId}/providers/Microsoft.ResourceHealth/availabilityStatuses/current?api-version={ResourceHealthApiVersion}";
        var requestUri = new Uri(managementEndpoint, relativePath);

        using var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        await EnsureResourceHealthSuccessAsync(response, cancellationToken, resourceId, parsedResourceId.ResourceType.ToString());

        var apiResponse = await response.Content.ReadFromJsonAsync(ResourceHealthJsonContext.Default.AvailabilityStatusResponse, cancellationToken)
            ?? throw new InvalidOperationException($"Failed to deserialize availability status response for resource '{resourceId}'");

        return apiResponse.ToAvailabilityStatus();
    }

    public async Task<List<AvailabilityStatus>> ListAvailabilityStatusesAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
        var subscriptionId = subscriptionResource.Id.SubscriptionId;

        var managementEndpoint = _tenantService.CloudConfiguration.ArmEnvironment.Endpoint;
        var token = await GetArmAccessTokenAsync(tenant, cancellationToken);

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", token.Token);

        // Construct URL safely using Uri to ensure path is relative to base
        var relativePath = resourceGroup != null
            ? $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.ResourceHealth/availabilityStatuses?api-version={ResourceHealthApiVersion}"
            : $"/subscriptions/{subscriptionId}/providers/Microsoft.ResourceHealth/availabilityStatuses?api-version={ResourceHealthApiVersion}";
        var requestUri = new Uri(managementEndpoint, relativePath);

        using var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        await EnsureResourceHealthSuccessAsync(response, cancellationToken);

        var apiResponse = await response.Content.ReadFromJsonAsync(ResourceHealthJsonContext.Default.AvailabilityStatusListResponse, cancellationToken);

        if (apiResponse?.Value == null)
        {
            return [];
        }

        return [.. apiResponse.Value.Select(item => item.ToAvailabilityStatus())];
    }

    public async Task<List<ServiceHealthEvent>> ListServiceHealthEventsAsync(
        string subscription,
        string? eventType = null,
        string? status = null,
        string? trackingId = null,
        string? filter = null,
        string? queryStartTime = null,
        string? queryEndTime = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
        var subscriptionId = subscriptionResource.Id.SubscriptionId;

        var managementEndpoint = _tenantService.CloudConfiguration.ArmEnvironment.Endpoint;

        var token = await GetArmAccessTokenAsync(tenant, cancellationToken);

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", token.Token);

        // Build OData filter - using correct property paths for Azure Resource Health API
        var filterParts = new List<string>();

        if (!string.IsNullOrWhiteSpace(eventType))
        {
            // Use correct property path for event type
            filterParts.Add($"properties/eventType eq '{eventType}'");
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            // Use correct property path for status
            filterParts.Add($"properties/status eq '{status}'");
        }

        if (!string.IsNullOrWhiteSpace(trackingId))
        {
            // Use correct property path for tracking ID
            filterParts.Add($"properties/trackingId eq '{trackingId}'");
        }

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filterParts.Add(filter);
        }

        // Use Service Health Events API with 2025-05-01 version
        var relativePath = $"/subscriptions/{subscriptionId}/providers/Microsoft.ResourceHealth/events?api-version=2025-05-01";

        // Add time range query parameters if provided (not as OData filters)
        if (!string.IsNullOrWhiteSpace(queryStartTime))
        {
            relativePath += $"&queryStartTime={Uri.EscapeDataString(queryStartTime)}";
        }

        if (!string.IsNullOrWhiteSpace(queryEndTime))
        {
            relativePath += $"&queryEndTime={Uri.EscapeDataString(queryEndTime)}";
        }

        // Add OData filters if provided
        if (filterParts.Count > 0)
        {
            var combinedFilter = string.Join(" and ", filterParts);
            relativePath += $"&$filter={Uri.EscapeDataString(combinedFilter)}";
        }

        // Construct URL safely using Uri to ensure path is relative to base
        var requestUri = new Uri(managementEndpoint, relativePath);

        using var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        await EnsureResourceHealthSuccessAsync(response, cancellationToken);

        var apiResponse = await response.Content.ReadFromJsonAsync(ResourceHealthJsonContext.Default.ServiceHealthEventListResponse, cancellationToken);

        if (apiResponse?.Value == null)
        {
            return [];
        }

        return apiResponse.Value
            .Select(item => item.ToServiceHealthEvent(subscriptionId))
            .Where(evt => !string.IsNullOrEmpty(evt.Id)) // Filter out any invalid entries
            .ToList();
    }

    private static async Task EnsureResourceHealthSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken,
        string? resourceId = null,
        string? resourceType = null)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var (errorCode, errorMessage) = ParseErrorResponse(responseContent);

        if (response.StatusCode == HttpStatusCode.UnprocessableEntity && resourceId is not null && resourceType is not null)
        {
            throw new ResourceHealthUnprocessableEntityException(
                resourceId,
                resourceType,
                errorCode,
                errorMessage,
                responseContent);
        }

        throw new ResourceHealthRequestFailedException(
            response.StatusCode,
            errorCode,
            errorMessage,
            responseContent);
    }

    private static (string? Code, string? Message) ParseErrorResponse(string responseContent)
    {
        if (string.IsNullOrWhiteSpace(responseContent))
        {
            return (null, null);
        }

        try
        {
            using var jsonDoc = JsonDocument.Parse(responseContent);
            var root = jsonDoc.RootElement;

            if (root.TryGetProperty("error", out var errorElement) && errorElement.ValueKind == JsonValueKind.Object)
            {
                return (GetStringProperty(errorElement, "code"), GetStringProperty(errorElement, "message"));
            }

            return (GetStringProperty(root, "code"), GetStringProperty(root, "message"));
        }
        catch (JsonException)
        {
            return (null, null);
        }
    }

    private static string? GetStringProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }
}
