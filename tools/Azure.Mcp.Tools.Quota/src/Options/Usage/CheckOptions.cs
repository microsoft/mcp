// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Quota.Options.Usage;

public sealed class CheckOptions : ISubscriptionOption
{
    [Option(Description = "The Azure region where the resources will be deployed. E.g. 'eastus', 'westus', etc.")]
    public required string Region { get; set; }

    [Option(Description = "Comma-separated list of Azure resource types that are going to be deployed. E.g. 'Microsoft.App/containerApps,Microsoft.Web/sites,Microsoft.CognitiveServices/accounts', etc.")]
    public required string ResourceTypes { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }
}
