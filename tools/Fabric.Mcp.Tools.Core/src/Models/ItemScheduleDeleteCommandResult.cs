// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>A receipt for the requested schedule's deletion, not a returned schedule or job state.</summary>
public sealed record ItemScheduleDeleteCommandResult(
    Guid WorkspaceId,
    Guid ItemId,
    string JobType,
    Guid ScheduleId)
{
    public string DeletionStatus => "Deleted";

    public string Guidance =>
        "Fabric confirmed deletion of the requested schedule (HTTP 200). No schedule snapshot was returned. " +
        "Deleting a schedule does not cancel already-running jobs or roll back completed work. " +
        "Job cancellation requires a separate tool and separate authorization. " +
        "No follow-up request, polling, or automatic retry was performed. Do not resubmit this confirmed deletion.";
}
