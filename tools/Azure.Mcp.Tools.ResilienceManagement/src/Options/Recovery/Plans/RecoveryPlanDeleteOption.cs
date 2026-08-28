// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.Recovery.Plans;

public sealed class RecoveryPlanDeleteOptions
{
    [Option(Description = ResilienceManagementOptionDescriptions.ServiceGroup)]
    public required string ServiceGroup { get; set; }

    [Option(Description = "The name of the recovery plan to delete.")]
    public required string RecoveryPlan { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

}
