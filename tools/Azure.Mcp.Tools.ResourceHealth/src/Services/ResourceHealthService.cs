// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.ResourceHealth.Models;
using Azure.ResourceManager.ResourceHealth;

namespace Azure.Mcp.Tools.ResourceHealth.Services;

public class ResourceHealthService(ISubscriptionService subscriptionService, ITenantService tenantService)
    : BaseAzureService(tenantService), IResourceHealthService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));

    public async Task<AvailabilityStatus> GetAvailabilityStatusAsync(
        string resourceId,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(resourceId);

        try
        {
            var armClient = await CreateArmClientAsync(null, retryPolicy);

            // Create ResourceIdentifier from the resource ID string
            var resourceIdentifier = new ResourceIdentifier(resourceId);

            // Call the Azure ResourceHealth API to get current availability status
            var response = await armClient.GetAvailabilityStatusAsync(
                resourceIdentifier,
                cancellationToken: default);

            var availabilityStatus = response.Value;
            var properties = availabilityStatus.Properties;

            // Map Azure SDK response to our model
            return new AvailabilityStatus
            {
                ResourceId = resourceId,
                AvailabilityState = properties.AvailabilityState?.ToString(),
                Summary = properties.Summary,
                DetailedStatus = properties.DetailedStatus,
                ReasonType = properties.ReasonType,
                OccurredTime = properties.OccuredOn, // Note: "OccuredOn" property name is misspelled in Azure SDK
                ReportedTime = properties.ReportedOn,
                CauseType = properties.HealthEventCause,
                RootCauseAttributionTime = properties.RootCauseAttributionOn?.ToString("O"),
                Category = properties.HealthEventCategory,
                Title = properties.Title,
                Location = availabilityStatus.Location?.Name ?? "Unknown"
            };
        }
        catch (RequestFailedException ex)
        {
            throw new Exception($"Failed to get availability status for resource '{resourceId}': {ex.Message}", ex);
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

            // Get all availability statuses from the subscription
            var availabilityStatuses = new List<AvailabilityStatus>();

            foreach (var status in subscriptionResource.GetAvailabilityStatusesBySubscription())
            {
                var properties = status.Properties;

                // If resource group filter is specified, check if the resource belongs to that group
                if (!string.IsNullOrWhiteSpace(resourceGroup))
                {
                    var resourceId = status.Id?.ToString(); // Convert ResourceIdentifier to string
                    if (!string.IsNullOrEmpty(resourceId) && !resourceId.Contains($"/resourceGroups/{resourceGroup}/", StringComparison.OrdinalIgnoreCase))
                    {
                        continue; // Skip resources not in the specified resource group
                    }
                }

                availabilityStatuses.Add(new AvailabilityStatus
                {
                    ResourceId = status.Id?.ToString() ?? status.Name, // Use Id first, fallback to Name
                    AvailabilityState = properties.AvailabilityState?.ToString(),
                    Summary = properties.Summary,
                    DetailedStatus = properties.DetailedStatus,
                    ReasonType = properties.ReasonType,
                    OccurredTime = properties.OccuredOn, // Note: "OccuredOn" property name is misspelled in Azure SDK
                    ReportedTime = properties.ReportedOn,
                    CauseType = properties.HealthEventCause,
                    RootCauseAttributionTime = properties.RootCauseAttributionOn?.ToString("O"),
                    Category = properties.HealthEventCategory,
                    Title = properties.Title,
                    Location = status.Location?.Name ?? "Unknown"
                });
            }

            return availabilityStatuses;
        }
        catch (RequestFailedException ex)
        {
            throw new Exception($"Failed to list availability statuses for subscription '{subscription}'{(resourceGroup != null ? $" and resource group '{resourceGroup}'" : "")}: {ex.Message}", ex);
        }
    }
}
