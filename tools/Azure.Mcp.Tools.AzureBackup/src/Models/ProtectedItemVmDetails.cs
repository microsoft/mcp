// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

// This contract intentionally mirrors the currently supported RSV SDK protected-item
// properties. Revisit it whenever Azure.ResourceManager.RecoveryServicesBackup is upgraded.
/// <summary>
/// VM-specific properties returned by the Recovery Services protected-item API.
/// </summary>
public sealed record ProtectedItemVmDetails(
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
    string? VmPolicyName,
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

public sealed record ProtectedItemHealthDetails(
    int? Code,
    string? Title,
    string? Message,
    IReadOnlyList<string>? Recommendations);

public sealed record ProtectedItemKpiHealthDetails(
    string? ResourceHealthStatus,
    IReadOnlyList<ProtectedItemHealthDetails>? ResourceHealthDetails);