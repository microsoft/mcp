// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Workflows;

public sealed class WorkflowsGenerateOptions
{
    [Option("YAML kind: agent or tool.")]
    public required string Kind { get; set; }

    [Option(SreAgentOptionDefinitions.NameDescription)]
    public required string Name { get; set; }

    [Option(SreAgentOptionDefinitions.DescriptionDescription)]
    public required string Description { get; set; }

    [Option("Tool type, such as KustoTool or LinkTool.")]
    public string? ModelOrType { get; set; }

    [Option(SreAgentOptionDefinitions.ToolsDescription)]
    public string[]? Tools { get; set; }

    [Option(SreAgentOptionDefinitions.HandoffsDescription)]
    public string[]? Handoffs { get; set; }

    [Option(SreAgentOptionDefinitions.ConnectorDescription)]
    public string? Connector { get; set; }

    [Option(SreAgentOptionDefinitions.DatabaseDescription)]
    public string? Database { get; set; }

    [Option(SreAgentOptionDefinitions.QueryDescription)]
    public string? Query { get; set; }

    [Option(SreAgentOptionDefinitions.UrlTemplateDescription)]
    public string? UrlTemplate { get; set; }

    [Option("Parameters as name:description.")]
    public string[]? Parameters { get; set; }
}
