// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

public sealed record ItemScheduleUpdateCommandResult(
    Guid WorkspaceId,
    Guid ItemId,
    string JobType,
    FabricItemSchedule Schedule,
    long? RetryAfterSeconds = null,
    DateTimeOffset? RetryAfterUtc = null)
{
    public string UpdateStatus => "Updated";

    public string Guidance =>
        "Fabric confirmed the schedule update, not job execution or success. An enabled schedule with a past start time may trigger a job immediately. " +
        "Disabling a schedule does not cancel existing job instances. The schedule owner identifies its creator or last modifier, separately from item ownership. " +
        "Schedule IDs are not job-instance or operation IDs. No polling, result retrieval, or automatic retry was performed. Do not resubmit this update blindly.";
}
