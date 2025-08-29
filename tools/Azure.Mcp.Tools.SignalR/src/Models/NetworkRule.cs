// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SignalR.Models;

/// <summary>
/// Represents a SignalR network rule.
/// </summary>
public class NetworkRule
{
    /// <summary> Gets or sets the default action to take when no specific rule matches. </summary>
    public string? DefaultAction { get; set; }

    /// <summary> Gets or sets the collection of private endpoint configurations with their specific access rules. </summary>
    public IEnumerable<PrivateEndpointNetworkAcl>? PrivateEndpoints { get; set; }

    /// <summary> Gets or sets the network access rules for public network connections. </summary>
    public NetworkAcl? PublicNetwork { get; set; }
}

public class PrivateEndpointNetworkAcl : NetworkAcl
{
    /// <summary> Gets or sets the name of the private endpoint. </summary>
    public string? Name { get; set; }
}

public class NetworkAcl
{
    /// <summary> Gets or sets the collection of allowed request types for public network access. </summary>
    public IEnumerable<string>? Allow { get; set; }

    /// <summary> Gets or sets the collection of denied request types for public network access. </summary>
    public IEnumerable<string>? Deny { get; set; }
}
