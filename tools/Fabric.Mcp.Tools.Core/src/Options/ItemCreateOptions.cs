// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

public class ItemCreateOptions
{
    [Option(Description = "The nonempty UUID of the Microsoft Fabric workspace. Workspace names are not resolved. Takes precedence over workspace unless omitted, empty, or whitespace.")]
    public string? WorkspaceId { get; set; }

    [Option(Description = "Backward-compatible alias for the workspace UUID, used when workspace-id is omitted or blank. Workspace names are not supported.")]
    public string? Workspace { get; set; }

    [Option(Description = "The display name for the item, following the naming rules for its item type.")]
    public required string DisplayName { get; set; }

    [Option(Description = "The type of the Fabric item (e.g., Lakehouse, Notebook, etc.).")]
    public required string ItemType { get; set; }

    [Option(Description = "The description for the item, at most 256 characters.")]
    public string? Description { get; set; }
}
