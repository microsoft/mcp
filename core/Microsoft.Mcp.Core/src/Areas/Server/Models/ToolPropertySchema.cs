// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Mcp.Core.Areas.Server.Models;

/// <summary>
/// Represents a property definition within a tool's input schema.
/// </summary>
public sealed class ToolPropertySchema
{
    /// <summary>
    /// The JSON type of the property (e.g., "string", "integer", "boolean", "array", "object").
    /// The type mapping is handled by <see cref="Commands.TypeToJsonTypeMapper"/>.
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    /// <summary>
    /// A description of what this property represents.
    /// </summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; init; }

    /// <summary>
    /// The type of the items in the array.
    /// The type mapping is handled by <see cref="Commands.TypeToJsonTypeMapper"/>.
    /// </summary>
    [JsonPropertyName("items")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ToolPropertySchema? Items { get; init; }

    /// <summary>
    /// The allowed values for this property when it represents an enumeration.
    /// Used to provide models with valid value guidance for constrained options.
    /// </summary>
    [JsonPropertyName("enum")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Enum { get; init; }
}
