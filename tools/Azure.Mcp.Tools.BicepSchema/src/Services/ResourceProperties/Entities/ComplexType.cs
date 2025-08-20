// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.BicepSchema.Services.ResourceProperties.Entities;

[JsonDerivedType(typeof(DiscriminatedObjectTypeEntity), typeDiscriminator: "DiscriminatedObject")]
[JsonDerivedType(typeof(ObjectTypeEntity), typeDiscriminator: "Object")]
[JsonDerivedType(typeof(ResourceFunctionTypeEntity), typeDiscriminator: "ResourceInstanceFunction")]
[JsonDerivedType(typeof(ResourceTypeEntity), typeDiscriminator: "Resource")]
public abstract class ComplexType
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }
}
