// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using ModelContextProtocol.Protocol;

namespace Microsoft.Mcp.Core.Services.Telemetry;

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
    /// <param name="clientInfo">The MCP client information to add to the activity. Under the
    /// 2025-11-25 protocol this is populated from the <c>initialize</c> handshake. Under the
    /// 2026-07-28 stateless protocol it is null; per-request client info is read from
    /// <paramref name="requestParams"/> instead.</param>
    /// <param name="requestParams">The request parameters for the MCP call. Starting in the
    /// 2026-07-28 spec, client name/version are carried in
    /// <c>_meta["io.modelcontextprotocol/clientInfo"]</c> on every request.</param>
    /// <returns>An Activity object or null if there are no active listeners or telemetry is disabled.</returns>
    /// <exception cref="InvalidOperationException">If the service is not in an operational state or <see cref="InitializeAsync"/> was not invoked.</exception>
    Activity? StartActivity(string activityName, Implementation? clientInfo, RequestParams? requestParams);

    /// <summary>
    /// Performs any initialization operations before telemetry service is ready.
    /// </summary>
    /// <returns>A task that completes when initialization is complete.</returns>
    Task InitializeAsync();
}
