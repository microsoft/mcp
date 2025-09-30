// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using ModelContextProtocol.Client;

namespace Azure.Mcp.Core.Areas.Server.Commands.Discovery;

public interface IMcpDiscoveryStrategy : IAsyncDisposable
{
    /// <summary>
    /// Discovers available MCP servers via this strategy.
    /// </summary>
    /// <returns>A collection of discovered MCP servers.</returns>
    Task<IEnumerable<IMcpServerProvider>> DiscoverServersAsync();

    /// <summary>
    /// Finds a server provider by name.
    /// </summary>
    /// <param name="name">The name of the server to find.</param>
    /// <returns>The server provider if found.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no server with the specified name is found.</exception>
    Task<IMcpServerProvider> FindServerProviderAsync(string name);

    /// <summary>
    /// Gets an MCP client for the specified server.
    /// </summary>
    /// <param name="name">The name of the server to get a client for.</param>
    /// <param name="clientOptions">Optional client configuration options. If null, default options are used.</param>
    /// <returns>An MCP client that can communicate with the specified server.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no server with the specified name is found.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the name parameter is null.</exception>
    Task<McpClient> GetOrCreateClientAsync(string name, McpClientOptions? clientOptions = null);
}
