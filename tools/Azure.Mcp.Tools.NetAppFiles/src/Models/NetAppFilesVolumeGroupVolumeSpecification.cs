// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.NetAppFiles.Models;

public class NetAppFilesVolumeGroupVolumeSpecification
{
    public required string Name { get; set; }

    public required string CreationToken { get; set; }

    public long QuotaGib { get; set; }

    public required string SubnetId { get; set; }

    public required string CapacityPoolId { get; set; }

    public required string VolumeSpecName { get; set; }

    public string? ServiceLevel { get; set; }

    public List<string>? Protocols { get; set; }

    public string? AllowedClients { get; set; }

    public float? ThroughputMibps { get; set; }

    public List<string>? Zones { get; set; }

    public string? ProximityPlacementGroupId { get; set; }
}
