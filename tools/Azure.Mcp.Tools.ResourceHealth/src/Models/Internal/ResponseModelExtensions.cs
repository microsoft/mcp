// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResourceHealth.Models;

namespace Azure.Mcp.Tools.ResourceHealth.Models.Internal;

/// <summary>
/// Extension methods for converting internal API response models to public models
/// </summary>
internal static class ResponseModelExtensions
{
    /// <summary>
    /// Converts an internal AvailabilityStatusResponse to public AvailabilityStatus model
    /// </summary>
    public static AvailabilityStatus ToAvailabilityStatus(this AvailabilityStatusResponse response)
    {
        return new AvailabilityStatus
        {
            ResourceId = response.Id,
            AvailabilityState = response.Properties?.AvailabilityState,
            Summary = response.Properties?.Summary,
            DetailedStatus = response.Properties?.DetailedStatus,
            ReasonType = response.Properties?.ReasonType,
            OccurredTime = response.Properties?.OccurredTime,
            ReportedTime = response.Properties?.ReportedTime,
            CauseType = response.Properties?.ReasonChronicity,
            RootCauseAttributionTime = response.Properties?.RootCauseAttributionTime,
            Category = response.Properties?.HealthEventCategory,
            Title = response.Properties?.Title,
            Location = response.Location ?? "Unknown"
        };
    }

    /// <summary>
    /// Converts an internal ServiceHealthEventResponse to public ServiceHealthEvent model
    /// </summary>
    public static ServiceHealthEvent ToServiceHealthEvent(this ServiceHealthEventResponse response, string? subscriptionId = null)
    {
        var affectedServices = new List<string>();
        var affectedRegions = new List<string>();

        if (response.Properties?.Impact != null)
        {
            foreach (var impact in response.Properties.Impact)
            {
                // Add affected service
                if (!string.IsNullOrEmpty(impact.ImpactedService) && !affectedServices.Contains(impact.ImpactedService))
                {
                    affectedServices.Add(impact.ImpactedService);
                }

                // Add affected regions
                if (impact.ImpactedRegions != null)
                {
                    foreach (var regionInfo in impact.ImpactedRegions)
                    {
                        if (!string.IsNullOrEmpty(regionInfo.ImpactedRegion) && !affectedRegions.Contains(regionInfo.ImpactedRegion))
                        {
                            affectedRegions.Add(regionInfo.ImpactedRegion);
                        }
                    }
                }
            }
        }

        return new ServiceHealthEvent
        {
            Id = response.Id,
            Name = response.Name,
            Title = response.Properties?.Title,
            Summary = response.Properties?.Summary,
            Details = response.Properties?.Description,
            EventType = response.Properties?.EventType,
            Status = response.Properties?.Status,
            Level = response.Properties?.EventLevel,
            TrackingId = response.Properties?.TrackingId,
            AffectedServices = affectedServices.Count > 0 ? affectedServices : null,
            AffectedRegions = affectedRegions.Count > 0 ? affectedRegions : null,
            AffectedSubscriptions = !string.IsNullOrEmpty(subscriptionId) ? new List<string> { subscriptionId } : null,
            StartTime = response.Properties?.ImpactStartTime,
            EndTime = response.Properties?.ImpactMitigationTime,
            LastModified = response.Properties?.LastUpdateTime,
            Communication = response.Properties?.CommunicationId
        };
    }
}
