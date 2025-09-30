// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;

namespace Azure.Mcp.Core.Areas.Server.Commands.Discovery;

/// <summary>
/// Base class for MCP server discovery strategies that provides common functionality.
/// Implements client caching and server provider lookup by name.
/// </summary>
public abstract class BaseDiscoveryStrategy(ILogger logger) : IMcpDiscoveryStrategy
{
    /// <summary>
    /// Logger instance for this discovery strategy.
    /// </summary>
    protected readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    /// <summary>
    /// Cache of MCP clients created by this discovery strategy, keyed by server name (case-insensitive).
    /// </summary>
    protected readonly Dictionary<string, McpClient> _clientCache = new(StringComparer.OrdinalIgnoreCase);

    private bool _disposed = false;

    /// <summary>
    /// Discovers available MCP servers via the implementing strategy.
    /// </summary>
    /// <returns>A collection of discovered MCP server providers.</returns>
    public abstract Task<IEnumerable<IMcpServerProvider>> DiscoverServersAsync();

    /// <summary>
    /// Finds a server provider by name from the discovered servers.
    /// </summary>
    /// <param name="name">The name of the server to find.</param>
    /// <returns>The server provider if found.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no server with the specified name is found.</exception>
    public async Task<IMcpServerProvider> FindServerProviderAsync(string name)
    {
        ArgumentNullException.ThrowIfNull(name, nameof(name));
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name), "Server name cannot be null or empty.");
        }

        var serverProviders = await DiscoverServersAsync();
        foreach (var serverProvider in serverProviders)
        {
            var metadata = serverProvider.CreateMetadata();
            if (metadata.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return serverProvider;
            }
        }

        throw new KeyNotFoundException($"No MCP server found with the name '{name}'.");
    }

    /// <summary>
    /// Gets an existing MCP client from the cache or creates a new one if not found.
    /// </summary>
    /// <param name="name">The name of the server to get or create a client for.</param>
    /// <param name="clientOptions">Optional client configuration options. If null, default options are used.</param>
    /// <returns>An MCP client that can communicate with the specified server.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the name parameter is null.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when no server with the specified name is found.</exception>
    public async Task<McpClient> GetOrCreateClientAsync(string name, McpClientOptions? clientOptions = null)
    {
        ArgumentNullException.ThrowIfNull(name, nameof(name));
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name), "Server name cannot be null or empty.");
        }

        if (_clientCache.TryGetValue(name, out var client))
        {
            return client;
        }

        var serverProvider = await FindServerProviderAsync(name);
        client = await serverProvider.CreateClientAsync(clientOptions ?? new McpClientOptions());
        _clientCache[name] = client;

        return client;
    }

    /// <summary>
    /// Disposes all cached MCP clients with double disposal protection.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            // First, let derived classes dispose their resources (isolated from base cleanup)
            try
            {
                await DisposeAsyncCore();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while disposing derived resources in discovery strategy {StrategyType}", GetType().Name);
            }

            // Then dispose our own critical resources using best-effort approach
            var clientDisposalTasks = _clientCache.Values.Select(async client =>
            {
                try
                {
                    await client.DisposeAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to dispose MCP client in discovery strategy {StrategyType}", GetType().Name);
                }
            });

            await Task.WhenAll(clientDisposalTasks);
            _clientCache.Clear();
        }
        catch (Exception ex)
        {
            // Log disposal failures but don't throw - we want to ensure cleanup continues
            // Individual disposal errors shouldn't stop the overall disposal process
            _logger.LogError(ex, "Error occurred while disposing discovery strategy {StrategyType}. Some resources may not have been properly disposed.", GetType().Name);
        }
        finally
        {
            _disposed = true;
        }
    }

    /// <summary>
    /// Override this method in derived classes to implement disposal logic.
    /// This method is called exactly once during disposal.
    /// </summary>
    /// <returns>A task representing the asynchronous disposal operation.</returns>
    protected virtual ValueTask DisposeAsyncCore()
    {
        // Default implementation does nothing - derived classes override to add their specific cleanup
        return ValueTask.CompletedTask;
    }
}
