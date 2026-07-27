// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.UsagePlans;

public sealed class UsagePlanGetOptions : ISubscriptionOption
{
    [Option(Description = "The name of the resource group. If omitted (and no usage plan name is given), all usage plans in the subscription are listed (id and name only).")]
    public string? ResourceGroup { get; set; }

    [Option(Description = "The name of the usage plan. Provide this argument to get the details of a particular usage plan (requires a resource group); omit it to list usage plans (id and name only) for the resource group, or for the whole subscription when no resource group is given.")]
    public string? Name { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
