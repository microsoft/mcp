// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Compute.Models;

public sealed record VmssRollingUpgradeStatus(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("policy")] UpgradePolicyInfo? Policy,
    [property: JsonPropertyName("runningStatus")] RollingUpgradeRunningStatus? RunningStatus,
    [property: JsonPropertyName("progress")] RollingUpgradeProgressInfo? Progress,
    [property: JsonPropertyName("error")] string? Error);

public sealed record UpgradePolicyInfo(
    [property: JsonPropertyName("mode")] string? Mode,
    [property: JsonPropertyName("maxBatchInstancePercent")] int? MaxBatchInstancePercent,
    [property: JsonPropertyName("maxUnhealthyInstancePercent")] int? MaxUnhealthyInstancePercent,
    [property: JsonPropertyName("maxUnhealthyUpgradedInstancePercent")] int? MaxUnhealthyUpgradedInstancePercent,
    [property: JsonPropertyName("pauseTimeBetweenBatches")] string? PauseTimeBetweenBatches);

public sealed record RollingUpgradeRunningStatus(
    [property: JsonPropertyName("code")] string? Code,
    [property: JsonPropertyName("startTime")] DateTimeOffset? StartTime,
    [property: JsonPropertyName("lastAction")] string? LastAction,
    [property: JsonPropertyName("lastActionTime")] DateTimeOffset? LastActionTime);

public sealed record RollingUpgradeProgressInfo(
    [property: JsonPropertyName("successfulInstanceCount")] int? SuccessfulInstanceCount,
    [property: JsonPropertyName("failedInstanceCount")] int? FailedInstanceCount,
    [property: JsonPropertyName("inProgressInstanceCount")] int? InProgressInstanceCount,
    [property: JsonPropertyName("pendingInstanceCount")] int? PendingInstanceCount);
