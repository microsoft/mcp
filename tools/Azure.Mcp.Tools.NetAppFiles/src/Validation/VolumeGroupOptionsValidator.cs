// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.NetAppFiles.Validation;

internal static class VolumeGroupOptionsValidator
{
    private const long MinimumQuotaGib = 50;
    private const long MaximumRegularVolumeQuotaGib = 100 * 1024;
    private static readonly string[] SupportedProtocols = ["NFSv3", "NFSv4.1", "CIFS"];
    private static readonly string[] SupportedServiceLevels = ["Standard", "Premium", "Ultra", "Flexible"];

    public static void ValidateApplicationType(string? applicationType, ValidationResult validationResult)
    {
        if (!string.Equals(applicationType, "SapHana", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(applicationType, "Oracle", StringComparison.OrdinalIgnoreCase))
        {
            validationResult.Errors.Add("--application-type must be SapHana or Oracle.");
        }
    }

    public static void ValidateVolumes(string volumesJson, ValidationResult validationResult)
    {
        List<NetAppFilesVolumeGroupVolumeSpecification>? volumes;
        try
        {
            volumes = JsonSerializer.Deserialize(
                volumesJson,
                NetAppFilesJsonContext.Default.ListNetAppFilesVolumeGroupVolumeSpecification);
        }
        catch (JsonException)
        {
            validationResult.Errors.Add("--volumes must be a valid JSON array of volume specifications.");
            return;
        }

        if (volumes is null || volumes.Count == 0)
        {
            validationResult.Errors.Add("--volumes must contain at least one volume specification.");
            return;
        }

        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var creationTokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var volume in volumes)
        {
            ValidateVolume(volume, names, creationTokens, validationResult);
        }
    }

    private static void ValidateVolume(
        NetAppFilesVolumeGroupVolumeSpecification volume,
        HashSet<string> names,
        HashSet<string> creationTokens,
        ValidationResult validationResult)
    {
        if (!VolumeGroupNameValidator.IsValidVolumeName(volume.Name))
        {
            validationResult.Errors.Add("Each volume name must be 1-64 characters, start with an alphanumeric character, and contain only alphanumerics, underscores, and hyphens.");
        }
        else if (!names.Add(volume.Name))
        {
            validationResult.Errors.Add($"Duplicate volume name '{volume.Name}' is not allowed.");
        }

        if (!VolumeGroupNameValidator.IsValidCreationToken(volume.CreationToken))
        {
            validationResult.Errors.Add("Each creationToken must be 1-80 characters, start with a letter, and contain only alphanumerics, underscores, and hyphens.");
        }
        else if (!creationTokens.Add(volume.CreationToken))
        {
            validationResult.Errors.Add($"Duplicate creationToken '{volume.CreationToken}' is not allowed.");
        }

        if (volume.QuotaGib is < MinimumQuotaGib or > MaximumRegularVolumeQuotaGib)
        {
            validationResult.Errors.Add($"Volume '{volume.Name}' quotaGib must be between {MinimumQuotaGib} and {MaximumRegularVolumeQuotaGib}.");
        }

        ValidateResourceId(volume.SubnetId, "Microsoft.Network/virtualNetworks/subnets", "subnetId", volume.Name, validationResult);
        ValidateResourceId(volume.CapacityPoolId, "Microsoft.NetApp/netAppAccounts/capacityPools", "capacityPoolId", volume.Name, validationResult);

        if (string.IsNullOrWhiteSpace(volume.VolumeSpecName))
        {
            validationResult.Errors.Add($"Volume '{volume.Name}' volumeSpecName cannot be empty or whitespace.");
        }

        if (volume.ServiceLevel is not null &&
            !SupportedServiceLevels.Contains(volume.ServiceLevel, StringComparer.OrdinalIgnoreCase))
        {
            validationResult.Errors.Add($"Volume '{volume.Name}' serviceLevel must be Standard, Premium, Ultra, or Flexible.");
        }

        if (volume.Protocols is { Count: 0 } ||
            volume.Protocols?.Any(protocol => !SupportedProtocols.Contains(protocol, StringComparer.OrdinalIgnoreCase)) == true)
        {
            validationResult.Errors.Add($"Volume '{volume.Name}' protocols must contain only NFSv3, NFSv4.1, or CIFS.");
        }

        var protocols = volume.Protocols ?? ["NFSv4.1"];
        if (protocols.Any(static protocol => protocol.StartsWith("NFS", StringComparison.OrdinalIgnoreCase)) &&
            string.IsNullOrWhiteSpace(volume.AllowedClients))
        {
            validationResult.Errors.Add($"Volume '{volume.Name}' allowedClients is required for NFS protocols.");
        }

        if (volume.ThroughputMibps <= 0)
        {
            validationResult.Errors.Add($"Volume '{volume.Name}' throughputMibps must be greater than zero when provided.");
        }

        if (volume.ProximityPlacementGroupId is not null)
        {
            ValidateResourceId(
                volume.ProximityPlacementGroupId,
                "Microsoft.Compute/proximityPlacementGroups",
                "proximityPlacementGroupId",
                volume.Name,
                validationResult);
        }
    }

    private static void ValidateResourceId(
        string? value,
        string expectedResourceType,
        string propertyName,
        string volumeName,
        ValidationResult validationResult)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                validationResult.Errors.Add($"Volume '{volumeName}' {propertyName} must be a valid Azure resource ID.");
                return;
            }

            var resourceId = new ResourceIdentifier(value);
            if (!string.Equals(resourceId.ResourceType.ToString(), expectedResourceType, StringComparison.OrdinalIgnoreCase))
            {
                validationResult.Errors.Add($"Volume '{volumeName}' {propertyName} must identify a {expectedResourceType} resource.");
            }
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException)
        {
            validationResult.Errors.Add($"Volume '{volumeName}' {propertyName} must be a valid Azure resource ID.");
        }
    }
}
