// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace Azure.Mcp.Core.Areas.Server.Commands.ToolLoading;

/// <summary>
/// Base class for tool loaders that provides common functionality including disposal patterns.
/// </summary>
/// <param name="logger">Logger instance for this tool loader.</param>
public abstract class BaseToolLoader(ILogger logger) : IToolLoader
{
    /// <summary>
    /// Logger instance for this tool loader.
    /// </summary>
    protected readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Cached empty JSON object to avoid repeated parsing.
    /// </summary>
    protected static readonly JsonElement EmptyJsonObject = JsonDocument.Parse("{}").RootElement;

    private bool _disposed = false;

    /// <summary>
    /// Handles requests to list all tools available in the MCP server.
    /// </summary>
    /// <param name="request">The request context containing metadata and parameters.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing the list of available tools.</returns>
    public abstract ValueTask<ListToolsResult> ListToolsHandler(RequestContext<ListToolsRequestParams> request, CancellationToken cancellationToken);

    /// <summary>
    /// Handles requests to call a specific tool with the provided parameters.
    /// </summary>
    /// <param name="request">The request context containing the tool name and parameters.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing the output of the tool invocation.</returns>
    public abstract ValueTask<CallToolResult> CallToolHandler(RequestContext<CallToolRequestParams> request, CancellationToken cancellationToken);

    /// <summary>
    /// Extracts the "parameters" JsonElement from the tool call request arguments.
    /// </summary>
    /// <param name="request">The request context containing the tool call parameters.</param>
    /// <returns>
    /// The "parameters" JsonElement if it exists and is a valid JSON object;
    /// otherwise, returns an empty JSON object.
    /// </returns>
    protected static JsonElement GetParametersJsonElement(RequestContext<CallToolRequestParams> request)
    {
        IReadOnlyDictionary<string, JsonElement>? args = request.Params?.Arguments;
        if (args != null && args.TryGetValue("parameters", out var parametersElem) && parametersElem.ValueKind == JsonValueKind.Object)
        {
            return parametersElem;
        }

        return EmptyJsonObject;
    }

    /// <summary>
    /// Extracts the "parameters" object from the tool call request arguments and converts it to a dictionary.
    /// </summary>
    /// <param name="request">The request context containing the tool call parameters.</param>
    /// <returns>
    /// A dictionary containing the parameter names and values if the "parameters" object exists and is valid;
    /// otherwise, returns an empty dictionary.
    /// </returns>
    protected static Dictionary<string, object?> GetParametersDictionary(RequestContext<CallToolRequestParams> request)
    {
        JsonElement parametersElem = GetParametersJsonElement(request);
        return parametersElem.EnumerateObject().ToDictionary(prop => prop.Name, prop => (object?)prop.Value);
    }

    /// <summary>
    /// Disposes resources owned by this tool loader with double disposal protection.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            await DisposeAsyncCore();
        }
        catch (Exception ex)
        {
            // Log disposal failures but don't throw - we want to ensure cleanup continues
            // Individual disposal errors shouldn't stop the overall disposal process
            _logger.LogError(ex, "Error occurred while disposing tool loader {LoaderType}. Some resources may not have been properly disposed.", GetType().Name);
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
        // Default implementation does nothing
        return ValueTask.CompletedTask;
    }

    protected McpClientOptions CreateClientOptions(McpServer server)
    {
        McpClientHandlers handlers = new();

        if (server.ClientCapabilities?.Sampling != null)
        {
            handlers.SamplingHandler = (request, progress, token) =>
            {
                ArgumentNullException.ThrowIfNull(request);
                return server.SampleAsync(request, token);
            };
        }

        if (server.ClientCapabilities?.Elicitation != null)
        {
            handlers.ElicitationHandler = (request, token) =>
            {
                ArgumentNullException.ThrowIfNull(request);
                return server.ElicitAsync(request, token);
            };
        }

        var clientOptions = new McpClientOptions
        {
            ClientInfo = server.ClientInfo,
            Handlers = handlers
        };

        return clientOptions;
    }
}
