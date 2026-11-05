// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

public sealed record ItemScheduleCreateCommandResult(
    Guid WorkspaceId,
    Guid ItemId,
    string JobType,
    FabricItemSchedule Schedule,
    long? RetryAfterSeconds = null,
    DateTimeOffset? RetryAfterUtc = null)
{
    public string CreationStatus => "Created";

    public string Guidance =>
        "Fabric confirmed schedule creation, not job execution or success. An enabled schedule with a past start time may trigger a job immediately. " +
        "Schedule IDs are not job-instance or operation IDs. No polling, result retrieval, or automatic retry was performed. Do not resubmit this creation.";
}
