// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.SreAgent.Models;

public sealed class SreSkillProperties
{
    public string? Content { get; set; }

    public string? Description { get; set; }

    public string? AgentName { get; set; }

    public string? SkillMdContent { get; set; }

    public List<string>? Tools { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
