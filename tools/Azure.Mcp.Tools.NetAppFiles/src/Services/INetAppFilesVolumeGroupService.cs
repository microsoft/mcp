// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.NetAppFiles.Models;

namespace Azure.Mcp.Tools.NetAppFiles.Services;

public interface INetAppFilesVolumeGroupService
{
    Task<NetAppFilesVolumeGroup> GetVolumeGroupAsync(
        string account,
        string volumeGroup,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<NetAppFilesVolumeGroup> UpdateVolumeGroupAsync(
        string account,
        string volumeGroup,
        string? applicationType,
        string? applicationIdentifier,
        string? groupDescription,
        IReadOnlyList<NetAppFilesVolumeGroupVolumeSpecification>? volumes,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<NetAppFilesVolumeGroup> CreateVolumeGroupAsync(
        string account,
        string volumeGroup,
        string location,
        string applicationType,
        string applicationIdentifier,
        IReadOnlyList<NetAppFilesVolumeGroupVolumeSpecification> volumes,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default);
}
