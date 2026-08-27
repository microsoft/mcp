// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.Recovery.Plans;

public sealed class RecoveryPlanReprotectOptions
{
    [Option(Description = ResilienceManagementOptionDescriptions.ServiceGroup)]
    public required string ServiceGroup { get; set; }

    [Option(Description = "The name of the recovery plan to reprotect.")]
    public required string RecoveryPlan { get; set; }

    [Option(Description = "Customer-provided full recovery-resource IDs to reprotect after failover. Select the resources explicitly; do not infer IDs from prior context or resource metadata.")]
    public required string[] SelectedResourceIds { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}