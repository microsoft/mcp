// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResourceHealth.Models;

public class AvailabilityStatus
{
    /// <summary> Azure resource ID of the resource this availability status is for. </summary>
    public string? ResourceId { get; set; }

    /// <summary> Availability status of the resource (Available, Unavailable, Degraded, Unknown). </summary>
    public string? AvailabilityState { get; set; }

    /// <summary> Human-readable summary of the resource's current availability. </summary>
    public string? Summary { get; set; }

    /// <summary> Detailed explanation of the resource's current availability. </summary>
    public string? DetailedStatus { get; set; }

    /// <summary> Category of availability issue (Platform, Customer, Unknown). </summary>
    public string? ReasonType { get; set; }

    /// <summary> The latest time this availability status was updated. </summary>
    public DateTimeOffset? OccurredTime { get; set; }

    /// <summary> The time when the availability status was reported. </summary>
    public DateTimeOffset? ReportedTime { get; set; }

    /// <summary> Type of cause for the current availability (Health, Configuration, Performance, Unknown). </summary>
    public string? CauseType { get; set; }

    /// <summary> Root cause of current availability status. </summary>
    public string? RootCauseAttributionTime { get; set; }

    /// <summary> Category of the resource health event. </summary>
    public string? Category { get; set; }

    /// <summary> Title of the availability status. </summary>
    public string? Title { get; set; }

    /// <summary> Location where the availability status was reported from. </summary>
    public string? Location { get; set; }

    /// <summary> Timestamp when the resource availability was first discovered. </summary>
    public DateTimeOffset? FirstDiscoveredTime { get; set; }

    /// <summary> Timestamp when the resource availability was last discovered. </summary>
    public DateTimeOffset? LastDiscoveredTime { get; set; }
}
