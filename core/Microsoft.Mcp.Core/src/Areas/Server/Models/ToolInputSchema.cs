// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Core.Areas.Server.Models;

/// <summary>
/// Represents the JSON schema for a tool's input parameters.
/// </summary>
public sealed class ToolInputSchema
{
    /// <summary>
    /// The type of the schema object (always "object" for tool schemas).
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; init; } = "object";

    /// <summary>
    /// The properties defined for this schema.
    /// </summary>
    [JsonPropertyName("properties")]
    public Dictionary<string, ToolPropertySchema> Properties { get; init; } = new();

    /// <summary>
    /// The list of required property names.
    /// </summary>
    [JsonPropertyName("required")]
    public string[]? Required { get; set; } = [];
}
