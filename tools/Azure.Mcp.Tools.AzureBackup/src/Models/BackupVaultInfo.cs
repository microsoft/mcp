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
    IDictionary<string, string>? Tags,
    // Expanded fields (populated only when 'vault get --expand' selects them). All optional
    // and defaulted to null so existing callers and record construction remain backward compatible.
    string? MuaState = null,
    string? MuaResourceGuardId = null,
    string? CrossRegionRestoreState = null,
    string? EncryptionState = null,
    string? EncryptionKeyUri = null);
