// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

public sealed class ItemJobInstanceListOptions()
{
    [Option(Description = "The ID of the Microsoft Fabric workspace containing the item. Must be a nonempty UUID.")]
    public required string WorkspaceId { get; set; }

    [Option(Description = "The ID of the Microsoft Fabric item whose job instances should be listed. Must be a nonempty UUID.")]
    public required string ItemId { get; set; }

    [Option(Description = "The unchanged continuation token from a previous page for the same workspace and item. Omit for the first page. Must be valid Unicode, not blank, without control characters and at most 8,192 UTF-8 bytes. Do not supply a continuation URL.")]
    public string? ContinuationToken { get; set; }
}
