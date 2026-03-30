// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.OneLake.Options;

public sealed class TableGetOptions : BaseItemOptions
{
    public string? Namespace { get; set; }
    public string? Table { get; set; }
}
