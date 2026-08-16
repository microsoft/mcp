// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.Recovery.Plans;

public sealed class RecoveryPlanUpdateResourcesOptions
{
    [Option(Description = ResilienceManagementOptionDescriptions.ServiceGroup)]
    public required string ServiceGroup { get; set; }

    [Option(Description = "The name of the recovery plan whose resources will be updated.")]
    public required string RecoveryPlan { get; set; }

    [Option(Description =
        "A JSON array of recovery resources to include, exclude, or configure. " +
        "Each item must contain properties.recoveryResourceUniqueId. " +
        "First inclusion and re-inclusion require matching selectedProtectionSolutionType and selectedProtectionSolutionSetting. " +
        "CustomRunbook requires failoverAction.resourceId and reprotectAction.resourceId values that identify Microsoft.Automation/automationAccounts/runbooks resources. " +
        "AzureSiteRecovery is supported for Microsoft.Compute/virtualMachines resources and requires Microsoft.Compute/disks and Microsoft.Storage/storageAccounts IDs in diskReprotectInputDetails; " +
        "AzureSiteRecovery also requires testFailoverParams.networkResourceId identifying a Microsoft.Network/virtualNetworks resource. " +
        "The service preserves existing settings on sparse updates and permits only inclusionState changes while the resource is excluded. " +
        "recoveryGroupId and associatedIdentity are optional.")]
    public string? ResourcesToUpdate { get; set; }

    [Option(Description = "A JSON array of full recovery-resource IDs to remove from the recovery plan.")]
    public string? ResourcesToRemove { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
