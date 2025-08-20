// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Core.Areas.Server.Models;

/// <summary>
/// Contains configuration information for an MCP server defined in the registry.
/// Supports command-based (stdio) transport mechanism.
/// </summary>
public sealed class RegistryServerInfo
{
    /// <summary>
    /// Gets or sets the name of the server, typically derived from the key in the registry.
    /// This property is not serialized to/from JSON.
    /// </summary>
    [JsonIgnore]
    public string? Name { get; set; }

    /// <summary>
    /// Gets the URL endpoint (deprecated - no longer used).
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; init; }

    /// <summary>
    /// Gets a description of the server's purpose or capabilities.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// Gets the transport type, e.g., "stdio".
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    /// <summary>
    /// Gets the command to execute for stdio-based transport.
    /// </summary>
    [JsonPropertyName("command")]
    public string? Command { get; init; }

    /// <summary>
    /// Gets the command-line arguments to pass to the command for stdio-based transport.
    /// </summary>
    [JsonPropertyName("args")]
    public List<string>? Args { get; init; }

    /// <summary>
    /// Gets environment variables to set for the stdio process.
    /// </summary>
    [JsonPropertyName("env")]
    public Dictionary<string, string>? Env { get; init; }
}
