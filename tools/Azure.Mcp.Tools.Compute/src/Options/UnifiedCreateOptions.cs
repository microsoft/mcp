// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Options;

/// <summary>
/// Options for the unified <c>compute create</c> entry point, which provisions a VMSS Flex scale set by
/// default and a single non-scalable VM only when <see cref="SingleInstance"/> is set to true.
///
/// The option surface is the superset of <see cref="Vmss.VmssCreateOptions"/> and the single-VM create
/// options — VMSS field names are canonical because Flex is the recommended path. When
/// <see cref="SingleInstance"/> is true, <see cref="Name"/> maps to the single VM's name and the
/// VMSS-only fields (<see cref="InstanceCount"/>, <see cref="UpgradePolicy"/>) are ignored.
/// </summary>
public class UnifiedCreateOptions : BaseComputeOptions
{
    /// <summary>
    /// Name of the resource to create. For the default VMSS Flex path this is the scale-set name; for
    /// <see cref="SingleInstance"/>=true this is the single VM name.
    /// </summary>
    public string? Name { get; set; }

    public string? Location { get; set; }

    public string? VmSize { get; set; }

    public string? Image { get; set; }

    public string? AdminUsername { get; set; }

    public string? AdminPassword { get; set; }

    public string? SshPublicKey { get; set; }

    public string? OsType { get; set; }

    public string? VirtualNetwork { get; set; }

    public string? Subnet { get; set; }

    public string? PublicIpAddress { get; set; }

    public string? NetworkSecurityGroup { get; set; }

    public bool NoPublicIp { get; set; }

    public string? SourceAddressPrefix { get; set; }

    /// <summary>
    /// Number of VMSS Flex instances. Ignored when <see cref="SingleInstance"/> is true. When omitted on
    /// the VMSS path, the service defaults to 2 to encourage zonal spread.
    /// </summary>
    public int? InstanceCount { get; set; }

    /// <summary>VMSS Flex upgrade policy. Ignored when <see cref="SingleInstance"/> is true.</summary>
    public string? UpgradePolicy { get; set; }

    public string? Zone { get; set; }

    public int? OsDiskSizeGb { get; set; }

    public string? OsDiskType { get; set; }

    /// <summary>
    /// When true, fall back to a single non-scalable VM instead of the recommended VMSS Flex path.
    /// </summary>
    public bool SingleInstance { get; set; }
}
