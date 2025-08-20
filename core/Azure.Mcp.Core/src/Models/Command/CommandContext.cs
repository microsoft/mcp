// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Core.Models.Command;

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
    /// Creates a new command context
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency injection</param>
    public CommandContext(IServiceProvider serviceProvider, Activity? activity = default)
    {
        _serviceProvider = serviceProvider;
        Activity = activity;
        Response = new CommandResponse
        {
            Status = 200,
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
