// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Aks.Commands;

namespace Azure.Mcp.Tools.Aks.Services.Models;

internal sealed class AksNetworkProfile
{
    /// <summary> Network plugin used for building the Kubernetes network. </summary>
    public string? NetworkPlugin { get; set; }
    /// <summary> Network policy used for building the Kubernetes network. </summary>
    public string? NetworkPolicy { get; set; }
    /// <summary> A CIDR notation IP range from which to assign service cluster IPs. It must not overlap with any Subnet IP ranges. </summary>
    public string? ServiceCidr { get; set; }
    /// <summary> An IP address assigned to the Kubernetes DNS service. It must be within the Kubernetes service address range specified in serviceCidr. </summary>
    public string? DnsServiceIP { get; set; }
}