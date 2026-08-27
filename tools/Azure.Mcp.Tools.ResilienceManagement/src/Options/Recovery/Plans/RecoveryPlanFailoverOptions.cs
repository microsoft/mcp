// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.Recovery.Plans;

public sealed class RecoveryPlanFailoverOptions
{
    [Option(Description = ResilienceManagementOptionDescriptions.ServiceGroup)]
    public required string ServiceGroup { get; set; }

    [Option(Description = "The name of the recovery plan to fail over.")]
    public required string RecoveryPlan { get; set; }

    [Option(Description = "Customer-provided Azure locations from which resources will fail over, such as eastus or westus2-az3. Provide source locations, selected resource IDs, or both. Do not infer omitted locations; ask the customer.")]
    public string[]? SourceLocations { get; set; }

    [Option(Description = "Customer-provided full recovery-resource IDs to fail over. Provide selected resource IDs, source locations, or both. Do not infer omitted IDs; ask the customer.")]
    public string[]? SelectedResourceIds { get; set; }

    [Option(Description = "Optional execution consent. Allowed values are Unspecified and Allowed.")]
    public string? UserConsent { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
