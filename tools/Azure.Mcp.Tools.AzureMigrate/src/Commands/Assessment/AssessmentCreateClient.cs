// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureMigrate.Commands.Assessment;

/// <summary>
/// Placeholder for the external assessment create API.
/// </summary>
public sealed class AssessmentCreateClient : IAssessmentCreateClient
{
    /// <inheritdoc/>
    public Task<AssessmentOperation> CreateAssessmentAsync(AssessmentRequest request, string token, CancellationToken cancellationToken) =>
        // TODO: Call the external assessment create API.
        throw new NotImplementedException("The assessment create API is not implemented yet.");
}
