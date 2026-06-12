// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.OneLake.Options;

public class PathListOptions
{
    [Option("The ID of the Microsoft Fabric workspace.")]
    public string? WorkspaceId { get; set; }

    [Option("The name or ID of the Microsoft Fabric workspace.")]
    public string? Workspace { get; set; }

    [Option("The ID of the Fabric item.")]
    public string? ItemId { get; set; }

    [Option("The name or ID of the Fabric item. When using friendly names, MUST include the item type suffix (e.g., 'ItemName.Lakehouse', 'ItemName.Warehouse').")]
    public string? Item { get; set; }

    [Option("The path to list in OneLake storage (optional, defaults to root).")]
    public string? Path { get; set; }

    [Option("Whether to perform the operation recursively.")]
    public bool Recursive { get; set; }

    [Option("Output format for OneLake API responses. Use 'json' for parsed objects, 'xml' for raw XML API response, or 'raw' for unprocessed API response. Supported values: 'json' (default), 'xml', 'raw'.")]
    public string? Format { get; set; }
}
