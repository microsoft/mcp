// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.Drills.Runs;

public sealed class DrillRunMarkCompleteOptions
{
    [Option(Description = ResilienceManagementOptionDescriptions.ServiceGroup)]
    public required string ServiceGroup { get; set; }

    [Option(Description = ResilienceManagementOptionDescriptions.Drill)]
    public required string Drill { get; set; }

    [Option(Description = "The name of the drill run whose stage will be marked complete.")]
    public required string DrillRun { get; set; }

    [Option(Description =
        "The drill run stage to mark complete, disabling further retries on it. " +
        "Allowed values: FaultInjection, Failover, Reprotect, FailoverReverse, ReprotectReverse.")]
    public required string Stage { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
