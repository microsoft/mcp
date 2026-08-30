// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>
/// Vault immutability state for both Recovery Services vaults and Backup vaults.
/// <para>
/// <c>Disabled</c> — Immutability is off. This is the initial state for new vaults.
/// </para>
/// <para>
/// <c>Unlocked</c> — Immutability is on but can still be disabled by a vault admin.
/// This is the ARM-canonical value used by both RSV and DPP APIs.
/// </para>
/// <para>
/// <c>Enabled</c> — Backward-compatible alias for <c>Unlocked</c>. Older tool users
/// requested <c>Enabled</c>; the service normalises it to <c>Unlocked</c> before calling ARM.
/// </para>
/// <para>
/// <c>Locked</c> — <b>Irreversible.</b> Once locked, immutability cannot be disabled and the
/// duration cannot be reduced. Confirm before use.
/// </para>
/// </summary>
public enum AzureBackupImmutabilityState
{
    Disabled,
    Unlocked,
    Enabled,
    Locked,
}
