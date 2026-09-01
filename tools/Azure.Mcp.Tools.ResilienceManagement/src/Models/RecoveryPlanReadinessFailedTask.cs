// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResilienceManagement.Models;

public sealed record RecoveryPlanReadinessFailedTask(
    string? TaskId,
    string? TaskName,
    string Status,
    RecoveryPlanReadinessError? Error);
