// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResilienceManagement.Models;

public sealed record RecoveryPlanFailoverResult(
    string OperationId,
    string? JobId,
    string Status,
    string Message,
    bool HasCompleted,
    int HttpStatus,
    string? LocationUri,
    string? AzureAsyncOperationUri,
    string? RequestId,
    string? ClientRequestId,
    string? CorrelationId);
