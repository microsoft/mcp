// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Policy.Options.Assignment;

public sealed class PolicyAssignmentListOptions : ISubscriptionOption
{
    [Option(Description = "The scope of the policy assignment (e.g., /subscriptions/{subscriptionId}, /subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}).")]
    public string? Scope { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
