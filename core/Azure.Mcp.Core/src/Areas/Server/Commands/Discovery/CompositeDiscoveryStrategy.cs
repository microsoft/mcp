// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Core.Areas.Server.Commands.Discovery;

/// <summary>
/// A discovery strategy that combines multiple discovery strategies into one.
/// This allows discovering servers from multiple sources and aggregating the results.
/// </summary>
/// <param name="strategies">The collection of discovery strategies to combine.</param>
/// <param name="logger">Logger instance for this discovery strategy.</param>
public sealed class CompositeDiscoveryStrategy(IEnumerable<IMcpDiscoveryStrategy> strategies, ILogger<CompositeDiscoveryStrategy> logger) : BaseDiscoveryStrategy(logger)
{
    private readonly List<IMcpDiscoveryStrategy> _strategies = InitializeStrategies(strategies);

    /// <summary>
    /// Initializes the list of discovery strategies, validating that at least one is provided.
    /// </summary>
    /// <param name="strategies">The collection of discovery strategies to initialize.</param>
    /// <returns>A list of initialized discovery strategies.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the strategies parameter is null.</exception>
    /// <exception cref="ArgumentException">Thrown when no discovery strategies are provided.</exception>
    private static List<IMcpDiscoveryStrategy> InitializeStrategies(IEnumerable<IMcpDiscoveryStrategy> strategies)
    {
        ArgumentNullException.ThrowIfNull(strategies);

        var strategyList = new List<IMcpDiscoveryStrategy>(strategies);

        if (strategyList.Count == 0)
        {
            throw new ArgumentException("At least one discovery strategy must be provided.", nameof(strategies));
        }

        return strategyList;
    }

    /// <summary>
    /// Discovers available MCP servers from all combined discovery strategies.
    /// </summary>
    /// <returns>A collection of all discovered MCP server providers from all strategies.</returns>
    public override async Task<IEnumerable<IMcpServerProvider>> DiscoverServersAsync()
    {
        var tasks = _strategies.Select(strategy => strategy.DiscoverServersAsync());
        var results = await Task.WhenAll(tasks);

        return results.SelectMany(result => result);
    }

    /// <summary>
    /// Disposes all child discovery strategies and then calls the base dispose.
    /// Uses best-effort disposal to ensure all strategies are disposed even if some fail.
    /// </summary>
    protected override async ValueTask DisposeAsyncCore()
    {
        // Dispose all child strategies using best-effort approach
        var childDisposalTasks = _strategies.Select(async strategy =>
        {
            try
            {
                await strategy.DisposeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dispose discovery strategy {StrategyType}", strategy.GetType().Name);
            }
        });

        await Task.WhenAll(childDisposalTasks);
    }
}
