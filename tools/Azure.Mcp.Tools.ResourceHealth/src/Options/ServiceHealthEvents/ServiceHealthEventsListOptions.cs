// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResourceHealth.Options.ServiceHealthEvents;

public sealed class ServiceHealthEventsListOptions : ISubscriptionOption
{
    /// <summary> Filter by event type (ServiceIssue, PlannedMaintenance, HealthAdvisory, Security). </summary>
    [Option(Description = "Filter by event type (ServiceIssue, PlannedMaintenance, HealthAdvisory, Security). If not specified, all event types are included.")]
    public string? EventType { get; set; }

    /// <summary> Filter by status (Active, Resolved). </summary>
    [Option(Description = "Filter by status (Active, Resolved). If not specified, all statuses are included.")]
    public string? Status { get; set; }

    /// <summary> Filter by tracking ID to get a specific service health event. </summary>
    [Option(Description = "Filter by tracking ID to get a specific service health event.")]
    public string? TrackingId { get; set; }

    /// <summary> Additional OData filter expression to apply to the service health events query. </summary>
    [Option(Description = "Additional OData filter expression to apply to the service health events query.")]
    public string? Filter { get; set; }

    /// <summary> Start time for the query in ISO 8601 format (e.g., 2024-01-01T00:00:00Z). </summary>
    [Option(Description = "Start time for the query in ISO 8601 format (e.g., 2024-01-01T00:00:00Z). Events from this time onwards will be included.")]
    public string? QueryStartTime { get; set; }

    /// <summary> End time for the query in ISO 8601 format (e.g., 2024-01-31T23:59:59Z). </summary>
    [Option(Description = "End time for the query in ISO 8601 format (e.g., 2024-01-31T23:59:59Z). Events up to this time will be included.")]
    public string? QueryEndTime { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
