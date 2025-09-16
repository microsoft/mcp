// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResourceHealth.Models;

/// <summary>
/// Represents a service health event (service issue) affecting Azure services
/// </summary>
public class ServiceHealthEvent
{
    /// <summary> The unique identifier of the service health event. </summary>
    public string? Id { get; set; }

    /// <summary> The name of the service health event. </summary>
    public string? Name { get; set; }

    /// <summary> The title of the service health event. </summary>
    public string? Title { get; set; }

    /// <summary> The summary description of the service health event. </summary>
    public string? Summary { get; set; }

    /// <summary> The detailed description of the service health event. </summary>
    public string? Details { get; set; }

    /// <summary> The event type of the service health event (ServiceIssue, PlannedMaintenance, HealthAdvisory, Security). </summary>
    public string? EventType { get; set; }

    /// <summary> The status of the service health event (Active, Resolved, etc.). </summary>
    public string? Status { get; set; }

    /// <summary> The level of impact of the service health event (Information, Warning, Error). </summary>
    public string? Level { get; set; }

    /// <summary> The tracking ID for this service health event. </summary>
    public string? TrackingId { get; set; }

    /// <summary> The Azure services affected by this service health event. </summary>
    public List<string>? AffectedServices { get; set; }

    /// <summary> The Azure regions affected by this service health event. </summary>
    public List<string>? AffectedRegions { get; set; }

    /// <summary> The subscription IDs affected by this service health event. </summary>
    public List<string>? AffectedSubscriptions { get; set; }

    /// <summary> The start time of the service health event. </summary>
    public DateTimeOffset? StartTime { get; set; }

    /// <summary> The end time of the service health event. </summary>
    public DateTimeOffset? EndTime { get; set; }

    /// <summary> The time when the service health event was last updated. </summary>
    public DateTimeOffset? LastModified { get; set; }

    /// <summary> The communication type (email, portal, etc.) for this event. </summary>
    public string? Communication { get; set; }

    /// <summary> The category of the service health event. </summary>
    public string? Category { get; set; }

    /// <summary> The location where this service health event was reported. </summary>
    public string? Location { get; set; }
}
