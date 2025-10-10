// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.EventHubs.Options;

public class BaseEventHubsOptions : SubscriptionOptions
{
    public string? Namespace { get; set; }


    public string? EventHub { get; set; }

    [JsonPropertyName(EventHubsOptionDefinitions.ConsumerGroup)]
    public string? ConsumerGroup { get; set; }
}
