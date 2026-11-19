// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureMigrate.Commands.Assessment;

/// <summary>
/// POC client that starts the HA skill on the Discovery Service MCP backdoor after an assessment completes.
/// </summary>
public interface IHaSessionClient
{
    /// <summary>
    /// Creates the caller's HA session, invokes it and uploads the HA input mini zip (see <see cref="HaInputPackage"/>) to the
    /// write-only SAS URL the invoke returns.
    /// </summary>
    Task<HaSessionResult> StartAsync(string token, HaInput input, CancellationToken cancellationToken);

    /// <summary>
    /// Like <see cref="StartAsync"/>, but in the session <paramref name="sessionId"/>, which the business case derives from its
    /// assessment operation so that repeated calls address the same session. Another active session of the caller is reported as a
    /// failure of the 'createSession' step (HTTP 409) instead of being reused.
    /// </summary>
    Task<HaSessionResult> StartInSessionAsync(string token, HaInput input, string sessionId, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the status of an HA operation (GET /mcprestapi/sessions/{id}/operationstatus/{operationId}) once.
    /// </summary>
    Task<HaOperationStatus> GetOperationStatusAsync(string token, string sessionId, Guid operationId, CancellationToken cancellationToken);
}

/// <summary>
/// Read SAS URLs the HA skill downloads its input from: the user's inventory and the assessment output and, for a business case,
/// the Business Case Service on-premises TCO manifest and the HA evaluation manifest.
/// </summary>
public sealed record HaInput(string InventorySasUri, string AssessmentSasUri, string? BusinessCaseSasUri = null, string? HaSasUri = null)
{
    /// <inheritdoc/>
    public override string ToString() => "HaInput { SAS URLs hidden }";
}

/// <summary>
/// One read of an HA operation status. <see cref="HttpStatus"/> is null on a network error; <see cref="Error"/> is set for any
/// non-success response.
/// </summary>
public sealed record HaOperationStatus(
    int? HttpStatus,
    string? Status,
    string? Result,
    string? ErrorCode,
    string? ErrorMessage,
    string? Error,
    int? RetryAfterSeconds);

/// <summary>
/// Result of starting the HA session. <see cref="FailedStep"/> is null on success.
/// </summary>
public sealed record HaSessionResult(
    string? FailedStep,
    int? HttpStatus,
    string? SessionId,
    bool ReusedExistingSession,
    string? OperationId,
    string? ClientRequestId,
    string? Error);
