// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Options.Agents;

public class AgentsToolsCreateOptions : BaseSreAgentOptions
{
    public string? Name { get; set; }

    public string? ToolType { get; set; }

    public string? Description { get; set; }

    public string? Connector { get; set; }

    public string? Database { get; set; }

    public string? Query { get; set; }

    public string? UrlTemplate { get; set; }

    public string? Parameters { get; set; }
}
