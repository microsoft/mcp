// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record BackupJobInfo(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("vaultType")] string VaultType,
    [property: JsonPropertyName("operation")] string? Operation,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("startTime")] DateTimeOffset? StartTime,
    [property: JsonPropertyName("endTime")] DateTimeOffset? EndTime,
    [property: JsonPropertyName("datasourceType")] string? DatasourceType,
    [property: JsonPropertyName("datasourceName")] string? DatasourceName);
