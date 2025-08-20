// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Models.Identity;

namespace Azure.Mcp.Tools.Grafana.Models.Workspace;

public class Workspace
{
    /// <summary> Name of the Grafana workspace resource. </summary>
    public string? Name { get; set; }

    /// <summary> Name of the resource group containing the Grafana workspace resource. </summary>
    public string? ResourceGroupName { get; set; }

    /// <summary> ID of the Azure subscription containing the Grafana workspace resource. </summary>
    public string? SubscriptionId { get; set; }

    /// <summary> Azure geo-location where the Grafana workspace resource lives. </summary>
    public string? Location { get; set; }

    /// <summary> SKU of the Grafana workspace resource. </summary>
    public string? Sku { get; set; }

    /// <summary> Provisioning status of the Grafana workspace resource. </summary>
    public string? ProvisioningState { get; set; }

    /// <summary> The endpoint URL for the Grafana workspace. </summary>
    public string? Endpoint { get; set; }

    /// <summary> The zone redundancy setting for the Grafana workspace. </summary>
    public string? ZoneRedundancy { get; set; }

    /// <summary> Indicates whether or not public network access is allowed for the Grafana workspace. </summary>
    public string? PublicNetworkAccess { get; set; }

    /// <summary> The Grafana major version. </summary>
    public string? GrafanaVersion { get; set; }

    /// <summary> The managed identity of the Grafana workspace. </summary>
    public ManagedIdentityInfo? Identity { get; set; }

    /// <summary> The resource tags for the Grafana workspace. </summary>
    public IDictionary<string, string>? Tags { get; set; }
}
