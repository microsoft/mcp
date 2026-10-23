// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>
/// Source lifecycle (retention) rule returned by DPP backup-policy retention rules.
/// </summary>
public sealed record BackupPolicyDppLifecycle(
    string? SourceDataStoreType,
    string? DeleteAfterDuration,
    string? DeleteAfterType,
    IReadOnlyList<BackupPolicyDppCopySetting>? TargetCopySettings);
