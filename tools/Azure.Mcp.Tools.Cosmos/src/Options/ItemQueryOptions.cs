// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Cosmos.Options;

public class ItemQueryOptions : BaseContainerOptions
{
    [JsonPropertyName(CosmosOptionDefinitions.QueryText)]
    public string? Query { get; set; }
}
