// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.SreAgent.Models;

public sealed class SreAgentToolProperties
{
    public string? Type { get; set; }

    public string? ToolType { get; set; }

    public string? Description { get; set; }

    public string? Connector { get; set; }

    public string? Database { get; set; }

    public string? Query { get; set; }

    public string? Template { get; set; }

    public List<SreAgentToolParameter>? Parameters { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
