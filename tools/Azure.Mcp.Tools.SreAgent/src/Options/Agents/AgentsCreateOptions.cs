// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Agents;

public sealed class AgentsCreateOptions : BaseSreAgentOptions
{
    [Option(Description = SreAgentOptionDefinitions.NameDescription)]
    public required string Name { get; set; }

    [Option(Description = SreAgentOptionDefinitions.DescriptionDescription)]
    public string? Description { get; set; }

    [Option(Description = "Instructions for the sub-agent.")]
    public string? Instructions { get; set; }

    [Option(Description = SreAgentOptionDefinitions.ToolsDescription)]
    public string[]? Tools { get; set; }

    [Option(Description = SreAgentOptionDefinitions.HandoffsDescription)]
    public string[]? Handoffs { get; set; }
}
