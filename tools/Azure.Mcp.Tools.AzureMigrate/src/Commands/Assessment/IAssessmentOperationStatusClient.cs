// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureMigrate.Commands.Assessment;

/// <summary>
/// Client for the external API that reads the state of an assessment operation started with <see cref="IAssessmentCreateClient"/>.
/// </summary>
public interface IAssessmentOperationStatusClient
{
    /// <summary>
    /// Reads the current state of an assessment operation.
    /// </summary>
    Task<AssessmentOperation> GetOperationAsync(Guid operationId, string? workspaceId, string token, CancellationToken cancellationToken);
}

/// <summary>
/// State of an assessment operation, or the error returned by the service. <see cref="OutputUrl"/> is set only when the
/// operation succeeded: a temporary read-only SAS URL of the assessment output.
/// </summary>
public sealed record AssessmentOperation(
    int? HttpStatus,
    Guid? OperationId,
    string? Status,
    string? OutputUrl,
    int? RetryAfterSeconds,
    string? ErrorCode,
    string? ErrorMessage);
