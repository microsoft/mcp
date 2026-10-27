// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.Goals.Assignments;

public sealed class GoalAssignmentCreateOptions
{
    [Option(Description = ResilienceManagementOptionDescriptions.ServiceGroup)]
    public required string ServiceGroup { get; set; }

    [Option(Description = "The name of the goal assignment to create or update.")]
    public required string GoalAssignment { get; set; }

    [Option(Description = "Whether the goal requires zonal resiliency. Explicitly set this and --require-regional-resiliency from the plan type.")]
    public required bool RequireZonalResiliency { get; set; }

    [Option(Description = "Whether the goal requires regional resiliency. Explicitly set this and --require-zonal-resiliency from the plan type.")]
    public required bool RequireRegionalResiliency { get; set; }

    [Option(Description = "ISO 8601 target recovery point objective. Required for a Regional goal.")]
    public string? RegionalRecoveryPointObjective { get; set; }

    [Option(Description = "ISO 8601 target recovery time objective. Required for a Regional goal.")]
    public string? RegionalRecoveryTimeObjective { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
