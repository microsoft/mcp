// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResourceHealth.Models;

public class ResourceEvent
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? ResourceId { get; set; }
    public string? AvailabilityState { get; set; }
    public string? PreviousAvailabilityState { get; set; }
    public string? Summary { get; set; }
    public string? DetailedStatus { get; set; }
    public string? ReasonType { get; set; }
    public string? ReasonChronicity { get; set; }
    public DateTimeOffset? EventTimestamp { get; set; }
    public DateTimeOffset? OccurredTime { get; set; }
    public DateTimeOffset? ReportedTime { get; set; }
    public string? RootCauseAttributionTime { get; set; }
    public string? Category { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? ResolutionEta { get; set; }
    public string? Level { get; set; }
    public string? Location { get; set; }
}
