// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

public sealed class ItemJobInstanceGetOptions()
{
    [Option(Description = "The ID of the Microsoft Fabric workspace containing the item. Must be a nonempty UUID.")]
    public required string WorkspaceId { get; set; }

    [Option(Description = "The ID of the Microsoft Fabric item whose job instance should be retrieved. Must be a nonempty UUID.")]
    public required string ItemId { get; set; }

    [Option(Description = "The ID of an existing job instance for this item, not a Fabric operation ID. Must be a nonempty UUID.")]
    public required string JobInstanceId { get; set; }
}
