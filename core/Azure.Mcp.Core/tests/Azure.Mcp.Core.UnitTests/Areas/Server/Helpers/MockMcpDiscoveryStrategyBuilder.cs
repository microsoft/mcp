// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Commands.Discovery;
using ModelContextProtocol.Client;
using NSubstitute;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Helpers;

/// <summary>
/// A builder for creating mock implementations of <see cref="IMcpDiscoveryStrategy"/> for testing purposes.
/// Provides a fluent API for adding servers and configuring mock behavior.
/// </summary>
public sealed class MockMcpDiscoveryStrategyBuilder
{
    private readonly List<IMcpServerProvider> _providers = new();

    /// <summary>
    /// Initializes a new instance of the MockMcpDiscoveryStrategyBuilder.
    /// </summary>
    public MockMcpDiscoveryStrategyBuilder()
    {
    }

    /// <summary>
    /// Adds a new server with the specified client to the mock discovery strategy.
    /// </summary>
    /// <param name="serverId">The unique identifier for the server.</param>
    /// <param name="serverName">The display name of the server. If null, uses the serverId.</param>
    /// <param name="description">The description of the server. If null, uses a default description.</param>
    /// <param name="client">The mock client to return for this server.</param>
    /// <returns>The current instance for method chaining.</returns>
    public MockMcpDiscoveryStrategyBuilder AddServer(string serverId, string? serverName = null, string? description = null, IMcpClient? client = null)
    {
        var mockProvider = Substitute.For<IMcpServerProvider>();
        var metadata = new McpServerMetadata
        {
            Id = serverId,
            Name = serverName ?? serverId,
            Description = description ?? $"Mock server for {serverId}"
        };

        mockProvider.CreateMetadata().Returns(metadata);

        if (client != null)
        {
            mockProvider.CreateClientAsync(Arg.Any<McpClientOptions>()).Returns(Task.FromResult(client));
        }
        else
        {
            // If no client is provided, create a basic substitute
            var defaultClient = Substitute.For<IMcpClient>();
            mockProvider.CreateClientAsync(Arg.Any<McpClientOptions>()).Returns(Task.FromResult(defaultClient));
        }

        _providers.Add(mockProvider);
        return this;
    }

    /// <summary>
    /// Adds a new server using a MockMcpClientBuilder for the client.
    /// </summary>
    /// <param name="serverId">The unique identifier for the server.</param>
    /// <param name="serverName">The display name of the server. If null, uses the serverId.</param>
    /// <param name="description">The description of the server. If null, uses a default description.</param>
    /// <param name="clientBuilder">The MockMcpClientBuilder to use for creating the client.</param>
    /// <returns>The current instance for method chaining.</returns>
    public MockMcpDiscoveryStrategyBuilder AddServer(string serverId, string? serverName, string? description, MockMcpClientBuilder clientBuilder)
    {
        var client = clientBuilder.Build();
        return AddServer(serverId, serverName, description, client);
    }

    /// <summary>
    /// Adds a new server using a MockMcpClientBuilder for the client.
    /// </summary>
    /// <param name="serverId">The unique identifier for the server.</param>
    /// <param name="clientBuilder">The MockMcpClientBuilder to use for creating the client.</param>
    /// <returns>The current instance for method chaining.</returns>
    public MockMcpDiscoveryStrategyBuilder AddServer(string serverId, MockMcpClientBuilder clientBuilder)
    {
        return AddServer(serverId, null, null, clientBuilder);
    }

    /// <summary>
    /// Removes a server from the mock discovery strategy.
    /// </summary>
    /// <param name="serverId">The unique identifier of the server to remove.</param>
    /// <returns>The current instance for method chaining.</returns>
    public MockMcpDiscoveryStrategyBuilder RemoveServer(string serverId)
    {
        var providerToRemove = _providers.FirstOrDefault(p =>
            p.CreateMetadata().Id.Equals(serverId, StringComparison.OrdinalIgnoreCase));

        if (providerToRemove != null)
        {
            _providers.Remove(providerToRemove);
        }

        return this;
    }

    /// <summary>
    /// Clears all servers from the mock discovery strategy.
    /// </summary>
    /// <returns>The current instance for method chaining.</returns>
    public MockMcpDiscoveryStrategyBuilder ClearServers()
    {
        _providers.Clear();
        return this;
    }



    /// <summary>
    /// Builds and returns the configured mock discovery strategy.
    /// </summary>
    /// <returns>The configured mock discovery strategy.</returns>
    public IMcpDiscoveryStrategy Build()
    {
        var mockStrategy = Substitute.For<IMcpDiscoveryStrategy>();

        // Configure DiscoverServersAsync to return the current providers
        mockStrategy.DiscoverServersAsync().Returns(Task.FromResult<IEnumerable<IMcpServerProvider>>(_providers));

        // Configure FindServerProviderAsync to find providers by name (case-insensitive)
        mockStrategy.FindServerProviderAsync(Arg.Any<string>()).Returns(callInfo =>
        {
            var serverName = callInfo.Arg<string>();
            var provider = _providers.FirstOrDefault(p =>
                p.CreateMetadata().Name.Equals(serverName, StringComparison.OrdinalIgnoreCase));

            if (provider == null)
            {
                throw new KeyNotFoundException($"No MCP server found with the name '{serverName}'.");
            }

            return Task.FromResult(provider);
        });

        // Configure GetOrCreateClientAsync to return the appropriate client
        mockStrategy.GetOrCreateClientAsync(Arg.Any<string>(), Arg.Any<McpClientOptions>()).Returns(callInfo =>
        {
            var serverName = callInfo.Arg<string>();
            var clientOptions = callInfo.ArgAt<McpClientOptions>(1) ?? new McpClientOptions();

            var provider = _providers.FirstOrDefault(p =>
                p.CreateMetadata().Name.Equals(serverName, StringComparison.OrdinalIgnoreCase));

            if (provider == null)
            {
                throw new KeyNotFoundException($"No MCP server found with the name '{serverName}'.");
            }

            // Return the client from the provider
            return provider.CreateClientAsync(clientOptions);
        });

        return mockStrategy;
    }
}
