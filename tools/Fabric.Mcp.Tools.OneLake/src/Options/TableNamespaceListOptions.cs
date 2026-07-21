// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.OneLake.Options;

public sealed class TableNamespaceListOptions
{
    [Option(Description = OneLakeOptionDescriptions.WorkspaceId)]
    public string? WorkspaceId { get; set; }

    [Option(Description = OneLakeOptionDescriptions.Workspace)]
    public string? Workspace { get; set; }

    [Option(Description = OneLakeOptionDescriptions.ItemId)]
    public string? ItemId { get; set; }

    [Option(Description = OneLakeOptionDescriptions.Item)]
    public string? Item { get; set; }
}
