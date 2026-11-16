// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Net;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.Assessment;

/// <summary>
/// POC mock of the Assessment Service backdoor until the real API is available. The PUT is accepted (HTTP 202) with a tracking
/// operation that reports Running for <see cref="RunningDuration"/> and Succeeded afterwards. Operations are kept in memory, so
/// they are lost when the server restarts and are not shared between instances. The caller's token is not used.
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
            (int)HttpStatusCode.Accepted, operationId, "Running", TrackingUrl(request.AssessmentName, operationId),
            (int)RunningDuration.TotalSeconds, null, null));
    }

    /// <inheritdoc/>
    public Task<AssessmentOperation> GetOperationAsync(string assessmentName, Guid operationId, string token, CancellationToken cancellationToken)
    {
        if (!_operations.TryGetValue(operationId, out var operation)
            || !string.Equals(operation.Request.AssessmentName, assessmentName, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new AssessmentOperation(
                (int)HttpStatusCode.NotFound, operationId, null, null, null, "OperationNotFound", "The assessment operation was not found."));
        }

        TimeSpan elapsed = _timeProvider.GetUtcNow() - operation.CreatedAt;
        bool completed = elapsed >= RunningDuration;
        return Task.FromResult(new AssessmentOperation(
            (int)HttpStatusCode.OK,
            operationId,
            completed ? "Succeeded" : "Running",
            TrackingUrl(assessmentName, operationId),
            completed ? null : (int)Math.Ceiling((RunningDuration - elapsed).TotalSeconds),
            null,
            null));
    }

    private static string TrackingUrl(string assessmentName, Guid operationId) =>
        $"{MockBaseUrl}/mcprestapi/assessments/{assessmentName}/operationstatus/{operationId:D}";
}
