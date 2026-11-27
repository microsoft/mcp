// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

public sealed class ItemMoveOptions()
{
    [Option(Description = "The Microsoft Fabric workspace ID. Must be a nonempty UUID; names are not resolved.")]
    public required string WorkspaceId { get; set; }

    [Option(Description = "The existing item to move. Must be a nonempty UUID. Fabric moves child items with their parent; a child cannot be moved on its own.")]
    public required string ItemId { get; set; }

    [Option(Description = "The existing destination folder UUID in the same workspace. Omit or pass null to move to the workspace root. No destination is created or looked up.")]
    public string? TargetFolderId { get; set; }
}
