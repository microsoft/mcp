// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using ModelContextProtocol.Protocol;

namespace Azure.Mcp.Core.Services.Telemetry;

public interface ITelemetryService : IDisposable
{
    /// <summary>
    /// Creates and starts a new telemetry activity.
    /// </summary>
    /// <param name="activityName">Name of the activity.</param>
    /// <returns>An Activity object or null if there are no active listeners or telemetry is disabled.</returns>
    /// <exception cref="InvalidOperationException">If the service is not in an operational state or <see cref="InitializeAsync"/> was not invoked.</exception>
    Activity? StartActivity(string activityName);

    /// <summary>
    /// Creates and starts a new telemetry activity.
    /// </summary>
    /// <param name="activityName">Name of the activity.</param>
    /// <param name="clientInfo">MCP client information to add to the activity.</param>
    /// <returns>An Activity object or null if there are no active listeners or telemetry is disabled.</returns>
    /// <exception cref="InvalidOperationException">If the service is not in an operational state or <see cref="InitializeAsync"/> was not invoked.</exception>
    Activity? StartActivity(string activityName, Implementation? clientInfo);

    /// <summary>
    /// Performs any initialization operations before telemetry service is ready.
    /// </summary>
    /// <returns>A task that completes when initialization is complete.</returns>
    Task InitializeAsync();
}
