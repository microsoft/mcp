// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Models;
using ModelContextProtocol.Client;

namespace Azure.Mcp.Core.Areas.Server.Commands.Discovery;

/// <summary>
/// Provides an MCP server implementation based on registry configuration.
/// Supports stdio transport mechanism.
/// </summary>
/// <param name="id">The unique identifier for the server.</param>
/// <param name="serverInfo">Configuration information for the server.</param>
public sealed class RegistryServerProvider(string id, RegistryServerInfo serverInfo) : IMcpServerProvider
{
    private readonly string _id = id;
    private readonly RegistryServerInfo _serverInfo = serverInfo;

    /// <summary>
    /// Creates metadata that describes this registry-based server.
    /// </summary>
    /// <returns>A metadata object containing the server's identity and description.</returns>
    public McpServerMetadata CreateMetadata()
    {
        return new McpServerMetadata
        {
            Id = _id,
            Name = _id,
            Title = _serverInfo.Title,
            Description = _serverInfo.Description ?? string.Empty
        };
    }

    /// <inheritdoc/>
    public async Task<McpClient> CreateClientAsync(McpClientOptions clientOptions, CancellationToken cancellationToken)
    {
        Func<McpClientOptions, CancellationToken, Task<McpClient>>? clientFactory = null;

        // Determine which factory function to use based on configuration
        if (!string.IsNullOrWhiteSpace(_serverInfo.Url))
        {
            clientFactory = CreateHttpClientAsync;
        }
        else if (!string.IsNullOrWhiteSpace(_serverInfo.Type) && _serverInfo.Type.Equals("stdio", StringComparison.OrdinalIgnoreCase))
        {
            clientFactory = CreateStdioClientAsync;
        }

        if (clientFactory == null)
        {
            throw new ArgumentException($"Registry server '{_id}' does not have a valid transport type. Either 'url' for HTTP transport or 'type=stdio' with 'command' must be specified.");
        }

        try
        {
            return await clientFactory(clientOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrWhiteSpace(_serverInfo.InstallInstructions))
            {
                var errorWithInstructions = $"""
                    Failed to initialize the '{_id}' MCP tool.
                    This tool may require dependencies that are not installed.

                    Installation Instructions:
                    {_serverInfo.InstallInstructions}
                    """;

                throw new InvalidOperationException(errorWithInstructions.Trim(), ex);
            }

            throw new InvalidOperationException($"Failed to create MCP client for registry server '{_id}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Creates an MCP client that communicates with the server using Server-Sent Events (SSE).
    /// </summary>
    /// <param name="clientOptions">Options to configure the client behavior.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A configured MCP client using SSE transport.</returns>
    private async Task<McpClient> CreateHttpClientAsync(McpClientOptions clientOptions, CancellationToken cancellationToken)
    {
        var transportOptions = new HttpClientTransportOptions
        {
            Name = _id,
            Endpoint = new Uri(_serverInfo.Url!),
            TransportMode = HttpTransportMode.AutoDetect,
        };
        var clientTransport = new HttpClientTransport(transportOptions);
        return await McpClient.CreateAsync(clientTransport, clientOptions, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Creates an MCP client that communicates with the server using stdio (standard input/output).
    /// </summary>
    /// <param name="clientOptions">Options to configure the client behavior.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A configured MCP client using stdio transport.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the server configuration doesn't specify a valid command for stdio transport.</exception>
    private async Task<McpClient> CreateStdioClientAsync(McpClientOptions clientOptions, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_serverInfo.Command))
        {
            throw new InvalidOperationException($"Registry server '{_id}' does not have a valid command for stdio transport.");
        }

        // Merge current system environment variables with serverInfo.Env (serverInfo.Env overrides system)
        var env = Environment.GetEnvironmentVariables()
            .Cast<System.Collections.DictionaryEntry>()
            .ToDictionary(e => (string)e.Key, e => (string?)e.Value);

        if (_serverInfo.Env != null)
        {
            foreach (var kvp in _serverInfo.Env)
            {
                env[kvp.Key] = kvp.Value;
            }
        }

        var transportOptions = new StdioClientTransportOptions
        {
            Name = _id,
            Command = _serverInfo.Command,
            Arguments = _serverInfo.Args,
            EnvironmentVariables = env
        };

        var clientTransport = new StdioClientTransport(transportOptions);
        return await McpClient.CreateAsync(clientTransport, clientOptions, cancellationToken: cancellationToken);
    }
}
