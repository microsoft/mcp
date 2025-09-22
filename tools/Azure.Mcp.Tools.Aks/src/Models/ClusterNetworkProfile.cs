// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Aks.Models;

public sealed class ClusterNetworkProfile
{
    public string? NetworkPlugin { get; set; }
    public string? NetworkPluginMode { get; set; }
    public string? NetworkPolicy { get; set; }
    public string? NetworkDataplane { get; set; }
    public string? LoadBalancerSku { get; set; }
    public ClusterNetworkLoadBalancerProfile? LoadBalancerProfile { get; set; }
    public string? PodCidr { get; set; }
    public string? ServiceCidr { get; set; }
    public string? DnsServiceIP { get; set; }
    public string? OutboundType { get; set; }
    public IList<string>? PodCidrs { get; set; }
    public IList<string>? ServiceCidrs { get; set; }
    public IList<string>? IpFamilies { get; set; }
}

public sealed class ClusterNetworkLoadBalancerProfile
{
    public int? ManagedOutboundIPCount { get; set; }
    public IList<EffectiveOutboundIPReference>? EffectiveOutboundIPs { get; set; }
    public string? BackendPoolType { get; set; }
}

public sealed class EffectiveOutboundIPReference
{
    public string? Id { get; set; }
}

