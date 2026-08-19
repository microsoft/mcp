// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResilienceManagement.Models;

public sealed record RecoveryPlanReadinessResult(
    string OperationId,
    string RecoveryJobId,
    bool? IsReady,
    string Status,
    RecoveryPlanReadinessError? Error,
    List<RecoveryPlanReadinessFailedTask> FailedTasks,
    List<RecoveryPlanReadinessFailedResource> FailedResources);