// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.Drills.Runs.Resources;

public class DrillRunResourceGetOptions : ISubscriptionOption
{
    [Option(Description = "The name of the service group.")]
    public required string ServiceGroup { get; set; }

    [Option(Description = "The name of the drill.")]
    public required string Drill { get; set; }

    [Option(Description = "The name of the drill run.")]
    public required string DrillRun { get; set; }

    [Option(Description = "The name of the drill run resource (target). Provide this argument to get the details of a particular drill run resource; omit it to list all resources (targets) of the drill run (id and name only).")]
    public string? Name { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
