// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.OneLake.Options;

public sealed class TableGetOptions
{
    [Option(Description = OneLakeOptionDescriptions.WorkspaceId)]
    public string? WorkspaceId { get; set; }

    [Option(Description = OneLakeOptionDescriptions.Workspace)]
    public string? Workspace { get; set; }

    [Option(Description = OneLakeOptionDescriptions.ItemId)]
    public string? ItemId { get; set; }

    [Option(Description = OneLakeOptionDescriptions.Item)]
    public string? Item { get; set; }

    [Option(Description = OneLakeOptionDescriptions.Namespace)]
    public string? Namespace { get; set; }

    [Option(Description = OneLakeOptionDescriptions.Schema)]
    public string? Schema { get; set; }

    [Option(Description = "The table name exposed by the OneLake table API.")]
    public required string Table { get; set; }
}
