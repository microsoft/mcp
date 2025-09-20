// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace Azure.Mcp.Tools.Foundry.Models;

public class AgentsConnectResult
{
    [JsonPropertyName("agentId")]
    public string? AgentId { get; set; }

    [JsonPropertyName("threadId")]
    public string? ThreadId { get; set; }

    [JsonPropertyName("runId")]
    public string? RunId { get; set; }

    [JsonPropertyName("responseText")]
    public string? ResponseText { get; set; }

    [JsonPropertyName("queryText")]
    public string? QueryText { get; set; }

    [JsonPropertyName("response")]
    public List<ChatMessage>? Response { get; set; }

    [JsonPropertyName("query")]
    public List<ChatMessage>? Query { get; set; }

    [JsonPropertyName("toolDefinitions")]
    public string? ToolDefinitions { get; set; }

    [JsonPropertyName("citations")]
    public List<string>? Citations { get; set; }
}
