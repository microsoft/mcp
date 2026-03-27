// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record ProtectedItemInfo(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("vaultType")] string VaultType,
    [property: JsonPropertyName("protectionStatus")] string? ProtectionStatus,
    [property: JsonPropertyName("datasourceType")] string? DatasourceType,
    [property: JsonPropertyName("datasourceId")] string? DatasourceId,
    [property: JsonPropertyName("policyName")] string? PolicyName,
    [property: JsonPropertyName("lastBackupTime")] DateTimeOffset? LastBackupTime,
    [property: JsonPropertyName("containerName")] string? ContainerName);
