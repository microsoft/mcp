// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResourceHealth;

public static class ResourceHealthOptionDefinitions
{
    public const string ResourceIdName = "resourceId";
    public const string EventTypeName = "event-type";
    public const string StatusName = "status";
    public const string TrackingIdName = "tracking-id";
    public const string FilterName = "filter";
    public const string QueryStartTimeName = "query-start-time";
    public const string QueryEndTimeName = "query-end-time";

    public static readonly Option<string> ResourceId = new(
        $"--{ResourceIdName}"
    )
    {
        Description = "The Azure resource ID to get health status for (e.g., /subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.Compute/virtualMachines/{vm}).",
        Required = true
    };

    public static readonly Option<string> EventType = new(
        $"--{EventTypeName}"
    )
    {
        Description = "Filter by event type (ServiceIssue, PlannedMaintenance, HealthAdvisory, Security). If not specified, all event types are included."
    };

    public static readonly Option<string> Status = new(
        $"--{StatusName}"
    )
    {
        Description = "Filter by status (Active, Resolved). If not specified, all statuses are included."
    };

    public static readonly Option<string> TrackingId = new(
        $"--{TrackingIdName}"
    )
    {
        Description = "Filter by tracking ID to get a specific service health event."
    };

    public static readonly Option<string> Filter = new(
        $"--{FilterName}"
    )
    {
        Description = "Additional OData filter expression to apply to the service health events query."
    };

    public static readonly Option<string> QueryStartTime = new(
        $"--{QueryStartTimeName}"
    )
    {
        Description = "Start time for the query in ISO 8601 format (e.g., 2024-01-01T00:00:00Z). Events from this time onwards will be included."
    };

    public static readonly Option<string> QueryEndTime = new(
        $"--{QueryEndTimeName}"
    )
    {
        Description = "End time for the query in ISO 8601 format (e.g., 2024-01-31T23:59:59Z). Events up to this time will be included."
    };
}
