// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

public sealed record ItemMoveCommandResult(
    IReadOnlyList<FabricMovedItem> Items,
    long? RetryAfterSeconds = null,
    DateTimeOffset? RetryAfterUtc = null)
{
    public string MoveStatus => "Moved";

    public string Guidance =>
        "Fabric confirmed item movement (HTTP 200). Items is the complete returned metadata collection, including any returned child items, in service order. An empty collection or absent fields do not establish which items were affected. No lookup, follow-up read, polling, or automatic retry was performed.";
}
