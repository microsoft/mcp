// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>
/// Result of an <c>azurebackup protecteditem protect</c> call.
/// </summary>
/// <param name="Status">
/// Final outcome of the protect operation as observed by MCP after polling.
/// For RSV: terminal status of the ConfigureBackup job (e.g. <c>Completed</c>,
/// <c>CompletedWithWarnings</c>, <c>Failed</c>, <c>Cancelled</c>) or <c>InProgress</c>
/// if the job is still running when the polling budget is exhausted.
/// For DPP: <c>Succeeded</c> when the backup instance reaches <c>ProtectionConfigured</c>
/// (or <c>ConfiguringProtection</c> if still finalizing) or <c>Failed</c> on error.
/// </param>
/// <param name="ProtectedItemName">
/// RSV protected item name or DPP backup instance name. Use this with
/// <c>azurebackup protecteditem get</c>.
/// </param>
/// <param name="JobId">
/// RSV ConfigureBackup job id (use with <c>azurebackup job get</c>). Only a genuine
/// backup job id is returned here; when the service does not surface a job this is
/// <c>null</c> and <see cref="OperationId"/> carries the async operation id instead.
/// Always <c>null</c> for DPP  -  DPP protection is not surfaced as a job; verify with
/// <c>azurebackup protecteditem get</c> or <c>list</c>.
/// </param>
/// <param name="Message">Human-readable summary of the outcome.</param>
/// <param name="ProtectionStatus">
/// DPP only  -  actual <c>protectionStatus.status</c> read back from the backup
/// instance after the operation (e.g. <c>ProtectionConfigured</c>,
/// <c>ConfiguringProtection</c>, <c>ProtectionError</c>).
/// </param>
/// <param name="ErrorMessage">
/// Error detail when <see cref="Status"/> is <c>Failed</c>. For RSV this comes
/// from the failed ConfigureBackup job; for DPP it comes from the
/// async <c>operationStatus</c> error envelope.
/// </param>
/// <param name="OperationId">
/// Azure async operation id returned by the protected-item PUT (from the
/// <c>Azure-AsyncOperation</c> response header). This tracks the control-plane request
/// itself and is <b>not</b> a backup job id  -  it cannot be queried with
/// <c>azurebackup job get</c>. Populated when the service returns an async operation id;
/// may be <c>null</c>.
/// </param>
public sealed record ProtectResult(
    string Status,
    string? ProtectedItemName,
    string? JobId,
    string? Message,
    string? ProtectionStatus = null,
    string? ErrorMessage = null,
    string? OperationId = null);
