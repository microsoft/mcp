// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Mcp.Core.Areas.Server.Models;

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
    /// Gets the URL of the remote server.
    /// This should be <see langword="null"/> if <see cref="Type"/> is "stdio".
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; init; }

    /// <summary>
    /// Gets OAuth scopes to request in the access token.
    /// Used for remote MCP servers protected by OAuth.
    /// </summary>
    [JsonPropertyName("oauthScopes")]
    public string[]? OAuthScopes { get; init; }

    /// <summary>
    /// Gets a description of the server's purpose or capabilities.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// Gets the user-friendly title for the server.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>
    /// Gets the transport type, e.g., "stdio".
    /// This should be <see langword="null"/> if <see cref="Url"/> is non-<see langword="null"/>.
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

    /// <summary>
    /// Gets the minimum required version for the command.
    /// When set along with a stdio command, a pre-flight version check is
    /// performed before creating the client. The version is parsed from
    /// the output of running the command with <see cref="VersionArgs"/>.
    /// </summary>
    [JsonPropertyName("minVersion")]
    public string? MinVersion { get; init; }

    /// <summary>
    /// Gets the command-line arguments used to query the command's version.
    /// Required when <see cref="MinVersion"/> is set.
    /// </summary>
    [JsonPropertyName("versionArgs")]
    public List<string>? VersionArgs { get; init; }

    /// <summary>
    /// Gets the regex pattern used to extract a version from the command output.
    /// The pattern must contain a capture group for the version string.
    /// Defaults to a semver pattern <c>(\d+\.\d+\.\d+)</c> if not specified.
    /// </summary>
    [JsonPropertyName("versionPattern")]
    public string? VersionPattern { get; init; }

    /// <summary>
    /// Gets installation instructions for the server.
    /// </summary>
    [JsonPropertyName("installInstructions")]
    public string? InstallInstructions { get; init; }

    /// <summary>
    /// Gets the prefix to prepend to all tool names exposed from this server.
    /// When set, tools from this server will be exposed with this prefix (e.g. "foundry_").
    /// </summary>
    [JsonPropertyName("toolPrefix")]
    public string? ToolPrefix { get; init; }
}
