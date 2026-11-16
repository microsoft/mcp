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
}

/// <summary>
/// Read SAS URLs the HA skill downloads its input from: the user's inventory and the assessment output.
/// </summary>
public sealed record HaInput(string InventorySasUri, string AssessmentSasUri);

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
