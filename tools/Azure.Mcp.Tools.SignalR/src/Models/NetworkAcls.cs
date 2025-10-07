// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SignalR.Models;

/// <summary>
/// Represents a SignalR network rule.
/// </summary>
public sealed class NetworkAcls
{
    /// <summary> Gets or sets the default action to take when no specific rule matches. </summary>
    public string? DefaultAction { get; set; }

    /// <summary> Gets or sets the collection of private endpoint configurations with their specific access rules. </summary>
    public IList<PrivateEndpoint>? PrivateEndpoints { get; set; }

    /// <summary> Gets or sets the network access rules for public network connections. </summary>
    public PublicNetwork? PublicNetwork { get; set; }
}

/// <summary>
/// Represents a collection of allow/deny lists for a network scope.
/// </summary>
public abstract class Network
{
    /// <summary> Gets or sets the collection of allowed request types for public network access. </summary>
    public IList<string>? Allow { get; set; }

    /// <summary> Gets or sets the collection of denied request types for public network access. </summary>
    public IList<string>? Deny { get; set; }
}

/// <summary> Represents public network access rules. </summary>
public sealed class PublicNetwork : Network;

/// <summary>
/// Represents a private endpoint network rule set.
/// </summary>
public sealed class PrivateEndpoint : Network
{
    /// <summary> Gets or sets the name of the private endpoint. </summary>
    public string? Name { get; set; }
}
