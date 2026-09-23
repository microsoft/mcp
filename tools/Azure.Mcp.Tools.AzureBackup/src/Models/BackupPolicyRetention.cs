// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>
/// Retention policy details returned by RSV backup policies.
/// Properties are populated when supported by the retention policy type.
/// </summary>
public sealed record BackupPolicyRetention(
    string? RetentionPolicyType,
    int? SimpleRetentionDurationCount,
    string? SimpleRetentionDurationType,
    IReadOnlyList<BackupPolicyRetentionSchedule>? Schedules);
