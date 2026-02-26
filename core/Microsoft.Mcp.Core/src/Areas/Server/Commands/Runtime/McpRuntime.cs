// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text.Json.Nodes;
using Azure.Mcp.Core.Models.Option;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Services.Telemetry;
using ModelContextProtocol.Protocol;

namespace Microsoft.Mcp.Core.Areas.Server.Commands.Runtime;

/// <summary>
/// Implementation of the MCP runtime that delegates tool discovery and invocation to a tool loader.
/// Provides logging and configuration support for the MCP server.
/// </summary>
public sealed class McpRuntime : IMcpRuntime
{
    private readonly IToolLoader _toolLoader;
    private readonly IOptions<ServiceStartOptions> _options;
    private readonly ILogger<McpRuntime> _logger;

    private readonly ITelemetryService _telemetry;

    /// <summary>
    /// Initializes a new instance of the McpRuntime class.
    /// </summary>
    /// <param name="toolLoader">The tool loader responsible for discovering and loading tools.</param>
    /// <param name="options">Configuration options for the MCP server.</param>
    /// <param name="logger">Logger for runtime operations.</param>
    /// <exception cref="ArgumentNullException">Thrown if any required dependencies are null.</exception>
    public McpRuntime(
        IToolLoader toolLoader,
        IOptions<ServiceStartOptions> options,
        ITelemetryService telemetry,
        ILogger<McpRuntime> logger)
    {
        _toolLoader = toolLoader ?? throw new ArgumentNullException(nameof(toolLoader));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation("McpRuntime initialized with tool loader of type {ToolLoaderType}.", _toolLoader.GetType().Name);
        _logger.LogInformation("ReadOnly mode is set to {ReadOnly}.", _options.Value.ReadOnly ?? false);
        _logger.LogInformation("Namespace is set to {Namespace}.", string.Join(",", _options.Value.Namespace ?? []));
    }

    /// <summary>
    /// Delegates tool invocation requests to the configured tool loader.
    /// </summary>
    /// <param name="request">The request context containing the tool name and parameters.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing the output of the tool invocation.</returns>
    public async ValueTask<CallToolResult> CallToolHandler(RequestContext<CallToolRequestParams> request, CancellationToken cancellationToken)
    {
        using var activity = _telemetry.StartActivity(ActivityName.ToolExecuted, request.Server.ClientInfo);
        CaptureToolCallMeta(activity, request.Params?.Meta);

        if (request.Params == null)
        {
            var content = new TextContentBlock
            {
                Text = "Cannot call tools with null parameters.",
            };

            activity?.SetStatus(ActivityStatusCode.Error);

            return new CallToolResult
            {
                Content = [content],
                IsError = true,
            };
        }

        activity?.AddTag(TagName.ToolName, request.Params.Name);

        var symbol = OptionDefinitions.Common.Subscription;

        var subscriptionArgument = request.Params?.Arguments?
            .Where(kvp => string.Equals(kvp.Key, NameNormalization.NormalizeOptionName(symbol.Name), StringComparison.OrdinalIgnoreCase))
            .Select(kvp => kvp.Value)
            .FirstOrDefault();
        if (subscriptionArgument != null
            && subscriptionArgument.HasValue
            && subscriptionArgument.Value.ValueKind == JsonValueKind.String)
        {
            var subscription = subscriptionArgument.Value.GetString();
            if (subscription != null)
            {
                activity?.AddTag(AzureTagName.SubscriptionGuid, subscription);
            }
        }

        CallToolResult callTool;
        try
        {
            callTool = await _toolLoader.CallToolHandler(request!, cancellationToken);

            var isSuccessful = !callTool.IsError.HasValue || !callTool.IsError.Value;
            if (isSuccessful)
            {
                activity?.SetStatus(ActivityStatusCode.Ok);
                return callTool;
            }

            activity?.SetStatus(ActivityStatusCode.Error);

            return callTool;
        }
        // Catches scenarios where child MCP clients are unable to be created
        // due to missing dependencies or misconfiguration.
        catch (InvalidOperationException ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Exception occurred calling tool handler");

            return new CallToolResult
            {
                Content = [new TextContentBlock
                {
                    Text = ex.Message,
                }],
                IsError = true,
            };
        }
        catch (Exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Exception occurred calling tool handler");
            throw;
        }
    }

    private static void CaptureToolCallMeta(Activity? activity, JsonObject? meta)
    {
        if (activity != null && meta != null)
        {
            var vsCodeConversationIdNode = meta["vscode.conversationId"];
            if (vsCodeConversationIdNode != null && vsCodeConversationIdNode.GetValueKind() == JsonValueKind.String)
            {
                activity.AddTag(TagName.VSCodeConversationId, vsCodeConversationIdNode.GetValue<string>());
            }
            var vsCodeRequestIdNode = meta["vscode.requestId"];
            if (vsCodeRequestIdNode != null && vsCodeRequestIdNode.GetValueKind() == JsonValueKind.String)
            {
                activity.AddTag(TagName.VSCodeRequestId, vsCodeRequestIdNode.GetValue<string>());
            }
        }
    }

    /// <summary>
    /// Delegates tool discovery requests to the configured tool loader.
    /// </summary>
    /// <param name="request">The request context containing metadata and parameters.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing the list of available tools.</returns>
    public async ValueTask<ListToolsResult> ListToolsHandler(RequestContext<ListToolsRequestParams> request, CancellationToken cancellationToken)
    {
        using var activity = _telemetry.StartActivity(ActivityName.ListToolsHandler, request.Server.ClientInfo);

        try
        {
            var result = await _toolLoader.ListToolsHandler(request!, cancellationToken);
            activity?.SetStatus(ActivityStatusCode.Ok);

            return result;
        }
        catch (Exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Exception occurred calling list tools handler");
            throw;
        }
    }

    /// <summary>
    /// Disposes the tool loader and releases associated resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await _toolLoader.DisposeAsync();
    }
}
