// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.SreAgent.Models;

public sealed class SreSkillProperties
{
    // Server expects this property to be serialized as "skillContent" (camelCase of SkillContent).
    // Sending "content" causes the SRE Agent skills data-plane endpoint to reject the request.
    public string? SkillContent { get; set; }

    public string? Description { get; set; }

    public string? SkillMdContent { get; set; }

    public List<string>? Tools { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
