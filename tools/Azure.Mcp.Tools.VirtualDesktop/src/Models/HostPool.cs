// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.ResourceManager.DesktopVirtualization;

namespace Azure.Mcp.Tools.VirtualDesktop.Models;

public class HostPool
{
    /// <summary> Name of the host pool. </summary>
    public string? Name { get; set; }

    /// <summary> Resource group name containing the host pool. </summary>
    public string? ResourceGroupName { get; set; }

    /// <summary> Azure subscription ID containing the host pool. </summary>
    public string? SubscriptionId { get; set; }

    /// <summary> Azure location where the host pool is deployed. </summary>
    public string? Location { get; set; }

    /// <summary> Type of host pool (Personal or Pooled). </summary>
    public string? HostPoolType { get; set; }

    /// <summary> Type of load balancer for the host pool. </summary>
    public string? LoadBalancerType { get; set; }

    /// <summary> Maximum number of users per session host. </summary>
    public int? MaxSessionLimit { get; set; }

    /// <summary> Friendly name of the host pool. </summary>
    public string? FriendlyName { get; set; }

    /// <summary> Description of the host pool. </summary>
    public string? Description { get; set; }

    /// <summary> Validation environment setting. </summary>
    public bool? ValidationEnvironment { get; set; }

    /// <summary> Preferred application group type. </summary>
    public string? PreferredAppGroupType { get; set; }

    /// <summary> Start VM on connect setting. </summary>
    public bool? StartVMOnConnect { get; set; }

    /// <summary> Whether to register the default desktop application group. </summary>
    public bool? RegistrationEnabled { get; set; }

    /// <summary> Custom RDP properties for the host pool. </summary>
    public string? CustomRdpProperty { get; set; }

    /// <summary> Azure Resource Manager resource identifier for the host pool. </summary>
    public string? ResourceIdentifier { get; set; }

    /// <summary> Constructor that takes a HostPoolResource and extracts relevant data. </summary>
    [SetsRequiredMembers]
    public HostPool(HostPoolResource resource) : this()
    {
        Name = resource.Data.Name;
        ResourceGroupName = resource.Id.ResourceGroupName;
        SubscriptionId = resource.Id.SubscriptionId;
        Location = resource.Data.Location.ToString();
        HostPoolType = resource.Data.HostPoolType.ToString();
        LoadBalancerType = resource.Data.LoadBalancerType.ToString();
        MaxSessionLimit = resource.Data.MaxSessionLimit;
        FriendlyName = resource.Data.FriendlyName;
        Description = resource.Data.Description;
        ValidationEnvironment = resource.Data.IsValidationEnvironment;
        PreferredAppGroupType = resource.Data.PreferredAppGroupType.ToString();
        StartVMOnConnect = resource.Data.StartVmOnConnect;
        RegistrationEnabled = resource.Data.RegistrationInfo?.RegistrationTokenOperation != null;
        CustomRdpProperty = resource.Data.CustomRdpProperty;
        ResourceIdentifier = resource.Id.ToString();
    }

    /// <summary> Default constructor for serialization. </summary>
    public HostPool() { }
}
