// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>
/// Action to apply to a pending Private Endpoint Connection on a Recovery Services vault.
/// </summary>
public enum PrivateEndpointConnectionAction
{
    /// <summary>Approve the Private Endpoint Connection.</summary>
    Approve,

    /// <summary>Reject the Private Endpoint Connection.</summary>
    Reject,
}
