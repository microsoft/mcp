// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.NetAppFiles.Models;

namespace Azure.Mcp.Tools.NetAppFiles.Services;

public interface INetAppFilesVolumeService
{
    Task<NetAppFilesVolume> GetVolumeAsync(
        string account,
        string pool,
        string volume,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<NetAppFilesVolume> CreateVolumeAsync(
        string account,
        string pool,
        string volume,
        string location,
        string subnetId,
        long quotaGib,
        string serviceLevel,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<NetAppFilesVolume> UpdateVolumeAsync(
        string account,
        string pool,
        string volume,
        long quotaGib,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default);
}