// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.BicepSchema.Services.ResourceProperties.Helpers;

namespace Azure.Mcp.Tools.BicepSchema.Services.ResourceProperties.Entities;

public record ResourcePropertyEntity
{
    public ResourcePropertyEntity(Guid resourceTypeId, string fullyQualifiedName, string description, string type, string flags, string valueConstraints, string? additionalPropertiesType)
    {
        ResourceTypeId = resourceTypeId;
        FullyQualifiedName = fullyQualifiedName;
        Type = type;
        Description = description;
        Flags = flags;
        ValueConstraints = valueConstraints;
        AdditionalPropertiesType = additionalPropertiesType;
    }

    [JsonPropertyName("id")]
    public Guid Id
    {
        get => GuidHelper.GenerateDeterministicGuid(ResourceTypeId.ToString().ToLower(), FullyQualifiedName.ToLower());
    }

    [JsonPropertyName("resourceTypeId")]
    public Guid ResourceTypeId { get; init; }

    [JsonPropertyName("fullyQualifiedName")]
    public string FullyQualifiedName { get; init; }

    [JsonPropertyName("type")]
    public string Type { get; init; }

    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; init; }

    [JsonPropertyName("flags")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Flags { get; init; }

    [JsonPropertyName("valueConstraints")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ValueConstraints { get; init; }

    [JsonPropertyName("additionalPropertiesType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AdditionalPropertiesType { get; init; }
}
