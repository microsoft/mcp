// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

public sealed class ItemScheduleGetOptions()
{
    [Option(Description = "The explicit ID of the Microsoft Fabric workspace containing the item. Must be a nonempty UUID.")]
    public required string WorkspaceId { get; set; }

    [Option(Description = "The explicit ID of the Microsoft Fabric item whose schedule is being read. Must be a nonempty UUID.")]
    public required string ItemId { get; set; }

    [Option(Description = "The job type supported by this item. Required; no default or discovery. Encoded as one path component, within the 1,024 UTF-8 byte tool budget.")]
    public required string JobType { get; set; }

    [Option(Description = "The explicit ID of the existing item schedule to read. Must be a nonempty UUID, not a job-instance or operation ID. No lookup is performed.")]
    public required string ScheduleId { get; set; }
}
