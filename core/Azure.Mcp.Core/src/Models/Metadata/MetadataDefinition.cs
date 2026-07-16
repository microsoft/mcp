// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Core.Models.Metadata;

public sealed class MetadataDefinition
{
    /// <summary>
    /// Gets or sets the boolean value of the metadata property.
    /// </summary>
    [JsonPropertyName("value")]
    public bool Value { get; init; }

    /// <summary>
    /// Gets or sets the description of what the metadata means.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;
}
