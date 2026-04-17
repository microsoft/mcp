// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record BackupVaultInfo(
    string? Id,
    string Name,
    string VaultType,
    string? Location,
    string? ResourceGroup,
    string? ProvisioningState,
    string? SkuName,
    string? StorageType,
    string? Redundancy,
    string? SoftDeleteState,
    int? SoftDeleteRetentionDays,
    string? ImmutabilityState,
    string? IdentityType,
    IDictionary<string, string>? Tags);
