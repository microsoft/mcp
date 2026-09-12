// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

internal static class AzureBackupPrivateEndpointGroupIdExtensions
{
    internal static string ToValue(this AzureBackupPrivateEndpointGroupId groupId) => groupId switch
    {
        AzureBackupPrivateEndpointGroupId.Primary => "AzureBackup",
        AzureBackupPrivateEndpointGroupId.Secondary => "AzureBackup_secondary",
        _ => throw new ArgumentOutOfRangeException(nameof(groupId), groupId, null)
    };
}
