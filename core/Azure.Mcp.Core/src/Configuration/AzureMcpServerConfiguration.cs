// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel.DataAnnotations;

namespace Azure.Mcp.Core.Configuration;

/// <summary>
/// Configuration settings for the MCP server.
/// </summary>
public class AzureMcpServerConfiguration
{
    /// <summary>
    /// The default prefix for the MCP server commands and help menus.
    /// </summary>
    [Required]
    public required string Prefix { get; set; }

    /// <summary>
    /// The name of the MCP server. (i.e. Azure.Mcp.Server)
    /// </summary>
    [Required]
    public required string Name { get; set; }

    /// <summary>
    /// The display name of the MCP server.
    /// </summary>
    [Required]
    public required string DisplayName { get; set; }

    /// <summary>
    /// The version of the MCP server.
    /// </summary>
    [Required]
    public required string Version { get; set; }

    /// <summary>
    /// Indicates whether telemetry is enabled.
    /// </summary>
    [Required]
    public bool IsTelemetryEnabled { get; set; } = true;

    /// <summary>
    /// The application insights connection string to use if <see cref="IsTelemetryEnabled"/> is true.
    /// </summary>
    public string? ApplicationInsightsConnectionString { get; set; }
}
