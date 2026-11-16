// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureMigrate.Commands.Assessment;

/// <summary>
/// POC client for the HA (app-centric) assessment PUT that runs after the Assessment Service assessment completes.
/// </summary>
public interface IHaAssessmentClient
{
    /// <summary>
    /// Sends the PUT HA assessment call for a completed assessment.
    /// </summary>
    Task<HaAssessmentResult> PutHaAssessmentAsync(AssessmentRequest request, string token, CancellationToken cancellationToken);
}

/// <summary>
/// Result of the PUT HA assessment call.
/// </summary>
public sealed record HaAssessmentResult(int? HttpStatus, string? Status, string? ErrorCode, string? ErrorMessage);

/// <summary>
/// POC mock of the PUT HA assessment call until the real API contract is available. It always succeeds with HTTP 201 and does
/// not use the caller's token.
/// </summary>
/// <remarks>
/// TODO: Replace with the real HA API client and confirm whether the PUT is synchronous or returns its own tracking operation.
/// </remarks>
public sealed class MockHaAssessmentClient : IHaAssessmentClient
{
    /// <inheritdoc/>
    public Task<HaAssessmentResult> PutHaAssessmentAsync(AssessmentRequest request, string token, CancellationToken cancellationToken) =>
        Task.FromResult(new HaAssessmentResult(201, "Succeeded", null, null));
}
