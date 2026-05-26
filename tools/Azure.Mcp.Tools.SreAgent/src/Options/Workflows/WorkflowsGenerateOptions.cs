// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Workflows;

public class WorkflowsGenerateOptions
{
    [Option("YAML kind: agent or tool.")]
    public required string Kind { get; set; }

    [Option("The name of the SRE Agent item.")]
    public required string Name { get; set; }

    [Option("A description for the SRE Agent item.")]
    public required string Description { get; set; }

    [Option("Tool type, such as KustoTool or LinkTool.")]
    public string? ModelOrType { get; set; }

    [Option("Tool names to attach. Multiple values are supported.")]
    public string[]? Tools { get; set; }

    [Option("Sub-agent handoff names. Multiple values are supported.")]
    public string[]? Handoffs { get; set; }

    [Option("The connector name for Kusto tools.")]
    public string? Connector { get; set; }

    [Option("The Kusto database for Kusto tools.")]
    public string? Database { get; set; }

    [Option("The Kusto query for Kusto tools.")]
    public string? Query { get; set; }

    [Option("The URL template for link tools.")]
    public string? UrlTemplate { get; set; }

    [Option("Parameters as name:description.")]
    public string[]? Parameters { get; set; }

    [Option(OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Name = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
