// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Cosmos.Options.Item;

public class ItemVectorSearchOptions : BaseContainerOptions
{
    [JsonPropertyName(CosmosOptionDefinitions.VectorPropertyName)]
    public string? VectorProperty { get; set; }

    [JsonPropertyName(CosmosOptionDefinitions.SelectPropertiesName)]
    public string? SelectProperties { get; set; }

    [JsonPropertyName(CosmosOptionDefinitions.CountName)]
    public int? Count { get; set; }

    [JsonPropertyName(CosmosOptionDefinitions.SearchTextName)]
    public string? SearchText { get; set; }

    [JsonPropertyName(CosmosOptionDefinitions.OpenAIEndpointName)]
    public string? OpenAIEndpoint { get; set; }

    [JsonPropertyName(CosmosOptionDefinitions.EmbeddingDeploymentName)]
    public string? EmbeddingDeployment { get; set; }

    [JsonPropertyName(CosmosOptionDefinitions.EmbeddingDimensionsName)]
    public int? EmbeddingDimensions { get; set; }
}
