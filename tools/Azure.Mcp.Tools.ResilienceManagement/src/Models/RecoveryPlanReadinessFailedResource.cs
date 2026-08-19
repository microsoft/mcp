// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResilienceManagement.Models;

public sealed record RecoveryPlanReadinessFailedResource(
    string Id,
    string? ResourceId,
    string Status,
    string? TaskName,
    RecoveryPlanReadinessError? Error);