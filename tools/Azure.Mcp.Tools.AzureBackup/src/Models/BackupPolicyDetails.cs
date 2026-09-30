// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

// This contract intentionally mirrors the currently supported Azure Backup SDK
// protection-policy properties for RSV. Revisit it whenever Azure.ResourceManager
// SDK packages used by Azure Backup are upgraded.
/// <summary>
/// Workload-specific details returned by RSV backup-policy APIs.
/// Properties are populated when supported by the backup-policy workload type.
/// </summary>
public sealed record BackupPolicyDetails(
    string? BackupManagementType,
    string? WorkloadType,
    int? ProtectedItemsCount,
    IReadOnlyList<string>? ResourceGuardOperationRequests,
    string? TimeZone,
    string? PolicyType,
    string? SnapshotConsistencyType,
    int? InstantRPRetentionRangeInDays,
    string? InstantRPResourceGroupNamePrefix,
    string? InstantRPResourceGroupNameSuffix,
    bool? MakePolicyConsistent,
    BackupPolicyWorkloadSettings? Settings,
    BackupPolicySchedule? SchedulePolicy,
    BackupPolicyRetention? RetentionPolicy,
    IReadOnlyList<BackupPolicyTiering>? TieringPolicies,
    IReadOnlyList<BackupPolicySubProtection>? SubProtectionPolicies);
