// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureTerraform.Models;

public sealed class AvmDocumentationResult
{
    public string ModuleName { get; set; } = string.Empty;
    public string ModuleVersion { get; set; } = string.Empty;
    public string Documentation { get; set; } = string.Empty;
}
