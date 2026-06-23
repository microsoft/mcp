// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Agents;

public sealed class AgentsToolsCreateOptions : BaseSreAgentOptions
{
    [Option(Description = SreAgentOptionDefinitions.NameDescription)]
    public required string Name { get; set; }

    [Option(Description = "The custom tool type, such as KustoTool or LinkTool.")]
    public required string ToolType { get; set; }

    [Option(Description = SreAgentOptionDefinitions.DescriptionDescription)]
    public string? Description { get; set; }

    [Option(Description = SreAgentOptionDefinitions.ConnectorDescription)]
    public string? Connector { get; set; }

    [Option(Description = SreAgentOptionDefinitions.DatabaseDescription)]
    public string? Database { get; set; }

    [Option(Description = SreAgentOptionDefinitions.QueryDescription)]
    public string? Query { get; set; }

    [Option(Description = SreAgentOptionDefinitions.UrlTemplateDescription)]
    public string? UrlTemplate { get; set; }

    [Option(Description = "JSON array of tool parameter definitions.")]
    public string? Parameters { get; set; }
}
