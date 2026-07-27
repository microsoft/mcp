// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Skills;

public sealed class SkillsCreateOptions : BaseSreAgentOptions
{
    [Option(Description = SreAgentOptionDefinitions.NameDescription)]
    public required string Name { get; set; }

    [Option(Description = SreAgentOptionDefinitions.ContentDescription)]
    public required string Content { get; set; }

    [Option(Description = SreAgentOptionDefinitions.DescriptionDescription)]
    public string? Description { get; set; }
}
