// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.OneLake.Options;

public sealed class TableGetOptions
{
    [Option("The ID of the Microsoft Fabric workspace.")]
    public string? WorkspaceId { get; set; }

    [Option("The name or ID of the Microsoft Fabric workspace.")]
    public string? Workspace { get; set; }

    [Option("The ID of the Fabric item.")]
    public string? ItemId { get; set; }

    [Option("The name or ID of the Fabric item. When using friendly names, MUST include the item type suffix (e.g., 'ItemName.Lakehouse', 'ItemName.Warehouse').")]
    public string? Item { get; set; }

    [Option("The table namespace (schema) to inspect within the OneLake table API.")]
    public string? Namespace { get; set; }

    [Option("Alias for --namespace when specifying table schemas in the OneLake table API.")]
    public string? Schema { get; set; }

    [Option("The table name exposed by the OneLake table API.")]
    public required string Table { get; set; }
}
