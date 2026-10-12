// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.Goals.Assignments;

public sealed class GoalAssignmentUpdateResourcesOptions
{
    [Option(Description = ResilienceManagementOptionDescriptions.ServiceGroup)]
    public required string ServiceGroup { get; set; }

    [Option(Description = "The name of the existing resilience goal assignment owning the goal resources.")]
    public required string GoalAssignment { get; set; }

    [Option(Description = "JSON array of goal resources to update. Each entry requires id (the discovered goal resource ID) and properties with resourceArmId, highAvailabilityGoalParticipation (Included or Excluded), and highAvailabilityAttestationStatus (NotAttested or ManuallyAttested). Optional disaster recovery participation, attestation, and high availability user confirmations use the service schema. This replaces per-resource properties: read existing goal resources first and include all desired disaster recovery values; omission can clear them. Exclusion reasons are system-managed and must not be supplied. Does not change service group membership or Azure resources.")]
    public required string Resources { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
