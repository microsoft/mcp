// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record BackupJobInfo(
    string? Id,
    string Name,
    string VaultType,
    string? Operation,
    string? Status,
    DateTimeOffset? StartTime,
    DateTimeOffset? EndTime,
    string? DatasourceType,
    string? DatasourceName);
