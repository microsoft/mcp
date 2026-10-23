// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Quota.Options.Region;

public sealed class AvailabilityListOptions : ISubscriptionOption
{
    [Option(Description = "Comma-separated list of Azure resource types to check available regions for. E.g. 'Microsoft.App/containerApps, Microsoft.Web/sites, Microsoft.CognitiveServices/accounts'.")]
    public required string ResourceTypes { get; set; }

    [Option(Description = "Optional model name for cognitive services. Only needed when Microsoft.CognitiveServices is included in resource types.")]
    public string? CognitiveServiceModelName { get; set; }

    [Option(Description = "Optional model version for cognitive services. Only needed when Microsoft.CognitiveServices is included in resource types.")]
    public string? CognitiveServiceModelVersion { get; set; }

    [Option(Description = "Optional deployment SKU name for cognitive services. Only needed when Microsoft.CognitiveServices is included in resource types.")]
    public string? CognitiveServiceDeploymentSkuName { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }
}
