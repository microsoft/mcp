// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.EventGrid.Options.Subscription;

public sealed class SubscriptionListOptions
{
    [Option(Description = EventGridOptionDescriptions.Topic)]
    public string? Topic { get; set; }

    [Option(Description = "The Azure region to filter resources by (e.g., 'eastus', 'westus2').")]
    public string? Location { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
