// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>A job-instance snapshot, not a Fabric long-running operation receipt.</summary>
public sealed record FabricItemJobInstance()
{
    public required Guid Id { get; init; }
    public required Guid ItemId { get; init; }
    public required string JobType { get; init; }
    public required string InvokeType { get; init; }
    public required string Status { get; init; }
    public Guid? RootActivityId { get; init; }

    // The API declares plain UTC strings and demonstrates values without an offset. Preserve them without local-time conversion.
    public string? StartTimeUtc { get; init; }
    public string? EndTimeUtc { get; init; }
    public FabricJobFailure? FailureReason { get; init; }
}
