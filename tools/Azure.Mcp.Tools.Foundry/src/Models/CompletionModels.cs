// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Foundry.Models;

public sealed record CompletionResult(
    [property: JsonPropertyName("completionText")] string CompletionText,
    [property: JsonPropertyName("usageInfo")] CompletionUsageInfo UsageInfo);

public sealed record CompletionUsageInfo(
    [property: JsonPropertyName("promptTokens")] int PromptTokens,
    [property: JsonPropertyName("completionTokens")] int CompletionTokens,
    [property: JsonPropertyName("totalTokens")] int TotalTokens);
