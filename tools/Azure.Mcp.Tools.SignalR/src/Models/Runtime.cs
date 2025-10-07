// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SignalR.Models;

/// <summary>
/// Represents a SignalR service runtime configuration.
/// </summary>
public sealed class Runtime
{
    /// <summary> The resource id. </summary>
    public string? Id { get; set; }

    /// <summary> The identity configuration. </summary>
    public Identity? Identity { get; set; }

    /// <summary> The runtime kind. </summary>
    public string? Kind { get; set; }

    /// <summary> The Azure region. </summary>
    public string? Location { get; set; }

    /// <summary> The SignalR service name. </summary>
    public string? Name { get; set; }

    /// <summary> The service properties. </summary>
    public RuntimeProperties? Properties { get; set; }

    /// <summary> The SKU information. </summary>
    public Sku? Sku { get; set; }

    /// <summary> Resource tags. </summary>
    public IDictionary<string, string>? Tags { get; set; }
}
