// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>
/// DPP-specific details returned by Azure Backup backup-policy APIs.
/// </summary>
public sealed record BackupPolicyDppDetails(
    IReadOnlyList<string>? DataSourceTypes,
    string? ObjectType,
    IReadOnlyList<BackupPolicyDppRule>? Rules);
