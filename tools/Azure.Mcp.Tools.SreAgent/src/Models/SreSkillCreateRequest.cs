// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Models;

public sealed class SreSkillCreateRequest
{
    public required string Name { get; set; }

    public string Type { get; set; } = "Skill";

    public SreSkillProperties Properties { get; set; } = new();
}
