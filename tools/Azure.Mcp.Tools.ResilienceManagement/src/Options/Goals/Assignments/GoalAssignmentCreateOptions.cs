// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.Goals.Assignments;

public sealed class GoalAssignmentCreateOptions : IGoalAssignmentOptions
{
    [Option(Description = ResilienceManagementOptionDescriptions.ServiceGroup)]
    public required string ServiceGroup { get; set; }

    [Option(Description = ResilienceManagementOptionDescriptions.GoalAssignment)]
    public required string GoalAssignment { get; set; }

    [Option(Description = "The Goals service requires zonal resiliency for new assignments. Specify true explicitly.")]
    public required bool? RequireZonalResiliency { get; set; }

    [Option(Description = "ARM IDs of service-level indicator resources associated with the assignment. Omit for no service-level targets.")]
    public string[]? ServiceLevelResourceIds { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
