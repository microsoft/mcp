// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.NetAppFiles.Models;

namespace Azure.Mcp.Tools.NetAppFiles.Services;

public interface INetAppFilesSnapshotService
{
    Task<NetAppFilesSnapshot> GetSnapshotAsync(
        string account,
        string pool,
        string volume,
        string snapshot,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<NetAppFilesSnapshot> CreateSnapshotAsync(
        string account,
        string pool,
        string volume,
        string snapshot,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<NetAppFilesSnapshot> UpdateSnapshotAsync(
        string account,
        string pool,
        string volume,
        string snapshot,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default);
}
