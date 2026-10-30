// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;

namespace Microsoft.Mcp.Core.Areas.Server.Commands.Discovery;

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

    private readonly object _clientCacheLock = new();
    private readonly Dictionary<string, Task<McpClient>> _clientCreations = new(StringComparer.OrdinalIgnoreCase);
    private readonly CancellationTokenSource _clientCreationCancellation = new();
    private TaskCompletionSource? _disposeCompletion;

    /// <inheritdoc/>
    public abstract Task<IEnumerable<IMcpServerProvider>> DiscoverServersAsync(CancellationToken cancellationToken);

    /// <inheritdoc/>
    public async Task<IMcpServerProvider> FindServerProviderAsync(string name, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(name, nameof(name));
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name), "Server name cannot be null or empty.");
        }

        var serverProviders = await DiscoverServersAsync(cancellationToken);
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

    /// <inheritdoc/>
    public async Task<McpClient> GetOrCreateClientAsync(string name, McpClientOptions? clientOptions = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(name, nameof(name));
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name), "Server name cannot be null or empty.");
        }

        Task<McpClient> creationTask;
        TaskCompletionSource<McpClient>? creation = null;
        lock (_clientCacheLock)
        {
            ObjectDisposedException.ThrowIf(_disposeCompletion != null, this);
            if (_clientCache.TryGetValue(name, out var client))
            {
                return client;
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (!_clientCreations.TryGetValue(name, out creationTask!))
            {
                creation = new TaskCompletionSource<McpClient>(TaskCreationOptions.RunContinuationsAsynchronously);
                creationTask = creation.Task;
                _clientCreations.Add(name, creationTask);
            }
        }

        if (creation != null)
        {
            _ = CreateClientAsync(name, clientOptions, creation);
        }

        return await creationTask.WaitAsync(cancellationToken);
    }

    private async Task CreateClientAsync(string name, McpClientOptions? clientOptions, TaskCompletionSource<McpClient> creation)
    {
        try
        {
            var cancellationToken = _clientCreationCancellation.Token;
            cancellationToken.ThrowIfCancellationRequested();
            var serverProvider = await FindServerProviderAsync(name, cancellationToken);
            var client = await serverProvider.CreateClientAsync(clientOptions ?? new McpClientOptions(), cancellationToken);
            lock (_clientCacheLock)
            {
                _clientCache[name] = client;
                _clientCreations.Remove(name);
            }
            creation.SetResult(client);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogDebug(ex, "Client initialization canceled for MCP server {ServerName}.", name);
            lock (_clientCacheLock)
            {
                _clientCreations.Remove(name);
            }
            creation.SetCanceled(ex.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize client for MCP server {ServerName}.", name);
            lock (_clientCacheLock)
            {
                _clientCreations.Remove(name);
            }
            creation.SetException(ex);
        }
    }

    /// <summary>
    /// Cancels pending initialization and disposes cached clients once, sharing completion with concurrent callers.
    /// </summary>
    public ValueTask DisposeAsync()
    {
        TaskCompletionSource completion;
        Task<McpClient>[] creations;
        lock (_clientCacheLock)
        {
            if (_disposeCompletion != null)
            {
                return new ValueTask(_disposeCompletion.Task);
            }

            completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            _disposeCompletion = completion;
            creations = [.. _clientCreations.Values];
        }

        _ = DisposeAsyncOnce(creations, completion);
        return new ValueTask(completion.Task);
    }

    private async Task DisposeAsyncOnce(Task<McpClient>[] creations, TaskCompletionSource completion)
    {
        try
        {
            try
            {
                await _clientCreationCancellation.CancelAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel pending MCP client initialization.");
            }

            try
            {
                await Task.WhenAll(creations);
            }
            catch (OperationCanceledException) when (_clientCreationCancellation.IsCancellationRequested)
            {
                _logger.LogDebug("Pending MCP client initialization was canceled during disposal.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MCP client initialization failed during disposal.");
            }

            try
            {
                await DisposeAsyncCore();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while disposing derived resources in discovery strategy {StrategyType}", GetType().Name);
            }

            McpClient[] clients;
            lock (_clientCacheLock)
            {
                clients = [.. _clientCache.Values];
                _clientCache.Clear();
            }

            var clientDisposalTasks = clients.Select(async client =>
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
        }
        catch (Exception ex)
        {
            // Log disposal failures but don't throw - we want to ensure cleanup continues
            // Individual disposal errors shouldn't stop the overall disposal process
            _logger.LogError(ex, "Error occurred while disposing discovery strategy {StrategyType}. Some resources may not have been properly disposed.", GetType().Name);
        }
        finally
        {
            _clientCreationCancellation.Dispose();
            completion.SetResult();
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
