// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>
/// Snapshot of a private endpoint connection attached to a backup vault. Populated by
/// 'vault get --expand network' for RSV and DPP vaults. All fields are best-effort and
/// may be null when the underlying SDK model does not carry them.
/// </summary>
public sealed record PrivateEndpointConnectionInfo(
    string? Id,
    string? Name,
    string? PrivateEndpointId,
    IReadOnlyList<string>? GroupIds,
    string? ProvisioningState,
    string? ConnectionStatus,
    string? Description,
    string? ActionsRequired);
