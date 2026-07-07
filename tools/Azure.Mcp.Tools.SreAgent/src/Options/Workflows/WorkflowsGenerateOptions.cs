// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Workflows;

public sealed class WorkflowsGenerateOptions
{
    [Option(Description = "YAML kind: agent or tool.")]
    public required string Kind { get; set; }

    [Option(Description = SreAgentOptionDefinitions.NameDescription)]
    public required string Name { get; set; }

    [Option(Description = SreAgentOptionDefinitions.DescriptionDescription)]
    public required string Description { get; set; }

    [Option(Description = "Tool type, such as KustoTool or LinkTool.")]
    public string? ModelOrType { get; set; }

    [Option(Description = SreAgentOptionDefinitions.ToolsDescription)]
    public string[]? Tools { get; set; }

    [Option(Description = SreAgentOptionDefinitions.HandoffsDescription)]
    public string[]? Handoffs { get; set; }

    [Option(Description = SreAgentOptionDefinitions.ConnectorDescription)]
    public string? Connector { get; set; }

    [Option(Description = SreAgentOptionDefinitions.DatabaseDescription)]
    public string? Database { get; set; }

    [Option(Description = SreAgentOptionDefinitions.QueryDescription)]
    public string? Query { get; set; }

    [Option(Description = SreAgentOptionDefinitions.UrlTemplateDescription)]
    public string? UrlTemplate { get; set; }

    [Option(Description = "Parameters as name:description.")]
    public string[]? Parameters { get; set; }
}
