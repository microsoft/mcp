// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

public sealed record FolderMoveCommandResult(
    FabricFolder Folder,
    long? RetryAfterSeconds = null,
    DateTimeOffset? RetryAfterUtc = null)
{
    public string MoveStatus => "Moved";

    public string Guidance =>
        "Fabric confirmed folder movement (HTTP 200). This is the returned metadata snapshot. No lookup, descendant enumeration, follow-up read, polling, or automatic retry was performed.";
}
