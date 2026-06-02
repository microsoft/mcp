// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

public class ItemCreateOptions
{
    [Option("The ID of the Microsoft Fabric workspace.")]
    public string? WorkspaceId { get; set; }

    [Option("The name or ID of the Microsoft Fabric workspace.")]
    public string? Workspace { get; set; }

    [Option("The display name for the item.")]
    public required string DisplayName { get; set; }

    [Option("The type of the Fabric item (e.g., Lakehouse, Notebook, etc.).")]
    public required string ItemType { get; set; }

    [Option("The description for the item.")]
    public string? Description { get; set; }
}
