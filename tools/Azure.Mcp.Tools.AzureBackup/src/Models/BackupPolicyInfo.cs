// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record BackupPolicyInfo(
    string? Id,
    string Name,
    string VaultType,
    IReadOnlyList<string>? DatasourceTypes,
    int? ProtectedItemsCount);
