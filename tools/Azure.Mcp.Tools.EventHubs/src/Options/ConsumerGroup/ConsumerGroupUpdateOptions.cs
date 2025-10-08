// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.EventHubs.Options;

namespace Azure.Mcp.Tools.EventHubs.Options.ConsumerGroup;

public class ConsumerGroupUpdateOptions : BaseEventHubsOptions
{
    [JsonPropertyName(EventHubsOptionDefinitions.EventHubName)]
    public string EventHubName { get; set; } = string.Empty;

    [JsonPropertyName(EventHubsOptionDefinitions.Namespace)]
    public string NamespaceName { get; set; } = string.Empty;

    [JsonPropertyName(EventHubsOptionDefinitions.ConsumerGroup)]
    public string ConsumerGroupName { get; set; } = string.Empty;

    [JsonPropertyName(EventHubsOptionDefinitions.UserMetadata)]
    public string? UserMetadata { get; set; }
}
