// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureIsv.Options.Datadog;

public sealed class MonitoredResourcesListOptions : ISubscriptionOption
{
    [Option(Description = "The name of the Datadog resource to use. This is the unique name you chose for your Datadog resource in Azure.")]
    public required string DatadogResource { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }
}
