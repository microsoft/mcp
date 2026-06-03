// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Cosmos.Options.Item;

public class ItemListRecentOptions : BaseContainerOptions
{
    [JsonPropertyName(CosmosOptionDefinitions.CountName)]
    public int? Count { get; set; }
}
