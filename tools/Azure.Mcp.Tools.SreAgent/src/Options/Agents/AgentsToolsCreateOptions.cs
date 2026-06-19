// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Agents;

public sealed class AgentsToolsCreateOptions : ISubscriptionOption
{
    [Option(SreAgentOptionDefinitions.AgentDescription)]
    public required string Agent { get; set; }

    [Option(SreAgentOptionDefinitions.NameDescription)]
    public required string Name { get; set; }

    [Option("The custom tool type, such as KustoTool or LinkTool.")]
    public required string ToolType { get; set; }

    [Option(SreAgentOptionDefinitions.DescriptionDescription)]
    public string? Description { get; set; }

    [Option(SreAgentOptionDefinitions.ConnectorDescription)]
    public string? Connector { get; set; }

    [Option(SreAgentOptionDefinitions.DatabaseDescription)]
    public string? Database { get; set; }

    [Option(SreAgentOptionDefinitions.QueryDescription)]
    public string? Query { get; set; }

    [Option(SreAgentOptionDefinitions.UrlTemplateDescription)]
    public string? UrlTemplate { get; set; }

    [Option("JSON array of tool parameter definitions.")]
    public string? Parameters { get; set; }

    [Option(OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(Name = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
