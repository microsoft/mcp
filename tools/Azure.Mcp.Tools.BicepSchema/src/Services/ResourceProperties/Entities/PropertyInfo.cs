// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text.Json.Serialization;
using Azure.Bicep.Types.Concrete;
using Azure.Mcp.Tools.BicepSchema.Services.Support;

namespace Azure.Mcp.Tools.BicepSchema.Services.ResourceProperties.Entities;

// Information about an object or resource property
public record PropertyInfo
{
    public PropertyInfo(string name, string type, string? description, string? flags, string? modifiers)
    {
        Name = name;
        Type = type;
        Description = description;
        Flags = flags;
        Modifiers = modifiers;
    }

    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("type")]
    public string Type { get; init; }

    private string? _description;
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description
    {
        get => _description.NullIfEmptyOrWhitespace();
        init => _description = value.NullIfEmptyOrWhitespace();
    }

    private string? _flags;
    [JsonPropertyName("flags")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Flags
    {
        get
        {
            Debug.Assert(_flags?.EqualsOrdinalInsensitively(nameof(ObjectTypePropertyFlags.None)) != true);
            return _flags;
        }
        init
        {
            _flags = value?.EqualsOrdinalInsensitively(nameof(ObjectTypePropertyFlags.None)) == true
                ? null
                : value.NullIfEmptyOrWhitespace();
        }
    }

    private string? _modifiers;
    [JsonPropertyName("modifiers")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Modifiers
    {
        get => _modifiers.NullIfEmptyOrWhitespace();
        init => _modifiers = value.NullIfEmptyOrWhitespace();
    }
}
