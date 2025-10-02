// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Grafana.Services.Models;

/// <summary>
/// A class representing the Managed Grafana properties model.
/// </summary>
internal sealed class ManagedGrafanaProperties
{
    /// <summary> Provisioning state of the resource. </summary>
    public string? ProvisioningState { get; set; }
    /// <summary> The Grafana software version. </summary>
    public string? GrafanaVersion { get; set; }
    /// <summary> The endpoint of the Grafana instance. </summary>
    public string? Endpoint { get; set; }
    /// <summary> Indicate the state for enable or disable traffic over the public interface. </summary>
    public string? PublicNetworkAccess { get; set; }
    /// <summary> The zone redundancy setting of the Grafana instance. </summary>
    public string? ZoneRedundancy { get; set; }
    /// <summary> The api key setting of the Grafana instance. </summary>
    public string? ApiKey { get; set; }
    /// <summary> Whether a Grafana instance uses deterministic outbound IPs. </summary>
    public string? DeterministicOutboundIP { get; set; }
    /// <summary> List of outbound IPs if deterministicOutboundIP is enabled. </summary>
    public IReadOnlyList<string>? OutboundIPs { get; set; }
}
