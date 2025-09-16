// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Core.Models.Elicitation;

/// <summary>
/// Represents a JSON schema for elicitation requests.
/// </summary>
public sealed class ElicitationSchemaRoot
{
    /// <summary>
    /// Gets or sets the type of the schema object.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "object";

    /// <summary>
    /// Gets or sets the properties of the schema.
    /// </summary>
    [JsonPropertyName("properties")]
    public required Dictionary<string, ElicitationSchemaProperty> Properties { get; set; }

    /// <summary>
    /// Gets or sets the list of required property names.
    /// </summary>
    [JsonPropertyName("required")]
    public string[]? Required { get; set; }
}

/// <summary>
/// Represents a property within an elicitation schema.
/// </summary>
public sealed class ElicitationSchemaProperty
{
    /// <summary>
    /// Gets or sets the type of the property.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "string";

    /// <summary>
    /// Gets or sets the display title for the property.
    /// </summary>
    [JsonPropertyName("title")]
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the description of what the property represents.
    /// </summary>
    [JsonPropertyName("description")]
    public required string Description { get; set; }

    /// <summary>
    /// Gets or sets the format hint for the property (e.g., "password").
    /// </summary>
    [JsonPropertyName("format")]
    public string? Format { get; set; }
}
