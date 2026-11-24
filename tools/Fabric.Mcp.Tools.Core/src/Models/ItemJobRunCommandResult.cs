// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>A submission receipt, never a claim about job execution or completion.</summary>
public sealed record ItemJobRunCommandResult(
    Guid WorkspaceId,
    Guid ItemId,
    string JobType,
    Guid? JobInstanceId,
    long? RetryAfterSeconds,
    DateTimeOffset? RetryAfterUtc,
    string? TrackingIssue,
    string? RetryAfterIssue)
{
    public string SubmissionStatus => "Accepted";

    public string Guidance => (JobInstanceId is null
        ? "Fabric accepted the submission but did not return a trustworthy job-instance ID. Inspect the item's job history in Fabric; do not resubmit to recover an ID."
        : "Fabric accepted the submission, not its completion. Use the returned IDs with jobs_get-item-job-instance in a separate call; do not resubmit.") +
        (RetryAfterSeconds is { } seconds
            ? FormattableString.Invariant($" Wait at least {seconds} seconds before the first status read.")
            : RetryAfterUtc is { } date
                ? FormattableString.Invariant($" Wait until at least {date.UtcDateTime:yyyy-MM-dd HH:mm:ss} UTC before the first status read.")
                : " No valid Retry-After guidance was supplied; the first-read delay is unknown.");
}
