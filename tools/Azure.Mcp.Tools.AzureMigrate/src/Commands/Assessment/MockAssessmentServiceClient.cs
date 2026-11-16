// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Net;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.Assessment;

/// <summary>
/// POC mock of the Business Case Service machine assessment, used when <see cref="BusinessCaseAssessmentServiceClient.BaseUrlSettingName"/>
/// is not set. The PUT is accepted (HTTP 202) with an operation that reports Running for <see cref="RunningDuration"/> and Succeeded
/// afterwards, with a fake output URL. Operations are kept in memory, so they are lost when the server restarts and are not shared
/// between instances. The caller's token and the inventory SAS URL are not used.
/// </summary>
public sealed class MockAssessmentServiceClient(TimeProvider timeProvider) : IAssessmentServiceClient
{
    internal const string MockBaseUrl = "https://assessment.mock.local";
    internal static readonly TimeSpan RunningDuration = TimeSpan.FromSeconds(30);

    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly ConcurrentDictionary<Guid, (AssessmentRequest Request, DateTimeOffset CreatedAt)> _operations = new();

    /// <inheritdoc/>
    public Task<AssessmentOperation> CreateAssessmentAsync(AssessmentRequest request, string token, CancellationToken cancellationToken)
    {
        Guid operationId = Guid.NewGuid();
        _operations[operationId] = (request, _timeProvider.GetUtcNow());
        return Task.FromResult(new AssessmentOperation(
            (int)HttpStatusCode.Accepted, operationId, "Running", null, (int)RunningDuration.TotalSeconds, null, null));
    }

    /// <inheritdoc/>
    public Task<AssessmentOperation> GetOperationAsync(Guid operationId, string? workspaceId, string token, CancellationToken cancellationToken)
    {
        if (!_operations.TryGetValue(operationId, out var operation)
            || !string.Equals(operation.Request.WorkspaceId ?? string.Empty, workspaceId ?? string.Empty, StringComparison.Ordinal))
        {
            return Task.FromResult(new AssessmentOperation(
                (int)HttpStatusCode.NotFound, operationId, null, null, null, "OperationNotFound", "ZIP machine assessment operation not found."));
        }

        TimeSpan elapsed = _timeProvider.GetUtcNow() - operation.CreatedAt;
        bool completed = elapsed >= RunningDuration;
        return Task.FromResult(new AssessmentOperation(
            (int)HttpStatusCode.OK,
            operationId,
            completed ? "Succeeded" : "Running",
            completed ? OutputUrl(operation.Request) : null,
            completed ? null : (int)Math.Ceiling((RunningDuration - elapsed).TotalSeconds),
            null,
            null));
    }

    private static string OutputUrl(AssessmentRequest request) =>
        $"{MockBaseUrl}/assessment-output/{Uri.EscapeDataString(request.WorkspaceId ?? "default")}/" +
        $"{Uri.EscapeDataString(request.AssessmentName)}-recommendations.json?sp=r&sig=mock";
}
