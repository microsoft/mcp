// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureMigrate.Commands.Assessment;

/// <summary>
/// POC client for the Business Case Service machine assessment from a collector ZIP
/// (/businessCaseRestApi/machineAssessmentsFromZip). Creating an assessment is asynchronous: the PUT returns an operation that
/// is checked until it reaches a terminal state.
/// </summary>
public interface IAssessmentServiceClient
{
    /// <summary>
    /// Starts the assessment (PUT) and returns its operation.
    /// </summary>
    Task<AssessmentOperation> CreateAssessmentAsync(AssessmentRequest request, string token, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the current state of an operation returned by <see cref="CreateAssessmentAsync"/>.
    /// </summary>
    Task<AssessmentOperation> GetOperationAsync(Guid operationId, string? workspaceId, string token, CancellationToken cancellationToken);
}

/// <summary>
/// Inputs of the assessment PUT. <see cref="InventorySasUri"/> is a read SAS URL, so it is left out of <see cref="ToString"/>.
/// </summary>
public sealed record AssessmentRequest(
    string AssessmentName,
    string? WorkspaceId,
    string InventorySasUri,
    string TargetRegion,
    string Currency)
{
    /// <inheritdoc/>
    public override string ToString() =>
        $"AssessmentRequest {{ AssessmentName = {AssessmentName}, WorkspaceId = {WorkspaceId}, TargetRegion = {TargetRegion}, Currency = {Currency} }}";
}

/// <summary>
/// State of an assessment operation, or the error returned by the service. <see cref="OutputUrl"/> is set only when the
/// operation succeeded: a temporary read-only SAS URL of the recommendations JSON.
/// </summary>
public sealed record AssessmentOperation(
    int? HttpStatus,
    Guid? OperationId,
    string? Status,
    string? OutputUrl,
    int? RetryAfterSeconds,
    string? ErrorCode,
    string? ErrorMessage);
