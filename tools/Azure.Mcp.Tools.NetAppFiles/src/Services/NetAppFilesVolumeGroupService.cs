// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.ResourceManager.NetApp;
using Azure.ResourceManager.NetApp.Models;

namespace Azure.Mcp.Tools.NetAppFiles.Services;

public class NetAppFilesVolumeGroupService(IAzureService azureService)
    : BaseAzureService(azureService), INetAppFilesVolumeGroupService
{
    private const long BytesPerGib = 1024L * 1024L * 1024L;

    public async Task<NetAppFilesVolumeGroup> GetVolumeGroupAsync(
        string account,
        string volumeGroup,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        var resourceGroupResource = await AzureService.GetResourceGroupResource(
            subscription,
            resourceGroup,
            tenant,
            cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException($"Resource group '{resourceGroup}' was not found.");

        var accountResource = resourceGroupResource.GetNetAppAccount(account, cancellationToken).Value;
        var volumeGroupResource = await accountResource
            .GetNetAppVolumeGroups()
            .GetAsync(volumeGroup, cancellationToken);

        return Map(volumeGroupResource.Value);
    }

    public async Task<NetAppFilesVolumeGroup> CreateVolumeGroupAsync(
        string account,
        string volumeGroup,
        string location,
        string applicationType,
        string applicationIdentifier,
        IReadOnlyList<NetAppFilesVolumeGroupVolumeSpecification> volumes,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        var resourceGroupResource = await AzureService.GetResourceGroupResource(
            subscription,
            resourceGroup,
            tenant,
            cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException($"Resource group '{resourceGroup}' was not found.");

        var accountResource = resourceGroupResource.GetNetAppAccount(account, cancellationToken).Value;
        var data = new NetAppVolumeGroupData
        {
            Location = new AzureLocation(location),
            GroupMetaData = new NetAppVolumeGroupMetadata
            {
                ApplicationType = ParseApplicationType(applicationType),
                ApplicationIdentifier = applicationIdentifier
            }
        };

        foreach (var specification in volumes)
        {
            data.Volumes.Add(CreateVolume(specification));
        }

        var operation = await accountResource
            .GetNetAppVolumeGroups()
            .CreateOrUpdateAsync(WaitUntil.Started, volumeGroup, data, cancellationToken);

        await WaitForLroCompletionAsync(operation, cancellationToken);

        return Map(operation.Value);
    }

    public async Task<NetAppFilesVolumeGroup> UpdateVolumeGroupAsync(
        string account,
        string volumeGroup,
        string? applicationType,
        string? applicationIdentifier,
        string? groupDescription,
        IReadOnlyList<NetAppFilesVolumeGroupVolumeSpecification>? volumes,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        var resourceGroupResource = await AzureService.GetResourceGroupResource(
            subscription,
            resourceGroup,
            tenant,
            cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException($"Resource group '{resourceGroup}' was not found.");

        var accountResource = resourceGroupResource.GetNetAppAccount(account, cancellationToken).Value;
        var volumeGroupResource = (await accountResource
            .GetNetAppVolumeGroups()
            .GetAsync(volumeGroup, cancellationToken)).Value;
        var data = volumeGroupResource.Data;

        if (applicationType is not null || applicationIdentifier is not null || groupDescription is not null)
        {
            data.GroupMetaData ??= new NetAppVolumeGroupMetadata();
            if (applicationType is not null)
            {
                data.GroupMetaData.ApplicationType = ParseApplicationType(applicationType);
            }

            if (applicationIdentifier is not null)
            {
                data.GroupMetaData.ApplicationIdentifier = applicationIdentifier;
            }

            if (groupDescription is not null)
            {
                data.GroupMetaData.GroupDescription = groupDescription;
            }
        }

        if (volumes is not null)
        {
            data.Volumes.Clear();
            foreach (var specification in volumes)
            {
                data.Volumes.Add(CreateVolume(specification));
            }
        }

        var operation = await volumeGroupResource.UpdateAsync(WaitUntil.Started, data, cancellationToken);
        await WaitForLroCompletionAsync(operation, cancellationToken);

        return Map(operation.Value);
    }

    private static NetAppVolumeGroupVolume CreateVolume(NetAppFilesVolumeGroupVolumeSpecification specification)
    {
        var volume = new NetAppVolumeGroupVolume(
            specification.CreationToken,
            checked(specification.QuotaGib * BytesPerGib),
            new ResourceIdentifier(specification.SubnetId))
        {
            Name = specification.Name,
            CapacityPoolResourceId = new ResourceIdentifier(specification.CapacityPoolId),
            VolumeSpecName = specification.VolumeSpecName,
            ServiceLevel = ParseServiceLevel(specification.ServiceLevel),
            ThroughputMibps = specification.ThroughputMibps,
            ProximityPlacementGroupId = specification.ProximityPlacementGroupId is null
                ? null
                : new ResourceIdentifier(specification.ProximityPlacementGroupId)
        };

        foreach (var protocol in specification.Protocols ?? ["NFSv4.1"])
        {
            volume.ProtocolTypes.Add(protocol);
        }

        foreach (var zone in specification.Zones ?? [])
        {
            volume.Zones.Add(zone);
        }

        if (volume.ProtocolTypes.Any(static protocol =>
            protocol.Equals("NFSv3", StringComparison.OrdinalIgnoreCase) ||
            protocol.Equals("NFSv4.1", StringComparison.OrdinalIgnoreCase)))
        {
            volume.ExportRules.Add(new NetAppVolumeExportPolicyRule
            {
                RuleIndex = 1,
                AllowedClients = specification.AllowedClients,
                HasRootAccess = true,
                IsUnixReadWrite = true,
                AllowNfsV3Protocol = volume.ProtocolTypes.Contains("NFSv3", StringComparer.OrdinalIgnoreCase),
                AllowNfsV41Protocol = volume.ProtocolTypes.Contains("NFSv4.1", StringComparer.OrdinalIgnoreCase)
            });
        }

        return volume;
    }

    private static NetAppApplicationType ParseApplicationType(string applicationType) =>
        applicationType.Equals("SapHana", StringComparison.OrdinalIgnoreCase)
            ? NetAppApplicationType.SapHana
            : NetAppApplicationType.Oracle;

    private static NetAppFileServiceLevel ParseServiceLevel(string? serviceLevel) =>
        serviceLevel?.ToUpperInvariant() switch
        {
            null or "PREMIUM" => NetAppFileServiceLevel.Premium,
            "STANDARD" => NetAppFileServiceLevel.Standard,
            "ULTRA" => NetAppFileServiceLevel.Ultra,
            "FLEXIBLE" => NetAppFileServiceLevel.Flexible,
            _ => throw new ArgumentOutOfRangeException(nameof(serviceLevel))
        };

    private static NetAppFilesVolumeGroup Map(NetAppVolumeGroupResource volumeGroup) => new(
        volumeGroup.Data.Name,
        volumeGroup.Data.Id.ToString(),
        volumeGroup.Data.Location?.ToString() ?? string.Empty,
        volumeGroup.Data.ProvisioningState,
        volumeGroup.Data.GroupMetaData?.ApplicationType?.ToString(),
        volumeGroup.Data.GroupMetaData?.ApplicationIdentifier,
        volumeGroup.Data.Volumes.Select(static volume => volume.Name ?? volume.CreationToken).ToList());
}
