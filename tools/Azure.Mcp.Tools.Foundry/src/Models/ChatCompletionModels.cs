// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Foundry.Models;

public sealed record ChatCompletionResult(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("object")] string Object,
    [property: JsonPropertyName("created")] long Created,
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("choices")] List<ChatCompletionChoice> Choices,
    [property: JsonPropertyName("usage")] ChatCompletionUsageInfo Usage,
    [property: JsonPropertyName("system_fingerprint")] string? SystemFingerprint);

public sealed record ChatCompletionChoice(
    [property: JsonPropertyName("index")] int Index,
    [property: JsonPropertyName("message")] ChatCompletionMessage Message,
    [property: JsonPropertyName("finish_reason")] string? FinishReason,
    [property: JsonPropertyName("logprobs")] object? LogProbs);

public sealed record ChatCompletionMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string? Content,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("function_call")] object? FunctionCall,
    [property: JsonPropertyName("tool_calls")] object? ToolCalls);

public sealed record ChatCompletionUsageInfo(
    [property: JsonPropertyName("prompt_tokens")] int PromptTokens,
    [property: JsonPropertyName("completion_tokens")] int CompletionTokens,
    [property: JsonPropertyName("total_tokens")] int TotalTokens);

public sealed record ChatCompletionCreateResult(
    [property: JsonPropertyName("result")] ChatCompletionResult Result);
