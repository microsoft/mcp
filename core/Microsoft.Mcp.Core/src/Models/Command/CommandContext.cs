// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Microsoft.Mcp.Core.Models.Command;

/// <summary>
/// Provides context for command execution including service access and response management
/// </summary>
public class CommandContext
{
    /// <summary>
    /// The service provider for dependency injection
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// The response object that will be returned to the client
    /// </summary>
    public CommandResponse Response { get; }

    /// <summary>
    /// Current telemetry context if there is one available.
    /// </summary>
    public Activity? Activity { get; }

    /// <summary>
    /// The MCP server handling the current tool call. Used by commands that need to send
    /// progress notifications or invoke sampling (deprecated in MCP 2026-07-28).
    /// </summary>
    public McpServer? McpServer { get; init; }

    /// <summary>
    /// Optional progress token from the client's request. When set, long-running commands can
    /// emit MCP <c>notifications/progress</c> via <see cref="McpServer"/> to stream updates and
    /// reset client-side inactivity timeouts.
    /// </summary>
    public ProgressToken? ProgressToken { get; set; }

    /// <summary>
    /// Creates a new command context
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency injection</param>
    public CommandContext(IServiceProvider serviceProvider, Activity? activity = default)
    {
        _serviceProvider = serviceProvider;
        Activity = activity;
        Response = new CommandResponse
        {
            Status = HttpStatusCode.OK,
            Message = "Success"
        };
    }

    /// <summary>
    /// Gets a required service from the service provider
    /// </summary>
    /// <typeparam name="T">The type of service to retrieve</typeparam>
    /// <returns>The requested service instance</returns>
    /// <exception cref="InvalidOperationException">Thrown if the service is not registered</exception>
    internal T GetService<T>() where T : class
    {
        return _serviceProvider.GetRequiredService<T>();
    }
}
