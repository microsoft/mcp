// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>Managed identity configurations supported by backup vault updates.</summary>
public enum AzureBackupManagedIdentityType
{
    /// <summary>Enable only a system-assigned identity.</summary>
    SystemAssigned,

    /// <summary>Enable only user-assigned identities.</summary>
    UserAssigned,

    /// <summary>Enable system-assigned and user-assigned identities.</summary>
    [JsonStringEnumMemberName("SystemAssigned,UserAssigned")]
    SystemAssignedUserAssigned,

    /// <summary>Disable managed identities.</summary>
    None
}
