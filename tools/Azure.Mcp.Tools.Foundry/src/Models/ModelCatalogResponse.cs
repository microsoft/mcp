// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Foundry.Models;

public class ModelCatalogResponse
{
    [JsonPropertyName("summaries")] public List<ModelInformation> Summaries { get; set; } = [];

    [JsonPropertyName("totalCount")] public int TotalCount { get; set; }

    [JsonPropertyName("continuationToken")]
    public string? ContinuationToken { get; set; }
}
