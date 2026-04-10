// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.OneLake.Options;

public sealed class TableListOptions : GlobalOptions
{
    public string? WorkspaceId { get; set; }
    public string? Workspace { get; set; }
    public string? ItemId { get; set; }
    public string? Item { get; set; }
    public string? Namespace { get; set; }
}
