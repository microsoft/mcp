// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>A cancellation-request receipt, not confirmation of the job's final state.</summary>
public sealed record ItemJobInstanceCancelCommandResult(
    Guid WorkspaceId,
    Guid ItemId,
    Guid JobInstanceId,
    long? RetryAfterSeconds,
    DateTimeOffset? RetryAfterUtc,
    string? RetryAfterIssue)
{
    public string CancellationStatus => "Requested";

    public string Guidance => "Fabric accepted the cancellation request, not confirmation that the job is Cancelled. " +
        "Use these IDs with jobs_get-item-job-instance in a separate call to check its status; do not automatically resubmit. " +
        "Cancellation does not guarantee rollback of work already performed." +
        (RetryAfterSeconds is { } seconds
            ? FormattableString.Invariant($" Wait at least {seconds} seconds before the first status read.")
            : RetryAfterUtc is { } date
                ? FormattableString.Invariant($" Wait until at least {date.UtcDateTime:yyyy-MM-dd HH:mm:ss} UTC before the first status read.")
                : " No valid Retry-After guidance was supplied; the first-read delay is unknown.");
}
