// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>A receipt for deletion of the explicitly requested empty folder, not a returned folder snapshot.</summary>
public sealed record FolderDeleteCommandResult(Guid WorkspaceId, Guid FolderId)
{
    public string DeletionStatus => "Deleted";

    public string Guidance =>
        "Fabric confirmed deletion of the requested empty folder (HTTP 200). No folder snapshot was returned. " +
        "No items or nested folders were deleted or moved. No lookup, follow-up read, polling, or automatic retry was performed. " +
        "Do not resubmit this confirmed deletion. The API does not document folder recovery or a retention period.";
}
