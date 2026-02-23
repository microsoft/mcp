// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Microsoft.Mcp.Core.Areas.Server.Options;

namespace Azure.Mcp.Core.Areas.Server.Options;

/// <summary>
/// Configuration options for starting the Azure MCP server service.
/// </summary>
public sealed class ServiceStartOptions : ServerOptions
{
    /// <summary>
    /// Gets or sets the transport mechanism for the server.
    /// Defaults to standard I/O (stdio).
    /// </summary>
    [JsonPropertyName("transport")]
    public string Transport { get; set; } = TransportTypes.StdIo;

    /// <summary>
    /// Gets or sets whether HTTP incoming authentication is disabled.
    /// When true, the server accepts unauthenticated HTTP requests.
    /// </summary>
    [JsonPropertyName("dangerouslyDisableHttpIncomingAuth")]
    public bool DangerouslyDisableHttpIncomingAuth { get; set; } = false;
}
