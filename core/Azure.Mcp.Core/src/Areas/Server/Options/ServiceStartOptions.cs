// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Core.Areas.Server.Options;

/// <summary>
/// Configuration options for starting the Azure MCP server service.
/// </summary>
public class ServiceStartOptions
{
    /// <summary>
    /// Gets or sets the transport mechanism for the server.
    /// Defaults to standard I/O (stdio).
    /// </summary>
    [JsonPropertyName("transport")]
    public string Transport { get; set; } = TransportTypes.StdIo;

    /// <summary>
    /// Gets or sets the service namespaces to expose through the server.
    /// When null, all available namespaces are exposed.
    /// </summary>
    [JsonPropertyName("namespace")]
    public string[]? Namespace { get; set; } = null;

    /// <summary>
    /// Gets or sets the mode mode for the server.
    /// Defaults to 'namespace' mode which exposes one tool per service namespace.
    /// </summary>
    [JsonPropertyName("mode")]
    public string? Mode { get; set; } = ModeTypes.NamespaceProxy;

    /// <summary>
    /// Gets or sets whether the server should operate in read-only mode.
    /// When true, only tools marked as read-only will be available.
    /// </summary>
    [JsonPropertyName("readOnly")]
    public bool? ReadOnly { get; set; } = null;
}
