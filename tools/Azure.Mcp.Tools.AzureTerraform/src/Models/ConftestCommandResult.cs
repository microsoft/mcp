// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureTerraform.Models;

public sealed class ConftestCommandResult
{
    public bool ConftestFound { get; set; }
    public string Command { get; set; } = string.Empty;
    public List<string> Args { get; set; } = [];
    public string Description { get; set; } = string.Empty;
    public string? WorkspaceFolder { get; set; }
    public string? WorkingDirectory { get; set; }
    public string? PolicySet { get; set; }
    public List<string> Notes { get; set; } = [];
    public InstallationHelp? InstallationHelp { get; set; }
}
