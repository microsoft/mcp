// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Commands;

namespace Microsoft.Mcp.Core.Areas.Server.Commands.Discovery;

/// <summary>
/// Represents metadata for an MCP server, including identification and descriptive information.
/// </summary>
/// <param name="id">The unique identifier for the server.</param>
/// <param name="name">The display name of the server.</param>
/// <param name="description">A description of the server's purpose or capabilities.</param>
public sealed class McpServerMetadata(string id = "", string name = "", string description = "")
{
    /// <summary>
    /// Gets or sets the unique identifier for the server.
    /// </summary>
    public string Id { get; set; } = id;

    /// <summary>
    /// Gets or sets the display name of the server.
    /// </summary>
    public string Name { get; set; } = name;

    /// <summary>
    /// Gets or sets the user-friendly title of the server for display purposes.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets a description of the server's purpose or capabilities.
    /// </summary>
    public string Description { get; set; } = description;

    /// <summary>
    /// Gets or sets the tool metadata for this server, containing tool-specific information.
    /// </summary>
    public ToolMetadata? ToolMetadata { get; set; }
}
