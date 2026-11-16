// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureMigrate.Commands.Assessment;

/// <summary>
/// Placeholder for the external assessment operation status API.
/// </summary>
public sealed class AssessmentOperationStatusClient : IAssessmentOperationStatusClient
{
    /// <inheritdoc/>
    public Task<AssessmentOperation> GetOperationAsync(Guid operationId, string? workspaceId, string token, CancellationToken cancellationToken) =>
        // TODO: Call the external assessment operation status API.
        throw new NotImplementedException("The assessment operation status API is not implemented yet.");
}
