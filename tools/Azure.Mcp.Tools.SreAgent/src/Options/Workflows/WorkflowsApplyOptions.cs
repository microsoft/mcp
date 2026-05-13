// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Options.Workflows;

public class WorkflowsApplyOptions : BaseSreAgentOptions
{
    public string? YamlContent { get; set; }
    public string? SourceName { get; set; }
}
