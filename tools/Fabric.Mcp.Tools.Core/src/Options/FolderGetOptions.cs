// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

public sealed class FolderGetOptions()
{
    [Option(Description = "The ID of the Microsoft Fabric workspace containing the folder. Must be a nonempty UUID; names are not resolved.")]
    public required string WorkspaceId { get; set; }

    [Option(Description = "The ID of the logical workspace folder to retrieve. Must be a nonempty UUID, not a folder name, OneLake directory, or local path.")]
    public required string FolderId { get; set; }
}
