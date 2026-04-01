// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Quota.Options.Usage;

public sealed class CheckOptions : SubscriptionOptions
{
    [JsonPropertyName("region")]
    public string Region { get; set; } = string.Empty;

    [JsonPropertyName("resourceTypes")]
    public string ResourceTypes { get; set; } = string.Empty;
}
