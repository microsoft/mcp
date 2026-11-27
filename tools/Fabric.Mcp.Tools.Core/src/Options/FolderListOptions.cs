// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

public sealed class FolderListOptions()
{
    [Option(Description = "The Microsoft Fabric workspace ID. Must be a nonempty UUID; names are not resolved.")]
    public required string WorkspaceId { get; set; }

    [Option(Description = "An existing root folder ID in the workspace. Must be a nonempty UUID. Omit to use the workspace root.")]
    public string? RootFolderId { get; set; }

    [Option(Description = "Include nested logical workspace folders. Omission uses the API default of true; false lists only direct children. This never performs client-side folder walking.")]
    public bool? Recursive { get; set; }

    [Option(Description = "The unchanged continuation token from a previous page. Keep the same workspace, root folder, and recursive options. Omit for the first page. Must be valid Unicode, not blank, without control characters and at most 8,192 UTF-8 bytes. Do not supply a continuation URL.")]
    public string? ContinuationToken { get; set; }
}
