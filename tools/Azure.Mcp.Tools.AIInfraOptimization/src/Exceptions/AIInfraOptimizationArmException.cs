// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AIInfraOptimization.Exceptions;

/// <summary>
/// A failed AI Infra Optimization ARM call carrying only sanitized, machine-actionable metadata. The message is a
/// static string chosen from the HTTP status; backend message text is never retained.
/// </summary>
internal sealed class AIInfraOptimizationArmException(
    int status,
    string? code,
    string? target,
    string? requestId,
    string? clientRequestId,
    string? retryAfter)
    : Exception(AIInfraOptimizationErrorMessages.ForStatus(status))
{
    public int Status { get; } = status;

    public string? Code { get; } = code;

    public string? Target { get; } = target;

    public string? RequestId { get; } = requestId;

    public string? ClientRequestId { get; } = clientRequestId;

    public string? RetryAfter { get; } = retryAfter;
}
