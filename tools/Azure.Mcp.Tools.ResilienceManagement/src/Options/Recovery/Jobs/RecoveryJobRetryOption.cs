// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.Recovery.Jobs;

public sealed class RecoveryJobRetryOptions
{
    [Option(Description = ResilienceManagementOptionDescriptions.ServiceGroup)]
    public required string ServiceGroup { get; set; }

    [Option(Name = "recoveryplan", Description = "The name of the recoveryplan containing the failed recovery job.")]
    public required string RecoveryPlan { get; set; }

    [Option(Description = "The name of the failed recovery job to retry.")]
    public required string RecoveryJob { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
