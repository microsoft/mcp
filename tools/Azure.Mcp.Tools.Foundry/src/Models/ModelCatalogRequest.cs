// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Foundry.Models;

public class ModelCatalogRequest
{
    [JsonPropertyName("filters")]
    public List<ModelCatalogFilter> Filters { get; set; } = [];

    [JsonPropertyName("continuationToken")]
    public string? ContinuationToken { get; set; }
}
