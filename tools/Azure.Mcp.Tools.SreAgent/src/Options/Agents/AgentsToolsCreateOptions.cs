// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Agents;

public class AgentsToolsCreateOptions : ISreAgentOption
{
    [Option("The name of the Azure SRE Agent resource to target.")]
    public required string? Agent { get; set; }

    [Option(OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Name = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }

    [Option("The name of the SRE Agent item.")]
    public required string Name { get; set; }

    [Option("The custom tool type, such as KustoTool or LinkTool.")]
    public required string ToolType { get; set; }

    [Option("A description for the SRE Agent item.")]
    public string? Description { get; set; }

    [Option("The connector name for Kusto tools.")]
    public string? Connector { get; set; }

    [Option("The Kusto database for Kusto tools.")]
    public string? Database { get; set; }

    [Option("The Kusto query for Kusto tools.")]
    public string? Query { get; set; }

    [Option("The URL template for link tools.")]
    public string? UrlTemplate { get; set; }

    [Option("JSON array of tool parameter definitions.")]
    public string? Parameters { get; set; }
}
