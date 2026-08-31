// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.Recovery.Jobs;

public sealed class RecoveryJobResumeOptions
{
    [Option(Description = ResilienceManagementOptionDescriptions.ServiceGroup)]
    public required string ServiceGroup { get; set; }

    [Option(Name = "recoveryplan", Description = "The name of the recoveryplan containing the paused recovery job.")]
    public required string RecoveryPlan { get; set; }

    [Option(Description = "The name of the paused recovery job to resume.")]
    public required string RecoveryJob { get; set; }

    [Option(Description = "Optional user-provided input for the paused recovery action, up to 100 characters.")]
    public string? Description { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

}
