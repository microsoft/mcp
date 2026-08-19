// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>
/// Represents a Microsoft.DataProtection/resourceGuards resource used to protect
/// backup vaults (both RSV and DPP) via Multi-User Authorization (MUA).
/// </summary>
public sealed record ResourceGuardInfo(
    string Id,
    string Name,
    string Location,
    string ResourceGroup,
    IReadOnlyList<string> VaultCriticalOperationExclusionList,
    IReadOnlyList<string> ProtectedOperations,
    IReadOnlyDictionary<string, string>? Tags = null,
    string? ProvisioningState = null,
    string? Description = null);
