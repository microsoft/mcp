// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureTerraform.Models;

public sealed class InstallationMethod
{
    public string Platform { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public bool ManagesPath { get; set; }
}
