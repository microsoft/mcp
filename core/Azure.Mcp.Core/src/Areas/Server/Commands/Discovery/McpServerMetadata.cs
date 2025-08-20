// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using ModelContextProtocol.Client;

namespace Azure.Mcp.Core.Areas.Server.Commands.Discovery;

/// <summary>
/// Represents metadata for an MCP server, including identification and descriptive information.
/// </summary>
/// <param name="id">The unique identifier for the server.</param>
/// <param name="name">The display name of the server.</param>
/// <param name="description">A description of the server's purpose or capabilities.</param>
public sealed class McpServerMetadata(string id = "", string name = "", string description = "")
{
    /// <summary>
    /// Gets or sets the unique identifier for the server.
    /// </summary>
    public string Id { get; set; } = id;

    /// <summary>
    /// Gets or sets the display name of the server.
    /// </summary>
    public string Name { get; set; } = name;

    /// <summary>
    /// Gets or sets a description of the server's purpose or capabilities.
    /// </summary>
    public string Description { get; set; } = description;
}

/// <summary>
/// Defines an interface for MCP server providers that can create server metadata and clients.
/// </summary>
public interface IMcpServerProvider
{
    /// <summary>
    /// Creates metadata that describes this server provider.
    /// </summary>
    /// <returns>A metadata object containing the server's identity and description.</returns>
    McpServerMetadata CreateMetadata();

    /// <summary>
    /// Creates an MCP client that can communicate with this server.
    /// </summary>
    /// <param name="clientOptions">Options to configure the client behavior.</param>
    /// <returns>A configured MCP client ready for use.</returns>
    Task<IMcpClient> CreateClientAsync(McpClientOptions clientOptions);
}
