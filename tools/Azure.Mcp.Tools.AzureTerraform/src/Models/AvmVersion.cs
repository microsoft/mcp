// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureTerraform.Models;

public sealed class AvmVersion
{
    public string TagName { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string TarballUrl { get; set; } = string.Empty;
}
