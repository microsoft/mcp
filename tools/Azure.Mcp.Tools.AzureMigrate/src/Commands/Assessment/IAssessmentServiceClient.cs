// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureMigrate.Commands.Assessment;

/// <summary>
/// POC client for the Azure Migrate Assessment Service backdoor. Creating an assessment is asynchronous: the PUT returns a
/// tracking operation that is checked until it reaches a terminal state.
/// </summary>
public interface IAssessmentServiceClient
{
    /// <summary>
    /// Sends the backdoor PUT for the assessment and returns the tracking operation.
    /// </summary>
    Task<AssessmentOperation> CreateAssessmentAsync(AssessmentRequest request, string token, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the current state of a tracking operation returned by <see cref="CreateAssessmentAsync"/>.
    /// </summary>
    Task<AssessmentOperation> GetOperationAsync(string assessmentName, Guid operationId, string token, CancellationToken cancellationToken);
}

/// <summary>
/// Body of the backdoor PUT assessment call.
/// </summary>
public sealed record AssessmentRequest(string AssessmentName, string TargetRegion, string Currency);

/// <summary>
/// State of an assessment tracking operation, or the error returned by the Assessment Service.
/// </summary>
public sealed record AssessmentOperation(
    int? HttpStatus,
    Guid? OperationId,
    string? Status,
    string? TrackingUrl,
    int? RetryAfterSeconds,
    string? ErrorCode,
    string? ErrorMessage);
