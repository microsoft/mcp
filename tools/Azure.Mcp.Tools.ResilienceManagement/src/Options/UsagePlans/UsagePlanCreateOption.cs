// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.UsagePlans;

public class UsagePlanCreateOptions : ISubscriptionOption
{
    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = "The name of the usage plan.")]
    public required string UsagePlan { get; set; }

    [Option(Description = "The plan type of the usage plan. Supported values: Basic, Standard.")]
    public required UsagePlanKind PlanType { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
