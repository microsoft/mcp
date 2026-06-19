// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Workflows;

public sealed class WorkflowsApplyOptions : BaseSreAgentOptions
{
    [Option(SreAgentOptionDefinitions.YamlContentDescription)]
    public required string YamlContent { get; set; }

    [Option("Optional source name.")]
    public string? SourceName { get; set; }

}
