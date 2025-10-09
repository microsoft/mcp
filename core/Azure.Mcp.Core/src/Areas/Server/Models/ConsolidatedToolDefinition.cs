// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands;

namespace Azure.Mcp.Core.Areas.Server.Models;

/// <summary>
/// Represents a composite tool definition that groups multiple related Azure operations together.
/// Used to create consolidated tools from the azure_mcp_consolidated_tools JSON configuration.
/// </summary>
public sealed class ConsolidatedToolDefinition
{
    /// <summary>
    /// Gets or sets the name of the composite tool.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the composite tool's capabilities and purpose.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tool metadata containing capability information.
    /// </summary>
    [JsonPropertyName("toolMetadata")]
    public ToolMetadata? ToolMetadata { get; set; }
}