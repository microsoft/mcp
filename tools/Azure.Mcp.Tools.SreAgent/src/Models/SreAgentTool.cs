// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.SreAgent.Models;

public sealed class SreAgentTool
{
    public string? Name { get; set; }

    public string? Type { get; set; }

    public SreAgentToolProperties? Properties { get; set; }

    public List<string>? Tags { get; set; }

    public string? Owner { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
