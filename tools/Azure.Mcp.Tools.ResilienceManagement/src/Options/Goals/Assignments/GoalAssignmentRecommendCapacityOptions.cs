// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.Goals.Assignments;

public sealed class GoalAssignmentRecommendCapacityOptions
{
    [Option(Description = ResilienceManagementOptionDescriptions.ServiceGroup)]
    public required string ServiceGroup { get; set; }

    [Option(Description = "The name of the existing resilience goal assignment.")]
    public required string GoalAssignment { get; set; }

    [Option(Description = "Azure resource ARM IDs to assess for zonal capacity recommendations. Omit to assess all eligible resources in the service group. These are underlying Azure resource IDs, not goal resource IDs.")]
    public string[]? ResourceIds { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
