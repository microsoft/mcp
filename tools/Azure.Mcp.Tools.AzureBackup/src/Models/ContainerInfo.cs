// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record ContainerInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("friendlyName")] string? FriendlyName,
    [property: JsonPropertyName("registrationStatus")] string? RegistrationStatus,
    [property: JsonPropertyName("healthStatus")] string? HealthStatus,
    [property: JsonPropertyName("protectableObjectType")] string? ProtectableObjectType,
    [property: JsonPropertyName("backupManagementType")] string? BackupManagementType,
    [property: JsonPropertyName("sourceResourceId")] string? SourceResourceId,
    [property: JsonPropertyName("workloadType")] string? WorkloadType,
    [property: JsonPropertyName("lastUpdatedTime")] DateTimeOffset? LastUpdatedTime);
