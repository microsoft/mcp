// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.VirtualDesktop.Services.Models;

/// <summary>
/// A class representing the host pool properties.
/// </summary>
internal sealed class HostPoolProperties
{
    /// <summary> ObjectId of HostPool. (internal use). </summary>
    public string? ObjectId { get; }
    /// <summary> Friendly name of HostPool. </summary>
    public string? FriendlyName { get; set; }
    /// <summary> Description of HostPool. </summary>
    public string? Description { get; set; }
    /// <summary> HostPool type for desktop. </summary>
    public string? HostPoolType { get; set; }
    /// <summary> PersonalDesktopAssignment type for HostPool. </summary>
    public string? PersonalDesktopAssignmentType { get; set; }
    /// <summary> Custom rdp property of HostPool. </summary>
    public string? CustomRdpProperty { get; set; }
    /// <summary> The max session limit of HostPool. </summary>
    public int? MaxSessionLimit { get; set; }
    /// <summary> The type of the load balancer. </summary>
    public string? LoadBalancerType { get; set; }
    /// <summary> The ring number of HostPool. </summary>
    public int? Ring { get; set; }
    /// <summary> Is validation environment. </summary>
    [JsonPropertyName("validationEnvironment")]
    public bool? IsValidationEnvironment { get; set; }
    /// <summary> The registration info of HostPool. </summary>
    public HostPoolRegistrationInfo? RegistrationInfo { get; set; }
    /// <summary> VM template for sessionhosts configuration within hostpool. </summary>
    public string? VmTemplate { get; set; }
    /// <summary> List of applicationGroup links. </summary>
    public IReadOnlyList<string>? ApplicationGroupReferences { get; }
    /// <summary> List of App Attach Package links. </summary>
    public IReadOnlyList<string>? AppAttachPackageReferences { get; }
    /// <summary> URL to customer ADFS server for signing WVD SSO certificates. </summary>
    public string? SsoAdfsAuthority { get; set; }
    /// <summary> ClientId for the registered Relying Party used to issue WVD SSO certificates. </summary>
    public string? SsoClientId { get; set; }
    /// <summary> Path to Azure KeyVault storing the secret used for communication to ADFS. </summary>
    public string? SsoClientSecretKeyVaultPath { get; set; }
    /// <summary> The type of single sign on Secret Type. </summary>
    public string? SsoSecretType { get; set; }
    /// <summary> The type of preferred application group type, default to Desktop Application Group. </summary>
    public string? PreferredAppGroupType { get; set; }
    /// <summary> The flag to turn on/off StartVMOnConnect feature. </summary>
    [JsonPropertyName("startVMOnConnect")]
    public bool? StartVmOnConnect { get; set; }
    /// <summary> Is cloud pc resource. </summary>
    [JsonPropertyName("cloudPcResource")]
    public bool? IsCloudPCResource { get; }
    /// <summary> Enabled allows this resource to be accessed from both public and private networks, Disabled allows this resource to only be accessed via private endpoints. </summary>
    public string? PublicNetworkAccess { get; set; }
}
