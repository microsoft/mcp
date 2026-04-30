// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureTerraform.Models;

public sealed class InstallationHelp
{
    public string ToolName { get; set; } = string.Empty;
    public List<InstallationMethod> InstallationMethods { get; set; } = [];
    public string DocumentationUrl { get; set; } = string.Empty;
    public string VerifyCommand { get; set; } = string.Empty;
    public List<string> AdditionalNotes { get; set; } = [];
}
