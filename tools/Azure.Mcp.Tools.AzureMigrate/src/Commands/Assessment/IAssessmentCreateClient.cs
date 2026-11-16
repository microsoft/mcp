// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureMigrate.Commands.Assessment;

/// <summary>
/// Client for the external API that starts an Azure Migrate machine assessment from a collector ZIP. Creating an assessment is
/// asynchronous: the call returns an operation that is checked with <see cref="IAssessmentOperationStatusClient"/>.
/// </summary>
public interface IAssessmentCreateClient
{
    /// <summary>
    /// Starts the assessment and returns its operation.
    /// </summary>
    Task<AssessmentOperation> CreateAssessmentAsync(AssessmentRequest request, string token, CancellationToken cancellationToken);
}

/// <summary>
/// Inputs of the assessment create call. <see cref="InventorySasUri"/> is a read SAS URL, so it is left out of <see cref="ToString"/>.
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
