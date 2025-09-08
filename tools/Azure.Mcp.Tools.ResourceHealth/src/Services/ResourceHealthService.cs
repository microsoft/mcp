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
        ValidateRequiredParameters(resourceId);

        try
        {
            var credential = await GetCredential();
            var token = await credential.GetTokenAsync(
                new TokenRequestContext(new[] { $"{AzureManagementBaseUrl}/.default" }),
                CancellationToken.None);

            using var client = _httpClientService.CreateClient(new Uri(AzureManagementBaseUrl));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Token);

            var url = $"{resourceId}/providers/Microsoft.ResourceHealth/availabilityStatuses/current?api-version={ResourceHealthApiVersion}";
            var response = await client.GetAsync(url);

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(content);
            var root = jsonDocument.RootElement;

            var properties = root.GetProperty("properties");

            // Map REST API response to our model
            return new AvailabilityStatus
            {
                ResourceId = resourceId,
                AvailabilityState = GetOptionalStringProperty(properties, "availabilityState"),
                Summary = GetOptionalStringProperty(properties, "summary"),
                DetailedStatus = GetOptionalStringProperty(properties, "detailedStatus"),
                ReasonType = GetOptionalStringProperty(properties, "reasonType"),
                OccurredTime = GetOptionalDateTimeProperty(properties, "occuredTime"),
                ReportedTime = GetOptionalDateTimeProperty(properties, "reportedTime"),
                CauseType = GetOptionalStringProperty(properties, "reasonChronicity"),
                RootCauseAttributionTime = GetOptionalStringProperty(properties, "rootCauseAttributionTime"),
                Category = GetOptionalStringProperty(properties, "healthEventCategory"),
                Title = GetOptionalStringProperty(properties, "title"),
                Location = GetOptionalStringProperty(root, "location") ?? "Unknown"
            };
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
        ValidateRequiredParameters(subscription);

        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
            var subscriptionId = subscriptionResource.Id.SubscriptionId;

            var credential = await GetCredential();
            var token = await credential.GetTokenAsync(
                new TokenRequestContext(new[] { $"{AzureManagementBaseUrl}/.default" }),
                CancellationToken.None);

            using var client = _httpClientService.CreateClient(new Uri(AzureManagementBaseUrl));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Token);

            var url = resourceGroup != null
                ? $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.ResourceHealth/availabilityStatuses?api-version={ResourceHealthApiVersion}"
                : $"/subscriptions/{subscriptionId}/providers/Microsoft.ResourceHealth/availabilityStatuses?api-version={ResourceHealthApiVersion}";

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(content);
            var root = jsonDocument.RootElement;

            var availabilityStatuses = new List<AvailabilityStatus>();

            if (root.TryGetProperty("value", out var valuesArray))
            {
                foreach (var item in valuesArray.EnumerateArray())
                {
                    var properties = item.GetProperty("properties");

                    availabilityStatuses.Add(new AvailabilityStatus
                    {
                        ResourceId = GetOptionalStringProperty(item, "id"),
                        AvailabilityState = GetOptionalStringProperty(properties, "availabilityState"),
                        Summary = GetOptionalStringProperty(properties, "summary"),
                        DetailedStatus = GetOptionalStringProperty(properties, "detailedStatus"),
                        ReasonType = GetOptionalStringProperty(properties, "reasonType"),
                        OccurredTime = GetOptionalDateTimeProperty(properties, "occuredTime"),
                        ReportedTime = GetOptionalDateTimeProperty(properties, "reportedTime"),
                        CauseType = GetOptionalStringProperty(properties, "reasonChronicity"),
                        RootCauseAttributionTime = GetOptionalStringProperty(properties, "rootCauseAttributionTime"),
                        Category = GetOptionalStringProperty(properties, "healthEventCategory"),
                        Title = GetOptionalStringProperty(properties, "title"),
                        Location = GetOptionalStringProperty(item, "location") ?? "Unknown"
                    });
                }
            }

            return availabilityStatuses;
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
        ValidateRequiredParameters(subscription);

        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
            var subscriptionId = subscriptionResource.Id.SubscriptionId;

            var credential = await GetCredential();
            var token = await credential.GetTokenAsync(
                new TokenRequestContext(new[] { $"{AzureManagementBaseUrl}/.default" }),
                CancellationToken.None);

            using var client = _httpClientService.CreateClient(new Uri(AzureManagementBaseUrl));
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
            var url = $"/subscriptions/{subscriptionId}/providers/Microsoft.ResourceHealth/events?api-version=2025-05-01";
            
            // Add time range query parameters if provided (not as OData filters)
            if (!string.IsNullOrWhiteSpace(queryStartTime))
            {
                url += $"&queryStartTime={Uri.EscapeDataString(queryStartTime)}";
            }
            
            if (!string.IsNullOrWhiteSpace(queryEndTime))
            {
                url += $"&queryEndTime={Uri.EscapeDataString(queryEndTime)}";
            }
            
            // Add OData filters if provided
            if (filterParts.Count > 0)
            {
                var combinedFilter = string.Join(" and ", filterParts);
                url += $"&$filter={Uri.EscapeDataString(combinedFilter)}";
            }

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            
            var jsonDocument = JsonDocument.Parse(content);
            var root = jsonDocument.RootElement;

            var serviceHealthEvents = new List<ServiceHealthEvent>();

            if (root.TryGetProperty("value", out var valuesArray))
            {
                foreach (var item in valuesArray.EnumerateArray())
                {
                    // Service Health Events API structure (2025-05-01)
                    var eventId = GetOptionalStringProperty(item, "id");
                    var eventName = GetOptionalStringProperty(item, "name");
                    
                    if (!item.TryGetProperty("properties", out var properties))
                        continue;
                    
                    // Get service health event details from properties
                    var eventTitle = GetOptionalStringProperty(properties, "title");
                    var eventSummary = GetOptionalStringProperty(properties, "summary");
                    var eventTypeValue = GetOptionalStringProperty(properties, "eventType");
                    var eventStatus = GetOptionalStringProperty(properties, "status");
                    var eventLevel = GetOptionalStringProperty(properties, "eventLevel");
                    var eventTrackingId = GetOptionalStringProperty(properties, "trackingId");
                    
                    // Get timestamps from Service Health Events API
                    DateTime? eventStartTime = GetOptionalDateTimeProperty(properties, "impactStartTime")?.DateTime;
                    DateTime? eventEndTime = GetOptionalDateTimeProperty(properties, "impactMitigationTime")?.DateTime;
                    DateTime? lastModified = GetOptionalDateTimeProperty(properties, "lastUpdateTime")?.DateTime;

                    // Extract affected services and regions from impact array
                    var affectedServices = new List<string>();
                    var affectedRegions = new List<string>();
                    var affectedSubscriptions = new List<string> { subscription };

                    if (properties.TryGetProperty("impact", out var impactArray))
                    {
                        foreach (var impactItem in impactArray.EnumerateArray())
                        {
                            // Extract service name from impact
                            var serviceName = GetOptionalStringProperty(impactItem, "impactedService");
                            if (!string.IsNullOrEmpty(serviceName) && !affectedServices.Contains(serviceName))
                            {
                                affectedServices.Add(serviceName);
                            }

                            // Extract regions from impactedRegions array
                            if (impactItem.TryGetProperty("impactedRegions", out var regionsArray))
                            {
                                foreach (var regionItem in regionsArray.EnumerateArray())
                                {
                                    var regionName = GetOptionalStringProperty(regionItem, "impactedRegion");
                                    if (!string.IsNullOrEmpty(regionName) && !affectedRegions.Contains(regionName))
                                    {
                                        affectedRegions.Add(regionName);
                                    }
                                }
                            }
                        }
                    }

                    serviceHealthEvents.Add(new ServiceHealthEvent
                    {
                        Id = eventId,
                        Name = eventName,
                        Title = eventTitle,
                        Summary = eventSummary,
                        Details = GetOptionalStringProperty(properties, "description"),
                        EventType = eventTypeValue,
                        Status = eventStatus,
                        Level = eventLevel,
                        TrackingId = eventTrackingId,
                        AffectedServices = affectedServices.Count > 0 ? affectedServices : null,
                        AffectedRegions = affectedRegions.Count > 0 ? affectedRegions : null,
                        AffectedSubscriptions = affectedSubscriptions,
                        StartTime = eventStartTime,
                        EndTime = eventEndTime,
                        LastModified = lastModified,
                        Communication = GetOptionalStringProperty(properties, "communicationId"),
                        Category = GetOptionalStringProperty(properties, "eventSource"),
                        Location = affectedRegions.Count > 0 ? affectedRegions.First() : "Global"
                    });
                }
            }

            return serviceHealthEvents;
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

    private static string? GetOptionalStringProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) ? property.GetString() : null;
    }

    private static DateTimeOffset? GetOptionalDateTimeProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property) && property.ValueKind != JsonValueKind.Null)
        {
            var dateString = property.GetString();
            if (!string.IsNullOrEmpty(dateString) && DateTimeOffset.TryParse(dateString, out var date))
            {
                return date;
            }
        }
        return null;
    }
}
