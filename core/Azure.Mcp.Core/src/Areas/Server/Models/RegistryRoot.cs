// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Core.Areas.Server.Models;

/// <summary>
/// Represents the root structure of the MCP server registry JSON file.
/// Contains a collection of server configurations keyed by server name.
/// </summary>
public sealed class RegistryRoot
{
    /// <summary>
    /// Gets the dictionary of server configurations, keyed by server name.
    /// </summary>
    [JsonPropertyName("servers")]
    public Dictionary<string, RegistryServerInfo>? Servers { get; init; }
}
