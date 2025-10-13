// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.EventHubs.Options.ConsumerGroup;

public class ConsumerGroupUpdateOptions : BaseEventHubsOptions
{
    [JsonPropertyName(EventHubsOptionDefinitions.UserMetadata)]
    public string? UserMetadata { get; set; }
}
