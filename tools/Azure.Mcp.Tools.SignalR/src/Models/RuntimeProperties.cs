// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SignalR.Models;

/// <summary>
/// Represents runtime-specific properties for a SignalR service.
/// </summary>
public sealed class RuntimeProperties
{
    /// <summary> The public external IP address. </summary>
    public string? ExternalIP { get; set; }

    /// <summary> The primary host name. </summary>
    public string? HostName { get; set; }

    /// <summary> Network access control rules. </summary>
    public NetworkAcls? NetworkAcls { get; set; }

    /// <summary> The provisioning state of the resource. </summary>
    public string? ProvisioningState { get; set; }

    /// <summary> Public network access setting. </summary>
    public string? PublicNetworkAccess { get; set; }

    /// <summary> Public port for client connections. </summary>
    public int? PublicPort { get; set; }

    /// <summary> Server port for upstream connections. </summary>
    public int? ServerPort { get; set; }

    /// <summary> Upstream forwarding templates. </summary>
    public IList<UpstreamTemplate>? UpstreamTemplates { get; set; }
}
