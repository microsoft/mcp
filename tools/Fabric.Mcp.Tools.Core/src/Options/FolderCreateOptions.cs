// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

public sealed class FolderCreateOptions()
{
    [Option(Description = "The Microsoft Fabric workspace ID. Must be a nonempty UUID; names are not resolved.")]
    public required string WorkspaceId { get; set; }

    [Option(Description = "The name of one logical workspace folder, not a filesystem path. Must meet Fabric folder naming rules: at most 255 characters, no leading/trailing spaces, control codes, prohibited characters, or reserved names. The value is not trimmed or sanitized.")]
    public required string DisplayName { get; set; }

    [Option(Description = "An existing parent folder ID in the workspace. Must be a nonempty UUID. Omit or supply null to create at the workspace root; no parent lookup or creation is performed.")]
    public string? ParentFolderId { get; set; }
}
