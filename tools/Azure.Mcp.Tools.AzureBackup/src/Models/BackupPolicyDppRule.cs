// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>
/// A single policy rule (backup rule or retention rule) returned by DPP rule-based backup policies.
/// Properties are populated when supported by the rule type.
/// </summary>
public sealed record BackupPolicyDppRule(
    string? Name,
    string? ObjectType,
    bool? IsDefault,
    string? BackupType,
    string? DataStoreType,
    string? ScheduleTimeZone,
    IReadOnlyList<string>? RepeatingTimeIntervals,
    IReadOnlyList<BackupPolicyDppTaggingCriteria>? TaggingCriteria,
    IReadOnlyList<BackupPolicyDppLifecycle>? Lifecycles);
