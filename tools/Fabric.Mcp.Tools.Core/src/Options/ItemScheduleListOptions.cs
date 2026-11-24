// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

public sealed class ItemScheduleListOptions()
{
    [Option(Description = "The ID of the Microsoft Fabric workspace containing the item. Must be a nonempty UUID.")]
    public required string WorkspaceId { get; set; }

    [Option(Description = "The ID of the Microsoft Fabric item whose schedules should be listed. Must be a nonempty UUID.")]
    public required string ItemId { get; set; }

    [Option(Description = "The explicit job type supported by the item, such as DefaultJob. No discovery is performed. Must be not blank, valid Unicode without control characters or dot segments, within the 1,024 UTF-8 byte tool budget.")]
    public required string JobType { get; set; }

    [Option(Description = "The unchanged continuation token from a previous page for the same workspace, item, and job type. Omit for the first page. Must be valid Unicode, not blank, without control characters and at most 8,192 UTF-8 bytes. Do not supply a continuation URL.")]
    public string? ContinuationToken { get; set; }
}
