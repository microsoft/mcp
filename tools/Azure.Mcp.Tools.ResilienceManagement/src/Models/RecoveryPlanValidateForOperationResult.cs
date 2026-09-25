// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResilienceManagement.Models;

public sealed record RecoveryPlanValidateForOperationResult(
    string OperationId,
    string OperationName,
    bool IsValid,
    string? ErrorCode,
    string? ErrorMessage);
