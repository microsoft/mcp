// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.EventHubs.Options.ConsumerGroup;

public class ConsumerGroupDeleteOptions : BaseEventHubsOptions
{
    [JsonPropertyName(EventHubsOptionDefinitions.EventHub)]
    public string EventHubName { get; set; } = string.Empty;

    [JsonPropertyName(EventHubsOptionDefinitions.Namespace)]
    public string NamespaceName { get; set; } = string.Empty;

    [JsonPropertyName(EventHubsOptionDefinitions.ConsumerGroup)]
    public string ConsumerGroupName { get; set; } = string.Empty;
}
