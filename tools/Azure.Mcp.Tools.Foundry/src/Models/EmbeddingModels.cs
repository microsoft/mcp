// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Foundry.Models;

public sealed record EmbeddingResult(
    [property: JsonPropertyName("object")] string Object,
    [property: JsonPropertyName("data")] List<EmbeddingData> Data,
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("usage")] EmbeddingUsageInfo Usage);

public sealed record EmbeddingData(
    [property: JsonPropertyName("object")] string Object,
    [property: JsonPropertyName("index")] int Index,
    [property: JsonPropertyName("embedding")] float[] Embedding);

public sealed record EmbeddingUsageInfo(
    [property: JsonPropertyName("prompt_tokens")] int PromptTokens,
    [property: JsonPropertyName("total_tokens")] int TotalTokens);
