// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using ModelContextProtocol.Protocol;

namespace Azure.Mcp.Core.Areas.Server.Commands.Runtime;

/// <summary>
/// Defines the core functionality for a Model Context Protocol (MCP) runtime.
/// The runtime is responsible for handling tool discovery and invocation requests.
/// </summary>
public interface IMcpRuntime : IAsyncDisposable
{
    /// <summary>
    /// Handles requests to list all tools available in the MCP server.
    /// </summary>
    /// <param name="request">The request context containing metadata and parameters.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing the list of available tools.</returns>
    ValueTask<ListToolsResult> ListToolsHandler(RequestContext<ListToolsRequestParams> request, CancellationToken cancellationToken);

    /// <summary>
    /// Handles requests to call a specific tool with the provided parameters.
    /// </summary>
    /// <param name="request">The request context containing the tool name and parameters.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing the output of the tool invocation.</returns>
    ValueTask<CallToolResult> CallToolHandler(RequestContext<CallToolRequestParams> request, CancellationToken cancellationToken);
}
