// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Cosmos.Options.Item;

public class ItemGetOptions : BaseContainerOptions
{
    [JsonPropertyName(CosmosOptionDefinitions.ItemIdName)]
    public string? Id { get; set; }

    [JsonPropertyName(CosmosOptionDefinitions.PartitionKeyName)]
    public string? PartitionKey { get; set; }
}
