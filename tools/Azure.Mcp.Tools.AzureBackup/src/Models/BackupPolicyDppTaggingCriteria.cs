// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>
/// Tagging criteria returned by DPP backup-policy backup rules.
/// </summary>
public sealed record BackupPolicyDppTaggingCriteria(
    string? TagName,
    bool? IsDefault,
    long? TaggingPriority,
    IReadOnlyList<string>? Criteria);
