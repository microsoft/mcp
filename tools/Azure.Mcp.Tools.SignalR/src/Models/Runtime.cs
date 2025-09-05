// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SignalR.Models;

/// <summary>
/// Represents a SignalR service runtime configuration.
/// </summary>
public class Runtime
{
    /// <summary> The name of the SignalR service. </summary>
    public string? Name { get; set; }

    /// <summary> The resource group name containing the SignalR service. </summary>
    public string? ResourceGroupName { get; set; }

    /// <summary> The Azure region location of the SignalR service. </summary>
    public string? Location { get; set; }

    /// <summary> The SKU name of the SignalR service. </summary>
    public string? SkuName { get; set; }

    /// <summary> The SKU tier of the SignalR service. </summary>
    public string? SkuTier { get; set; }

    /// <summary> The provisioning state of the SignalR service. </summary>
    public string? ProvisioningState { get; set; }

    /// <summary> The hostname of the SignalR service. </summary>
    public string? HostName { get; set; }

    /// <summary> The public port of the SignalR service. </summary>
    public int? PublicPort { get; set; }

    /// <summary> The server port of the SignalR service. </summary>
    public int? ServerPort { get; set; }
}
