// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.Recovery.Plans.Resources;

public class RecoveryResourceGetOptions : ISubscriptionOption
{
    [Option(Description = "The name of the service group.")]
    public required string ServiceGroup { get; set; }

    [Option(Description = "The name of the recovery plan.")]
    public required string RecoveryPlan { get; set; }

    [Option(Description = "The name of the recovery resource (member). Provide this argument to get the details of a particular recovery resource; omit it to list all resources (members) of the recovery plan (id and name only).")]
    public string? Name { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
