// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SreAgent.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Workflows;

public class WorkflowsGenerateOptions : GlobalOptions
{
    public string? Kind { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ModelOrType { get; set; }
    public string[]? Tools { get; set; }
    public string[]? Handoffs { get; set; }
    public string? Connector { get; set; }
    public string? Database { get; set; }
    public string? Query { get; set; }
    public string? UrlTemplate { get; set; }
    public string[]? Parameters { get; set; }
}

public class WorkflowsValidateOptions : GlobalOptions
{
    public string? YamlContent { get; set; }
}

public class WorkflowsApplyOptions : BaseSreAgentOptions
{
    public string? YamlContent { get; set; }
    public string? SourceName { get; set; }
}
