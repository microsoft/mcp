// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.Assessment;

/// <summary>
/// Reads an assessment operation on the Assessment Service:
/// GET /businessCaseRestApi/machineAssessmentsFromZip/operations/{operationId}?workspaceId=..., which returns the status and, once
/// succeeded, a fresh read-only SAS URL of the recommendations JSON.
/// </summary>
public sealed class AssessmentOperationStatusClient(ILogger<AssessmentOperationStatusClient> logger) : IAssessmentOperationStatusClient
{
    private readonly ILogger<AssessmentOperationStatusClient> _logger = logger;

    /// <inheritdoc/>
    public Task<AssessmentOperation> GetOperationAsync(Guid operationId, string? workspaceId, string token, CancellationToken cancellationToken) =>
        AssessmentApi.SendAsync(
            _logger,
            HttpMethod.Get,
            AssessmentApi.OperationUrl(AssessmentApi.BaseUrl, operationId, AssessmentApi.ResolveWorkspaceId(workspaceId)),
            token,
            null,
            "operation status",
            cancellationToken);
}
