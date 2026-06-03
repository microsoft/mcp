// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Cosmos.Options.Item;

public class ItemTextSearchOptions : BaseContainerOptions
{
    [JsonPropertyName(CosmosOptionDefinitions.SearchPropertyName)]
    public string? SearchProperty { get; set; }

    [JsonPropertyName(CosmosOptionDefinitions.SearchPhraseName)]
    public string? SearchPhrase { get; set; }

    [JsonPropertyName(CosmosOptionDefinitions.CountName)]
    public int? Count { get; set; }

    [JsonPropertyName(CosmosOptionDefinitions.PropertiesToSelectName)]
    public string? PropertiesToSelect { get; set; }
}
