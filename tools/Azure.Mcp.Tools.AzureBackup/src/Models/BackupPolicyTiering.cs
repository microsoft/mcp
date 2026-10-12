// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>
/// Tiering policy details returned by RSV backup policies, keyed by the source recovery-point tier.
/// </summary>
public sealed record BackupPolicyTiering(
    string SourceTier,
    string? TieringMode,
    int? DurationValue,
    string? DurationType);
