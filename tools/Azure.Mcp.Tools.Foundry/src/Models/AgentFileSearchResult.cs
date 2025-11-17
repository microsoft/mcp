// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.AI.Agents.Persistent;

namespace Azure.Mcp.Tools.Foundry.Models;

internal class AgentFileSearchResult
{
    [JsonPropertyName("fileId")]
    public string? file_id { get; set; }

    [JsonPropertyName("fileName")]
    public string? file_name { get; set; }

    [JsonPropertyName("score")]
    public float score { get; set; }

    [JsonPropertyName("content")]
    public IReadOnlyList<FileSearchToolCallContent>? content { get; set; }
}
