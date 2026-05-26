// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Options.Skills;

public class SkillsCreateOptions : BaseSreAgentOptions
{
    public string Name { get; set; } = string.Empty;

    public string? Content { get; set; }

    public string? Description { get; set; }
}
