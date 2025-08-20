// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;

namespace Azure.Mcp.Core.Areas.Server.Commands.ToolLoading;

/// <summary>
/// A tool loader that combines multiple tool loaders into one.
/// This allows aggregating tools from multiple sources and delegating tool calls to the appropriate loader.
/// </summary>
/// <param name="toolLoaders">The collection of tool loaders to combine.</param>
/// <param name="logger">Logger for tool loading operations.</param>
public sealed class CompositeToolLoader(IEnumerable<IToolLoader> toolLoaders, ILogger<CompositeToolLoader> logger) : BaseToolLoader(logger)
{
    private readonly IEnumerable<IToolLoader> _toolLoaders = InitializeToolLoaders(toolLoaders);
    private readonly Dictionary<string, IToolLoader> _toolLoaderMap = new();
    private readonly SemaphoreSlim _initializationSemaphore = new(1, 1);
    private bool _isInitialized = false;
    private List<Tool>? _cachedTools;

    /// <summary>
    /// Initializes the list of tool loaders, validating that at least one is provided.
    /// </summary>
    /// <param name="toolLoaders">The collection of tool loaders to initialize.</param>
    /// <returns>A list of initialized tool loaders.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the toolLoaders parameter is null.</exception>
    /// <exception cref="ArgumentException">Thrown when no tool loaders are provided.</exception>
    private static IEnumerable<IToolLoader> InitializeToolLoaders(IEnumerable<IToolLoader> toolLoaders)
    {
        ArgumentNullException.ThrowIfNull(toolLoaders);

        var loadersList = toolLoaders.ToList();

        if (loadersList.Count == 0)
        {
            throw new ArgumentException("At least one tool loader must be provided.", nameof(toolLoaders));
        }

        return loadersList;
    }

    /// <summary>
    /// Lists all tools from all tool loaders and builds a mapping of tool names to their respective loaders.
    /// </summary>
    /// <param name="request">The request context containing metadata and parameters.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing the combined list of all available tools, or an empty list if initialization fails.</returns>
    public override async ValueTask<ListToolsResult> ListToolsHandler(RequestContext<ListToolsRequestParams> request, CancellationToken cancellationToken)
    {
        try
        {
            await InitializeAsync(request.Server, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Tool loader initialization failed");

            // Return empty result with error indication
            return new ListToolsResult
            {
                Tools = []
            };
        }

        // Return cached result after initialization
        return new ListToolsResult
        {
            Tools = _cachedTools!
        };
    }

    /// <summary>
    /// Calls a tool by its name using the appropriate tool loader.
    /// </summary>
    /// <param name="request">The request context containing the tool name and parameters.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing the output of the tool invocation, or an error result if the tool is not found or initialization fails.</returns>
    public override async ValueTask<CallToolResult> CallToolHandler(RequestContext<CallToolRequestParams> request, CancellationToken cancellationToken)
    {
        if (request.Params == null)
        {
            var content = new TextContentBlock
            {
                Text = "Cannot call tools with null parameters.",
            };

            _logger.LogWarning(content.Text);

            return new CallToolResult
            {
                Content = [content],
                IsError = true,
            };
        }

        // Ensure tool loader map is populated before attempting tool lookup
        try
        {
            await InitializeAsync(request.Server, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            var content = new TextContentBlock
            {
                Text = $"Failed to initialize tool loaders: {ex.Message}",
            };

            _logger.LogError(ex, "Tool loader initialization failed");

            return new CallToolResult
            {
                Content = [content],
                IsError = true,
            };
        }

        if (!_toolLoaderMap.TryGetValue(request.Params.Name, out var toolCaller))
        {
            var content = new TextContentBlock
            {
                Text = $"The tool {request.Params.Name} was not found",
            };

            _logger.LogWarning(content.Text);

            return new CallToolResult
            {
                Content = [content],
                IsError = true,
            };
        }

        return await toolCaller.CallToolHandler(request, cancellationToken);
    }

    /// <summary>
    /// Initializes the tool loader map by discovering all tools from child loaders.
    /// This provides thread-safe initialization using the double-checked locking pattern.
    /// </summary>
    /// <param name="server">The server context for creating list tools requests.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task InitializeAsync(IMcpServer server, CancellationToken cancellationToken)
    {
        if (_isInitialized)
        {
            return;
        }

        await _initializationSemaphore.WaitAsync(cancellationToken);
        try
        {
            // Double-check pattern: verify we're still not initialized after acquiring the lock
            if (_isInitialized)
            {
                return;
            }

            // Populate the tool loader map and cache the combined tools
            var allTools = new List<Tool>();

            // Create a request for listing tools to populate the tool loader map
            var listToolsRequest = new RequestContext<ListToolsRequestParams>(server)
            {
                Params = new ListToolsRequestParams()
            };

            foreach (var loader in _toolLoaders)
            {
                var toolsResponse = await loader.ListToolsHandler(listToolsRequest, cancellationToken);
                if (toolsResponse == null)
                {
                    throw new InvalidOperationException("Tool loader returned null response during initialization.");
                }

                foreach (var tool in toolsResponse.Tools)
                {
                    _toolLoaderMap[tool.Name] = loader;
                    allTools.Add(tool);
                }
            }

            _cachedTools = allTools;
            _isInitialized = true;
        }
        finally
        {
            _initializationSemaphore.Release();
        }
    }

    /// <summary>
    /// Disposes all child tool loaders and the initialization semaphore.
    /// Uses best-effort disposal to ensure all loaders are disposed even if some fail.
    /// </summary>
    protected override async ValueTask DisposeAsyncCore()
    {
        // Dispose initialization semaphore first
        _initializationSemaphore?.Dispose();

        // Dispose all child loaders using best-effort approach
        var childDisposalTasks = _toolLoaders.Select(async loader =>
        {
            try
            {
                await loader.DisposeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dispose tool loader {LoaderType}", loader.GetType().Name);
            }
        });

        await Task.WhenAll(childDisposalTasks);
    }
}
