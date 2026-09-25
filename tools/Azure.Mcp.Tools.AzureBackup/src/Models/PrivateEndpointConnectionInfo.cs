// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>
/// Summary of a Private Endpoint Connection attached to a Recovery Services vault.
/// Populated from either the vault properties (list path) or the tracked
/// <c>Microsoft.RecoveryServices/vaults/privateEndpointConnections</c> resource
/// (single-connection path).
/// </summary>
public sealed record PrivateEndpointConnectionInfo(
    string? Id,
    string Name,
    string? PrivateEndpointId,
    IReadOnlyList<string>? GroupIds,
    string? ProvisioningState,
    string? ConnectionStatus,
    string? Description,
    string? ActionsRequired);
