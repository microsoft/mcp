// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.VolumeGroup;

public class VolumeGroupCreateOptions : ISubscriptionOption
{
    [Option(Description = "The name of the Azure NetApp Files account that will contain the volume group.")]
    public required string Account { get; set; }

    [Option(Description = "The name of the volume group to create. Must be 1-64 characters, start with an alphanumeric character, and contain only alphanumerics, underscores, and hyphens.")]
    public required string VolumeGroup { get; set; }

    [Option(Description = "The Azure region where the volume group will be created (for example, 'eastus' or 'westus2').")]
    public required string Location { get; set; }

    [Option(Description = "The application type for the volume group. Supported values are SapHana and Oracle.")]
    public required string ApplicationType { get; set; }

    [Option(Description = "The application-specific identifier, such as an SAP system ID or Oracle database ID.")]
    public required string ApplicationIdentifier { get; set; }

    [Option(Description = "A JSON array of volume specifications. Each item requires name, creationToken, quotaGib, subnetId, capacityPoolId, and volumeSpecName. Optional fields are serviceLevel (defaults to Premium), protocols, throughputMibps, zones, and proximityPlacementGroupId. NFS volumes also require allowedClients.")]
    public required string Volumes { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
