// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureMigrate.Commands.Assessment;

/// <summary>
/// POC client that starts the HA skill on the Discovery Service MCP backdoor after an assessment completes.
/// </summary>
public interface IHaSessionClient
{
    /// <summary>
    /// Creates (or reuses) the caller's HA session and invokes it, returning the write-only SAS URL where the HA input must be uploaded.
    /// </summary>
    Task<HaSessionResult> StartAsync(string token, CancellationToken cancellationToken);
}

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
    string? UploadUrl,
    string? Error);
