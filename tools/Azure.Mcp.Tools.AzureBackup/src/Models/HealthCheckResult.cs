// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record HealthCheckResult(
    string? VaultName,
    string? VaultType,
    int TotalProtectedItems,
    int HealthyItems,
    int UnhealthyItems,
    int ItemsBreachingRpo,
    string? SoftDeleteState,
    string? ImmutabilityState,
    string? EncryptionType,
    IReadOnlyList<HealthCheckItemDetail>? Details);

public sealed record HealthCheckItemDetail(
    string? Name,
    string? ProtectionStatus,
    string? HealthStatus,
    DateTimeOffset? LastBackupTime,
    bool RpoBreached);
