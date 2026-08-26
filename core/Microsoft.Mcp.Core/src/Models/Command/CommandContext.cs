// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Net;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Microsoft.Mcp.Core.Models.Command;

/// <summary>
/// Provides context for command execution including response management
/// </summary>
public class CommandContext
{
    /// <summary>
    /// The response object that will be returned to the client
    /// </summary>
    public CommandResponse Response { get; }

    /// <summary>
    /// Current telemetry context if there is one available.
    /// </summary>
    private Activity? _activity { get; }

    /// <summary>
    /// The MCP server handling the current tool call. Used by commands that need to send
    /// progress notifications or invoke sampling (deprecated in MCP 2026-07-28).
    /// <para>
    /// Note: in the <c>2026-07-28</c> stateless protocol there is no <c>initialize</c> handshake,
    /// so <see cref="McpServer.ClientInfo"/> will be <see langword="null"/> on every request.
    /// Per-request client identity is instead available via
    /// <c>_meta["io.modelcontextprotocol/clientInfo"]</c> — see
    /// <see cref="Helpers.McpHelper.ClientInfoMetaKey"/>. The <c>McpServer</c>
    /// reference itself is still populated by the tool loaders on every request.
    /// </para>
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
    public CommandContext(Activity? activity = default)
    {
        _activity = activity;
        Response = new CommandResponse
        {
            Status = HttpStatusCode.OK,
            Message = "Success"
        };
    }

    /// <summary>
    /// Adds a telemetry tag to the current activity if one is available. This is a no-op if there is no current activity.
    /// </summary>
    /// <para>
    /// Equivalent to <see cref="Activity.AddTag(string, object?)" />.
    /// </para>
    /// <param name="key">The telemetry tag name.</param>
    /// <param name="value">The telemetry tag value to add.</param>
    /// <returns>The CommandContext</returns>
    public CommandContext AddTelemetryTag(string key, object? value)
    {
        _activity?.AddTag(key, value);
        return this;
    }

    /// <summary>
    /// Set a telemetry tag to the current activity if one is available. This is a no-op if there is no current activity.
    /// </summary>
    /// <para>
    /// Equivalent to <see cref="Activity.SetTag(string, object?)" />.
    /// </para>
    /// <param name="key">The telemetry tag name.</param>
    /// <param name="value">The telemetry tag value to set.</param>
    /// <returns>The CommandContext</returns>
    public CommandContext SetTelemetryTag(string key, object? value)
    {
        _activity?.SetTag(key, value);
        return this;
    }

    internal CommandContext SetTelemetryStatus(ActivityStatusCode status)
    {
        _activity?.SetStatus(status);
        return this;
    }
}
