// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResourceHealth.Options.ServiceHealthEvents;

public class ServiceHealthEventsListOptions : BaseResourceHealthOptions
{
    /// <summary> Filter by event type (ServiceIssue, PlannedMaintenance, HealthAdvisory, Security). </summary>
    public string? EventType { get; set; }

    /// <summary> Filter by status (Active, Resolved). </summary>
    public string? Status { get; set; }

    /// <summary> Filter by tracking ID to get a specific service health event. </summary>
    public string? TrackingId { get; set; }

    /// <summary> Additional OData filter expression to apply to the service health events query. </summary>
    public string? Filter { get; set; }

    /// <summary> Start time for the query in ISO 8601 format (e.g., 2024-01-01T00:00:00Z). </summary>
    public string? QueryStartTime { get; set; }

    /// <summary> End time for the query in ISO 8601 format (e.g., 2024-01-31T23:59:59Z). </summary>
    public string? QueryEndTime { get; set; }
}
