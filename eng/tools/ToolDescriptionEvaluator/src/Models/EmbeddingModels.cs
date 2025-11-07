// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace ToolSelection.Models;

// Azure OpenAI Embedding API models
public class EmbeddingRequest
{
    [JsonPropertyName("input")]
    public required string[] Input { get; set; }
}

public class EmbeddingResponse
{
    [JsonPropertyName("data")]
    public EmbeddingData[]? Data { get; set; }

    [JsonPropertyName("usage")]
    public Usage? Usage { get; set; }

    [JsonPropertyName("error")]
    public ApiError? Error { get; set; }
}

public class EmbeddingData
{
    [JsonPropertyName("embedding")]
    public required float[] Embedding { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }
}

public class Usage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

public class ApiError
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
