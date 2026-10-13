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
/// <param name="activity">The invocation's telemetry activity, or <see langword="null"/> when unavailable.</param>
public class CommandContext(Activity? activity = default)
{
    /// <summary>
    /// The response object that will be returned to the client
    /// </summary>
    public CommandResponse Response { get; } = new()
    {
        Status = HttpStatusCode.OK,
        Message = "Success"
    };

    /// <summary>
    /// Current telemetry context if there is one available.
    /// </summary>
    public Activity? Activity { get; } = activity;

    /// <summary>
    /// Gets the executing command's original registered setup-area name.
    /// </summary>
    /// <value>
    /// The original tool namespace, such as <c>storage</c>, or <see langword="null"/> when
    /// the loader cannot resolve a registered namespace.
    /// </value>
    /// <remarks>
    /// Initialized by the local tool loader from the selected command's original registration.
    /// This identity is independent of single-tool proxy names and synthetic consolidated groups,
    /// and may differ from an endpoint service type such as <c>arm</c>. Do not populate it from
    /// request arguments or telemetry tags. An unresolved namespace does not disable endpoint
    /// validation and cannot match a namespace-scoped bypass.
    /// </remarks>
    public string? ToolNamespaceName { get; init; }

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

}
