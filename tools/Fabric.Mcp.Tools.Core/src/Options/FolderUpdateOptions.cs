// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

public sealed class FolderUpdateOptions()
{
    [Option(Description = "The Microsoft Fabric workspace ID. Must be a nonempty UUID; names are not resolved.")]
    public required string WorkspaceId { get; set; }

    [Option(Description = "The existing logical workspace folder ID. Must be a nonempty UUID, not a name or filesystem path.")]
    public required string FolderId { get; set; }

    [Option(Description = "The required replacement folder display name. Must meet Fabric folder naming rules: 1 to 255 characters, no leading/trailing spaces, control codes, prohibited characters, or reserved names. The value is not trimmed or sanitized.")]
    public required string DisplayName { get; set; }
}
