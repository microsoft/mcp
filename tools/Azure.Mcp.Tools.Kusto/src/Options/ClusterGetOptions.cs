// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Kusto.Options;

public class ClusterGetOptions : SubscriptionOptions
{
    [JsonPropertyName(KustoOptionDefinitions.ClusterName)]
    public string? ClusterName { get; set; }
}
