// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.Recovery.Plans;

public sealed class RecoveryPlanValidateForReprotectOptions
{
    [Option(Description = ResilienceManagementOptionDescriptions.ServiceGroup)]
    public required string ServiceGroup { get; set; }

    [Option(Description = "The name of the recovery plan to validate for reprotect.")]
    public required string RecoveryPlan { get; set; }

    [Option(Description = "Optional customer-provided full recovery-resource IDs to validate. Omit this option to validate all qualified resources in the recovery plan. Do not infer resource IDs from prior context or resource metadata.")]
    public string[]? SelectedResourceIds { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
