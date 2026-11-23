// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

public sealed class FolderMoveOptions()
{
    [Option(Description = "The Microsoft Fabric workspace ID. Must be a nonempty UUID; names are not resolved.")]
    public required string WorkspaceId { get; set; }

    [Option(Description = "The existing logical workspace folder to move. Must be a nonempty UUID, not a name or filesystem path.")]
    public required string FolderId { get; set; }

    [Option(Description = "The existing destination folder UUID in the same workspace. Omit or pass null to move to the workspace root. Cannot be the folder being moved; Fabric validates descendant, depth, and name conflicts.")]
    public string? TargetFolderId { get; set; }
}
