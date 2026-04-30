// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureTerraform.Models;

public sealed class AvmVersionListResult
{
    public string ModuleName { get; set; } = string.Empty;
    public List<AvmVersion> Versions { get; set; } = [];
}
