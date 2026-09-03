// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>
/// Immutability duration mode. Required on api-version 2026-05-01+ whenever the immutability
/// state is not <c>Disabled</c>. Introduced to the SDK payload contract in
/// <c>Azure.ResourceManager.RecoveryServices 1.3.0</c>.
/// </summary>
public enum AzureBackupImmutabilityType
{
    /// <summary>
    /// Recovery-point retention comes from the backup policy. Use this for most workloads
    /// (VMs, files, workload backups) where policy-driven retention is desired.
    /// </summary>
    AsPerPolicy,

    /// <summary>
    /// Recovery points are immutable for a fixed number of days regardless of policy.
    /// Requires <c>ImmutabilityDurationDays</c> in the 30–36135 range.
    /// </summary>
    TimeBased,
}
