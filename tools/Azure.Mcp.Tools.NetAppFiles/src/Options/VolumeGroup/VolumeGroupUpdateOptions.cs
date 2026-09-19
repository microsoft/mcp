// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.VolumeGroup;

public sealed class VolumeGroupUpdateOptions : ISubscriptionOption
{
    [Option(Description = "The name of the Azure NetApp Files account containing the application volume group. Must be 1-128 characters, start with an alphanumeric character, and contain only alphanumerics, underscores, and hyphens.")]
    public required string Account { get; set; }

    [Option(Description = "The name of the Azure NetApp Files application volume group to update. Must be 1-64 characters, start with an alphanumeric character, and contain only alphanumerics, underscores, and hyphens.")]
    public required string VolumeGroup { get; set; }

    [Option(Description = "The application type for the volume group. Supported values are SapHana and Oracle.")]
    public string? ApplicationType { get; set; }

    [Option(Description = "The application-specific identifier, such as an SAP system ID or Oracle database ID.")]
    public string? ApplicationIdentifier { get; set; }

    [Option(Description = "A description for the volume group. Providing an empty string clears the description.")]
    public string? GroupDescription { get; set; }

    [Option(Description = "A JSON array that replaces the complete member-volume specification list. Each item requires name, creationToken, quotaGib, subnetId, capacityPoolId, and volumeSpecName. Optional fields are serviceLevel (defaults to Premium), protocols, throughputMibps, zones, and proximityPlacementGroupId. NFS volumes also require allowedClients.")]
    public string? Volumes { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
