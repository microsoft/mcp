// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.DependencyInjection;

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
    /// Whether the connected client advertised support for apps (resource-based pagination).
    /// </summary>
    public bool SupportsApps { get; }

    /// <summary>
    /// Creates a new command context
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency injection</param>
    /// <param name="activity">Optional telemetry activity</param>
    /// <param name="supportsApps">Whether the client supports apps / resource-based pagination</param>
    public CommandContext(IServiceProvider serviceProvider, Activity? activity = default, bool supportsApps = false)
    {
        _serviceProvider = serviceProvider;
        Activity = activity;
        SupportsApps = supportsApps;
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
    public T GetService<T>() where T : class
    {
        return _serviceProvider.GetRequiredService<T>();
    }
}
