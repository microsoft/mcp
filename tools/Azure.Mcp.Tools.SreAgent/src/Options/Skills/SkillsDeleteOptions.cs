// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Options.Skills;

public class SkillsDeleteOptions : BaseSreAgentOptions
{
    public string Name { get; set; } = string.Empty;

    public bool Confirm { get; set; }
}
