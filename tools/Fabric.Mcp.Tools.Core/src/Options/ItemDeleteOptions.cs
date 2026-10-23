// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

public sealed class ItemDeleteOptions()
{
    [Option(Description = "The ID of the Microsoft Fabric workspace containing the item. Must be a nonempty UUID.")]
    public required string WorkspaceId { get; set; }

    [Option(Description = "The ID of the Microsoft Fabric item to delete. Must be a nonempty UUID.")]
    public required string ItemId { get; set; }

    [Option(Description = "Explicitly pass true to permanently delete the item; this cannot be recovered and requires workspace Admin. Omit or pass false to use Fabric's default deletion behavior, which soft-deletes only supported item types and does not guarantee recovery. When supplied, this option requires an explicit true or false value.")]
    public bool? HardDelete { get; set; }
}
