// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Monitor.Models.HealthModels;

/// <summary>What to read from one health model: which entities, and what to add to each.</summary>
public sealed record HealthModelInspectRequest(
    string? Entity,
    HealthModelHealthFilter? WhereHealth,
    HealthModelWalk? Walk,
    IReadOnlyCollection<HealthModelInclude> Include,
    DateTimeOffset? StartAt,
    DateTimeOffset? EndAt,
    DateTimeOffset? Timestamp,
    int Top,
    int Skip);
