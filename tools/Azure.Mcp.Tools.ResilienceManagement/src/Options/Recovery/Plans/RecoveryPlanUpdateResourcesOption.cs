// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.Recovery.Plans;

public sealed class RecoveryPlanUpdateResourcesOptions
{
    [Option(Description = "The name of the Azure service group that owns the recovery plan.")]
    public required string ServiceGroup { get; set; }

    [Option(Description = "The name of the recovery plan whose resources will be updated.")]
    public required string RecoveryPlan { get; set; }

    [Option(Description = "A JSON array of recovery resources to include, exclude, or configure. Each item must contain a properties object with recoveryResourceUniqueId. The read-only id may be omitted; when supplied, it must match the unique ID and selected recovery plan. Supported caller-controlled properties include inclusionState, selectedProtectionSolutionType, selectedProtectionSolutionSetting, recoveryGroupId, and associatedIdentity.")]
    public string? ResourcesToUpdate { get; set; }

    [Option(Description = "A JSON array of full recovery-resource IDs to remove from the recovery plan.")]
    public string? ResourcesToRemove { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
