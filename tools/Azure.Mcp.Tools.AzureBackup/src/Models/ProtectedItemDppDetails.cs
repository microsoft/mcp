// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>
/// DPP-specific protected-item properties returned by Azure Backup backup-instance APIs.
/// </summary>
public sealed record ProtectedItemDppDetails(
    string? FriendlyName,
    string? CurrentProtectionState,
    string? ProvisioningState,
    string? ValidationType,
    string? ObjectType,
    IReadOnlyList<string>? ResourceGuardOperationRequests,
    ProtectedItemDppDataSourceReference? DataSourceInfo,
    ProtectedItemDppDataSourceReference? DataSourceSetInfo,
    ProtectedItemDppPolicyInfo? PolicyInfo,
    ProtectedItemDppProtectionStatus? ProtectionStatus,
    ProtectedItemDppError? ResourceProtectionError,
    string? DataSourceAuthCredentialsType,
    ProtectedItemDppIdentityDetails? IdentityDetails);