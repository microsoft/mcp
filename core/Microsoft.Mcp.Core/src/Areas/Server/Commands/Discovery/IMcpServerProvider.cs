// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using ModelContextProtocol.Client;

namespace Microsoft.Mcp.Core.Areas.Server.Commands.Discovery;

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
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A configured MCP client ready for use.</returns>
    /// <exception cref="ArgumentException">Thrown when the server configuration doesn't specify a valid transport type (missing URL or stdio configuration).</exception>
    /// <exception cref="InvalidOperationException">Thrown when the server configuration is valid but client creation fails (e.g., missing command for stdio transport, dependency issues, or external process failures).</exception>
    Task<McpClient> CreateClientAsync(McpClientOptions clientOptions, CancellationToken cancellationToken);
}
