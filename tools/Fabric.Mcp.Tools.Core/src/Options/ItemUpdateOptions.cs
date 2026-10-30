// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

public sealed class ItemUpdateOptions()
{
    [Option(Description = "The nonempty UUID of the Microsoft Fabric workspace containing the item.")]
    public required string WorkspaceId { get; set; }

    [Option(Description = "The nonempty UUID of the Microsoft Fabric item to update.")]
    public required string ItemId { get; set; }

    [Option(Description = "The new item display name. Must follow the naming rules for the item's type. Omit to keep the current name.")]
    public string? DisplayName { get; set; }

    [Option(
        Description = "The new item description, at most 256 characters. An empty string clears the description; omit to keep it unchanged.",
        AllowEmptyOrWhiteSpaceString = true)]
    public string? Description { get; set; }
}
