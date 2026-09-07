// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

// This contract intentionally mirrors the currently supported Azure Backup SDK
// protected-item properties for RSV. Revisit it whenever Azure.ResourceManager
// SDK packages used by Azure Backup are upgraded.
/// <summary>
/// Workload-specific details returned by RSV protected-item APIs.
/// Properties are populated when supported by the protected-item workload type.
/// </summary>
public sealed record ProtectedItemDetails(
    string? BackupManagementType,
    string? WorkloadType,
    DateTimeOffset? LastRecoverOn,
    string? BackupSetName,
    string? CreateMode,
    DateTimeOffset? DeferredDeletedOn,
    bool? IsScheduledForDeferredDelete,
    string? DeferredDeleteTimeRemaining,
    bool? IsDeferredDeleteScheduleUpcoming,
    bool? IsRehydrate,
    IReadOnlyList<string>? ResourceGuardOperationRequests,
    bool? IsArchiveEnabled,
    string? PolicyName,
    int? SoftDeleteRetentionPeriodInDays,
    string? VaultId,
    string? FriendlyName,
    string? VirtualMachineId,
    string? ProtectionStatus,
    string? ProtectionState,
    string? HealthStatus,
    IReadOnlyList<ProtectedItemHealthDetails>? HealthDetails,
    IReadOnlyDictionary<string, ProtectedItemKpiHealthDetails>? KpisHealths,
    string? LastBackupStatus,
    string? ProtectedItemDataId,
    string? PolicyType,
    DateTimeOffset? LastBackupOn,
    DateTimeOffset? OldestRecoverOn,
    DateTimeOffset? OldestRecoveryPointInVault,
    DateTimeOffset? OldestRecoveryPointInArchive,
    DateTimeOffset? NewestRecoveryPointInArchive,
    int? RecoveryPointCount,
    bool? IsPolicyInconsistent,
    ProtectedItemExtendedProperties? ExtendedProperties);