// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Http;
using Azure.Mcp.Tools.ResourceHealth.Models;
using Azure.Mcp.Tools.ResourceHealth.Models.Internal;

namespace Azure.Mcp.Tools.ResourceHealth.Services;

public class ResourceHealthService(ISubscriptionService subscriptionService, ITenantService tenantService, IHttpClientService httpClientService)
    : BaseAzureService(tenantService), IResourceHealthService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly IHttpClientService _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));

    private const string AzureManagementBaseUrl = "https://management.azure.com";
    private const string ResourceHealthApiVersion = "2025-05-01";

    public async Task<AvailabilityStatus> GetAvailabilityStatusAsync(
        string resourceId,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters((nameof(resourceId), resourceId));

        // Parse and validate resource ID format using Azure SDK
        var parsedResourceId = ResourceIdentifier.Parse(resourceId);

        try
        {
            var credential = await GetCredential();
            var token = await credential.GetTokenAsync(
                new TokenRequestContext([$"{AzureManagementBaseUrl}/.default"]),
                CancellationToken.None);

            using var client = _httpClientService.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Token);

            // Construct URL safely using Uri to ensure path is relative to base
            var baseUri = new Uri(AzureManagementBaseUrl);
            var relativePath = $"{parsedResourceId}/providers/Microsoft.ResourceHealth/availabilityStatuses/current?api-version={ResourceHealthApiVersion}";
            var requestUri = new Uri(baseUri, relativePath);

            using var response = await client.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize(content, ResourceHealthJsonContext.Default.AvailabilityStatusResponse);

            if (apiResponse == null)
            {
                throw new InvalidOperationException($"Failed to deserialize availability status response for resource '{resourceId}'");
            }

            return apiResponse.ToAvailabilityStatus();
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Failed to get availability status for resource '{resourceId}': {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new Exception($"Failed to parse availability status response for resource '{resourceId}': {ex.Message}", ex);
        }
    }

    public async Task<List<AvailabilityStatus>> ListAvailabilityStatusesAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
            var subscriptionId = subscriptionResource.Id.SubscriptionId;

            var credential = await GetCredential();
            var token = await credential.GetTokenAsync(
                new TokenRequestContext([$"{AzureManagementBaseUrl}/.default"]),
                CancellationToken.None);

            using var client = _httpClientService.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Token);

            // Construct URL safely using Uri to ensure path is relative to base
            var baseUri = new Uri(AzureManagementBaseUrl);
            var relativePath = resourceGroup != null
                ? $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.ResourceHealth/availabilityStatuses?api-version={ResourceHealthApiVersion}"
                : $"/subscriptions/{subscriptionId}/providers/Microsoft.ResourceHealth/availabilityStatuses?api-version={ResourceHealthApiVersion}";
            var requestUri = new Uri(baseUri, relativePath);

            using var response = await client.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize(content, ResourceHealthJsonContext.Default.AvailabilityStatusListResponse);

            if (apiResponse?.Value == null)
            {
                return new List<AvailabilityStatus>();
            }

            return apiResponse.Value.Select(item => item.ToAvailabilityStatus()).ToList();
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Failed to list availability statuses for subscription '{subscription}'{(resourceGroup != null ? $" and resource group '{resourceGroup}'" : "")}: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new Exception($"Failed to parse availability statuses response: {ex.Message}", ex);
        }
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
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
            var subscriptionId = subscriptionResource.Id.SubscriptionId;

            var credential = await GetCredential();
            var token = await credential.GetTokenAsync(
                new TokenRequestContext([$"{AzureManagementBaseUrl}/.default"]),
                CancellationToken.None);

            using var client = _httpClientService.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Token);

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
            var baseUri = new Uri(AzureManagementBaseUrl);
            var requestUri = new Uri(baseUri, relativePath);

            using var response = await client.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            var apiResponse = JsonSerializer.Deserialize(content, ResourceHealthJsonContext.Default.ServiceHealthEventListResponse);

            if (apiResponse?.Value == null)
            {
                return new List<ServiceHealthEvent>();
            }

            return apiResponse.Value
                .Select(item => item.ToServiceHealthEvent(subscriptionId))
                .Where(evt => !string.IsNullOrEmpty(evt.Id)) // Filter out any invalid entries
                .ToList();

        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Failed to list service health events for subscription '{subscription}': {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new Exception($"Failed to parse service health events response: {ex.Message}", ex);
        }
    }
}
