// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.Goals.Assignments;

public sealed class GoalAssignmentUpdateResourcesOptions : IGoalAssignmentOptions
{
    [Option(Description = ResilienceManagementOptionDescriptions.ServiceGroup)]
    public required string ServiceGroup { get; set; }

    [Option(Description = ResilienceManagementOptionDescriptions.GoalAssignment)]
    public required string GoalAssignment { get; set; }

    [Option(Description = "JSON array of GA goal resources. Each entry requires properties.resourceArmId and properties.zonalResiliency with goalParticipation (Included/Excluded), attestationStatus (NotAttested/ManuallyAttested), or userConfirmation. Manual attestation must reflect verified resilience. Maximum 1 MB.")]
    public required string Resources { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
