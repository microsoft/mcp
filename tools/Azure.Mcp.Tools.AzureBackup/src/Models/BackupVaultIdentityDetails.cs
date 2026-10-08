// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record BackupVaultIdentityDetails(
    string? PrincipalId,
    string? TenantId,
    string? Type,
    IReadOnlyList<BackupVaultUserAssignedIdentity>? UserAssignedIdentities);
