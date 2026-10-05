// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureBackup.Models;

namespace Azure.Mcp.Tools.AzureBackup.Services;

public interface IRsvBackupOperationService
{
    Task<BackupOperationInfo> GetOperationAsync(
        string operation, string vault, string resourceGroup, string? subscription,
        string? container, string? protectedItem, string? fabric, string? tenant,
        CancellationToken cancellationToken);
}
