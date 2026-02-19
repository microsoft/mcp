// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Configuration;

/// <summary>
/// Configuration settings for the MCP server.
/// </summary>
public class McpServerConfiguration
{
    /// <summary>
    /// The default prefix for the MCP server commands and help menus.
    /// </summary>
    public required string RootCommandGroupName { get; set; }

    /// <summary>
    /// The name of the MCP server. (i.e. Azure.Mcp.Server, Fabric.Mcp.Server, etc.)
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The display name of the MCP server.
    /// </summary>
    public required string DisplayName { get; set; }

    /// <summary>
    /// The version of the MCP server.
    /// </summary>
    public required string Version { get; set; }

    /// <summary>
    /// Indicates whether telemetry is enabled for the MCP server.  By default, it is set to true.
    /// </summary>
    public bool IsTelemetryEnabled { get; set; } = true;
}
