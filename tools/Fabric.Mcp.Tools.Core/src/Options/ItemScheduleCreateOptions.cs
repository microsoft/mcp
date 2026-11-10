// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

public sealed class ItemScheduleCreateOptions()
{
    [Option(Description = "The explicit ID of the Microsoft Fabric workspace containing the item. Must be a nonempty UUID.")]
    public required string WorkspaceId { get; set; }

    [Option(Description = "The explicit ID of the Microsoft Fabric item to schedule. Must be a nonempty UUID.")]
    public required string ItemId { get; set; }

    [Option(Description = "The job type supported by this item. Required; no default or discovery. Encoded as one path component, within the 1,024 UTF-8 byte tool budget.")]
    public required string JobType { get; set; }

    [Option(Description = "Explicit true or false; no default. Enabling a schedule can consume compute or change data, and a past startDateTime may trigger a job immediately. Confirm this timing before consenting.")]
    public required bool Enabled { get; set; }

    [Option(Description = "Required JSON configuration object with type, startDateTime, endDateTime, and localTimeZoneId. Supply explicit UTC bounds (end after start) and a Windows time zone ID; offset-less API examples are treated as UTC, never machine-local time. Cron requires interval in minutes (1-5270400), not a cron expression. Daily/Weekly/Monthly require their documented timing fields. Extensible types and fields are preserved. At most 1 MiB UTF-8 and 32 nesting levels; the complete serialized request also has a 1 MiB budget.")]
    public required string Configuration { get; set; }

    [Option(Description = "Optional JSON object passed unchanged as executionData: fixed static, workload/job-type-defined inputs that may be sensitive. Omit to omit the field; no inference, stripping, or retry. At most 1 MiB UTF-8 and 32 nesting levels; the complete serialized request also has a 1 MiB budget.")]
    public string? ExecutionData { get; set; }
}
