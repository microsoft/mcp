// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>
/// Vault soft delete state.
/// <para>
/// <c>Off</c> — Soft delete disabled. Deleted backups are purged immediately.
/// </para>
/// <para>
/// <c>On</c> — Soft delete enabled with the configured retention period. Vault admins can disable it.
/// </para>
/// <para>
/// <c>AlwaysOn</c> — <b>Irreversible.</b> Soft delete cannot be disabled after this is set.
/// Confirm before use.
/// </para>
/// </summary>
public enum AzureBackupSoftDeleteState
{
    Off,
    On,
    AlwaysOn,
}
