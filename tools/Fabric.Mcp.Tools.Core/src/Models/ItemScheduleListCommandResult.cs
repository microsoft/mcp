// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>One complete schedule page, with a token for an explicitly requested next page.</summary>
public sealed record ItemScheduleListCommandResult(
    IReadOnlyList<FabricItemSchedule> Schedules,
    string? ContinuationToken = null,
    long? RetryAfterSeconds = null,
    DateTimeOffset? RetryAfterUtc = null);
