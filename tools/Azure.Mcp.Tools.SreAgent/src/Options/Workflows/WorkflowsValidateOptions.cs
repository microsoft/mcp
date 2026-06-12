// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Workflows;

public class WorkflowsValidateOptions : GlobalOptions
{
    public string? YamlContent { get; set; }
}
