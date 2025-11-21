// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.AI.Agents.Persistent;

namespace Azure.Mcp.Tools.Foundry.Models;

internal class AgentFileSearchResult
{
    [JsonPropertyName("fileId")]
    public string? FileId { get; set; }

    [JsonPropertyName("fileName")]
    public string? FileName { get; set; }

    [JsonPropertyName("score")]
    public float Score { get; set; }

    [JsonPropertyName("content")]
    public IReadOnlyList<FileSearchToolCallContent>? Content { get; set; }
}
