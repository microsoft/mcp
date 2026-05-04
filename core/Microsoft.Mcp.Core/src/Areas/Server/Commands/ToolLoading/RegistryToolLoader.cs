// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas.Server.Commands.Discovery;
using Microsoft.Mcp.Core.Commands;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;

/// <summary>
/// RegistryToolLoader is a tool loader that retrieves tools from a registry.
/// Tools are loaded from each MCP server and exposed through the MCP server.
/// It handles tool call proxying and provides a unified interface for tool operations.
/// </summary>
public sealed class RegistryToolLoader(
    IMcpDiscoveryStrategy discoveryStrategy,
    IOptions<ToolLoaderOptions> options,
    ILogger<RegistryToolLoader> logger) : BaseToolLoader(logger)
{
    private readonly IMcpDiscoveryStrategy _serverDiscoveryStrategy = discoveryStrategy;
    private readonly IOptions<ToolLoaderOptions> _options = options;
    private Dictionary<string, (string ServerName, string OriginalToolName, McpClient Client, Tool Tool)> _toolClientMap = [];
    private List<McpClient> _discoveredClients = [];
    private Dictionary<McpClient, string?> _clientPrefixMap = [];
    private readonly SemaphoreSlim _initializationSemaphore = new(1, 1);
    private bool _isInitialized = false;

    /// <summary>
    /// Gets or sets the client options used when creating MCP clients.
    /// </summary>
    public McpClientOptions ClientOptions { get; set; } = new McpClientOptions();

    /// <summary>
    /// Lists all tools available from registered MCP servers.
    /// </summary>
    /// <param name="request">The request context containing parameters and metadata.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A result containing the list of available tools.</returns>
    public override async ValueTask<ListToolsResult> ListToolsHandler(RequestContext<ListToolsRequestParams> request, CancellationToken cancellationToken)
    {
        await InitializeAsync(cancellationToken);

        var allToolsResponse = new ListToolsResult
        {
            Tools = []
        };

        // Use cached discovered clients instead of re-discovering servers
        foreach (var mcpClient in _discoveredClients)
        {
            var toolsResponse = await mcpClient.ListToolsAsync(cancellationToken: cancellationToken);
            var filteredTools = toolsResponse
                .Select(t => t.ProtocolTool)
                .Where(t => !_options.Value.ReadOnly || (t.Annotations?.ReadOnlyHint == true))
                .Where(t => !_options.Value.IsHttpMode || !HasLocalRequiredHint(t.Meta));

            // Filter by specific tools if provided
            if (_options.Value.Tool != null && _options.Value.Tool.Length > 0)
            {
                filteredTools = filteredTools.Where(t => _options.Value.Tool.Any(tool => tool.Contains(t.Name, StringComparison.OrdinalIgnoreCase)));
            }

            var prefix = _clientPrefixMap.TryGetValue(mcpClient, out var p) ? p : null;
            foreach (var tool in filteredTools)
            {
                var exposedTool = string.IsNullOrEmpty(prefix)
                    ? tool
                    : new Tool { Name = prefix + tool.Name, Description = tool.Description, InputSchema = tool.InputSchema, Annotations = tool.Annotations };
                allToolsResponse.Tools.Add(exposedTool);
            }
        }

        return allToolsResponse;
    }

    /// <summary>
    /// Handles tool calls by routing them to the appropriate MCP client.
    /// </summary>
    /// <param name="request">The request context containing parameters and metadata.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The result of the tool call operation.</returns>
    public override async ValueTask<CallToolResult> CallToolHandler(RequestContext<CallToolRequestParams> request, CancellationToken cancellationToken)
    {
        Activity.Current?.SetTag(TagName.IsServerCommandInvoked, false);
        if (request.Params == null)
        {
            var content = new TextContentBlock
            {
                Text = "Cannot call tools with null parameters.",
            };

            return new CallToolResult
            {
                Content = [content],
                IsError = true,
            };
        }

        // Initialize the tool client map if not already done
        await InitializeAsync(cancellationToken);

        // Check if tool filtering is enabled and validate the requested tool
        if (_options.Value.Tool != null && _options.Value.Tool.Length > 0)
        {
            if (!_options.Value.Tool.Any(tool => tool.Contains(request.Params.Name, StringComparison.OrdinalIgnoreCase)))
            {
                var content = new TextContentBlock
                {
                    Text = $"Tool '{request.Params.Name}' is not available. This server is configured to only expose the tools: {string.Join(", ", _options.Value.Tool.Select(t => $"'{t}'"))}",
                };

                return new CallToolResult
                {
                    Content = [content],
                    IsError = true,
                };
            }
        }

        if (!_toolClientMap.TryGetValue(request.Params.Name, out var kvp) || kvp.Client is null)
        {
            var content = new TextContentBlock
            {
                Text = $"The tool {request.Params.Name} was not found in the tool registry.",
            };

            return new CallToolResult
            {
                Content = [content],
                IsError = true,
            };
        }

        // Enforce read-only mode at execution time
        if (_options.Value.ReadOnly && kvp.Tool.Annotations?.ReadOnlyHint != true)
        {
            var content = new TextContentBlock
            {
                Text = $"Tool '{request.Params.Name}' is not available. This server is configured in read-only mode and this tool is not a read-only tool.",
            };

            return new CallToolResult
            {
                Content = [content],
                IsError = true,
            };
        }

        // Enforce HTTP mode restrictions at execution time
        if (_options.Value.IsHttpMode && HasLocalRequiredHint(kvp.Tool.Meta))
        {
            var content = new TextContentBlock
            {
                Text = $"Tool '{request.Params.Name}' is not available. This server is running in HTTP mode and this tool requires local execution.",
            };

            return new CallToolResult
            {
                Content = [content],
                IsError = true,
            };
        }

        // For MCP servers loaded from registry.json, the ToolArea is also its "server name".
        Activity.Current?.SetTag(TagName.ToolArea, kvp.ServerName)
            .SetTag(TagName.ToolName, request.Params.Name)
            .SetTag(TagName.IsServerCommandInvoked, true);

        var parameters = TransformArgumentsToDictionary(request.Params.Arguments);
        return await kvp.Client.CallToolAsync(kvp.OriginalToolName, parameters, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Transforms tool call arguments to a parameters dictionary.
    /// This transformation is used because McpClientExtensions.CallToolAsync expects parameters as Dictionary&lt;string, object?&gt;.
    /// </summary>
    /// <param name="args">The arguments to transform to parameters.</param>
    /// <returns>A dictionary of parameter names and values compatible with McpClientExtensions.CallToolAsync.</returns>
    private static Dictionary<string, object?> TransformArgumentsToDictionary(IDictionary<string, JsonElement>? args)
    {
        if (args == null)
        {
            return [];
        }

        return args.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value);
    }

    /// <summary>
    /// Initializes the tool client map by discovering servers and populating tools.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (_isInitialized)
        {
            return;
        }

        // When running under a test proxy (TEST_PROXY_URL is set by the test infrastructure),
        // every outgoing HTTP request is redirected through the proxy by RecordingRedirectHandler.
        // External registry server connections (e.g. mcp.ai.azure.com) would therefore hit the
        // test proxy during an active recording/playback session, either producing unrecorded
        // traffic in playback mode or polluting the recording sequence in record mode. Skip
        // registry initialization entirely so only the local in-process tools are loaded.
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEST_PROXY_URL")))
        {
            _logger.LogDebug("Skipping registry server initialization: TEST_PROXY_URL is set (running under test proxy).");
            _isInitialized = true;
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

            var startTime = Stopwatch.GetTimestamp();
            _logger.LogInformation("Starting server discovery and initialization...");

            var serverList = await _serverDiscoveryStrategy.DiscoverServersAsync(cancellationToken);
            var serverArray = serverList.ToArray();
            _logger.LogInformation("Discovered {ServerCount} servers. Beginning parallel client initialization...", serverArray.Length);

            // Parallelize server initialization to reduce startup time
            var initializationTasks = serverArray.Select(async server =>
            {
                var serverMetadata = server.CreateMetadata();
                try
                {
                    McpClient mcpClient;
                    try
                    {
                        mcpClient = await _serverDiscoveryStrategy.GetOrCreateClientAsync(serverMetadata.Name, ClientOptions, cancellationToken);

                        if (mcpClient == null)
                        {
                            _logger.LogWarning("Failed to get MCP client for provider {ProviderName}.", serverMetadata.Name);
                            return (serverMetadata.Name, serverMetadata.ToolPrefix, null, (IEnumerable<Tool>?)null);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Rethrow cancellation to prevent setting _isInitialized
                        throw;
                    }
                    catch (InvalidOperationException ex)
                    {
                        _logger.LogWarning("Failed to create client for provider {ProviderName}: {Error}", serverMetadata.Name, ex.Message);
                        return (serverMetadata.Name, serverMetadata.ToolPrefix, null, (IEnumerable<Tool>?)null);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Failed to start client for provider {ProviderName}: {Error}", serverMetadata.Name, ex.Message);
                        return (serverMetadata.Name, serverMetadata.ToolPrefix, null, (IEnumerable<Tool>?)null);
                    }

                    try
                    {
                        var toolsResponse = await mcpClient.ListToolsAsync(cancellationToken: cancellationToken);
                        var allTools = toolsResponse
                            .Select(t => t.ProtocolTool)
                            .ToArray();

                        return (serverMetadata.Name, serverMetadata.ToolPrefix, mcpClient, (IEnumerable<Tool>?)allTools);
                    }
                    catch (OperationCanceledException)
                    {
                        // Rethrow cancellation to prevent setting _isInitialized
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Failed to list tools for provider {ProviderName}: {Error}", serverMetadata.Name, ex.Message);
                        return (serverMetadata.Name, serverMetadata.ToolPrefix, (McpClient?)null, (IEnumerable<Tool>?)null);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Rethrow cancellation to prevent setting _isInitialized
                    throw;
                }
            });

            var results = await Task.WhenAll(initializationTasks);

            var successCount = 0;
            var toolCount = 0;

            // Process results and populate the client cache and tool map
            foreach (var (serverName, toolPrefix, mcpClient, tools) in results)
            {
                if (mcpClient != null && tools != null)
                {
                    _discoveredClients.Add(mcpClient);
                    _clientPrefixMap[mcpClient] = toolPrefix;

                    foreach (var tool in tools)
                    {
                        var exposedName = string.IsNullOrEmpty(toolPrefix) ? tool.Name : toolPrefix + tool.Name;
                        _toolClientMap[exposedName] = (serverName, tool.Name, mcpClient, tool);
                        toolCount++;
                    }
                    successCount++;
                }
            }

            var elapsedMs = Stopwatch.GetElapsedTime(startTime).TotalMilliseconds;
            _logger.LogInformation(
                "Server initialization complete. Successfully initialized {SuccessCount}/{TotalCount} servers with {ToolCount} tools in {ElapsedMs:F0}ms",
                successCount, serverArray.Length, toolCount, elapsedMs);

            _isInitialized = true;
        }
        finally
        {
            _initializationSemaphore.Release();
        }
    }

    private static bool HasLocalRequiredHint(JsonObject? meta)
    {
        if (meta != null && meta.TryGetPropertyValue("LocalRequiredHint", out var localRequired))
        {
            return localRequired?.GetValueKind() == JsonValueKind.True;
        }
        return false;
    }

    /// <summary>
    /// Disposes resources owned by this tool loader.
    /// Clears collections and disposes the initialization semaphore.
    /// Note: MCP clients are owned by the discovery strategy, not disposed here.
    /// </summary>
    protected override async ValueTask DisposeAsyncCore()
    {
        // Only dispose resources we own, not the MCP clients
        _initializationSemaphore?.Dispose();

        // Clear references to clients (but don't dispose them - discovery strategy owns them)
        _discoveredClients.Clear();
        _toolClientMap.Clear();
        _clientPrefixMap.Clear();

        await ValueTask.CompletedTask;
    }
}
