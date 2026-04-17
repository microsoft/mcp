// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record ProtectedItemInfo(
    string? Id,
    string Name,
    string VaultType,
    string? ProtectionStatus,
    string? DatasourceType,
    string? DatasourceId,
    string? PolicyName,
    DateTimeOffset? LastBackupTime,
    string? ContainerName);
