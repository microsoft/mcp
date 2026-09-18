// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>
/// Target-data-store copy setting returned by DPP backup-policy retention lifecycles.
/// </summary>
public sealed record BackupPolicyDppCopySetting(
    string? DataStoreType,
    string? CopyAfterType);
