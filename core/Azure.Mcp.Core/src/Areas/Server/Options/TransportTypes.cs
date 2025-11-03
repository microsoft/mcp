// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Core.Areas.Server.Options;

/// <summary>
/// Defines the supported transport mechanisms for the Azure MCP server.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransportTypes
{
    /// <summary>
    /// Standard Input/Output transport mechanism (default).
    /// </summary>
    StdIo,

    /// <summary>
    /// HTTP transport mechanism.
    /// </summary>
    Http
}
