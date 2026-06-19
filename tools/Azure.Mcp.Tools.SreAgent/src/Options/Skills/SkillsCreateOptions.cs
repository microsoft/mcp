// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Skills;

public sealed class SkillsCreateOptions : BaseSreAgentOptions
{
    [Option(SreAgentOptionDefinitions.NameDescription)]
    public required string Name { get; set; }

    [Option(SreAgentOptionDefinitions.ContentDescription)]
    public required string Content { get; set; }

    [Option(SreAgentOptionDefinitions.DescriptionDescription)]
    public string? Description { get; set; }
}
