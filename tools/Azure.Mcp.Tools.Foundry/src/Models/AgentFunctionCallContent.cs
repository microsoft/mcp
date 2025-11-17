// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Foundry.Models;

internal class AgentFunctionCallContent
{
    [JsonPropertyName("callerId")]
    public string? CallerId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("arguments")]
    public IDictionary<string, JsonElement>? Arguments { get; set; }
}
