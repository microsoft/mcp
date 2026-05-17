// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Quota.Options.Region;

public sealed class AvailabilityListOptions : SubscriptionOptions
{
    [JsonPropertyName("resourceTypes")]
    public string ResourceTypes { get; set; } = string.Empty;

    [JsonPropertyName("modelName")]
    public string? CognitiveServiceModelName { get; set; }

    [JsonPropertyName("modelVersion")]
    public string? CognitiveServiceModelVersion { get; set; }

    [JsonPropertyName("deploymentSkuName")]
    public string? CognitiveServiceDeploymentSkuName { get; set; }
}
