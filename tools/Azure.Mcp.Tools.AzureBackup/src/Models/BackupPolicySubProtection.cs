// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>
/// Sub-protection policy (Full, Differential, Incremental, or Log) returned by RSV workload backup policies.
/// </summary>
public sealed record BackupPolicySubProtection(
    string? PolicyType,
    BackupPolicySchedule? SchedulePolicy,
    BackupPolicyRetention? RetentionPolicy,
    IReadOnlyList<BackupPolicyTiering>? TieringPolicies,
    int? SnapshotInstantRPRetentionRangeInDays);
