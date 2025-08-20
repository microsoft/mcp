// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text.Json.Serialization;
using Azure.Bicep.Types.Concrete;
using Azure.Mcp.Tools.BicepSchema.Services.Support;

namespace Azure.Mcp.Tools.BicepSchema.Services.ResourceProperties.Entities;

public class ResourceTypeEntity : ComplexType
{
    [JsonPropertyName("bodyType")]
    public required ComplexType BodyType { get; init; }

    [JsonPropertyName("writableScopes")]
    public string WritableScopes { get; init; } = "Unknown";

    [JsonPropertyName("readableScopes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ReadableScopes { get; init; }

    public override bool Equals(object? obj) =>
        obj is ResourceTypeEntity other &&
        Name == other.Name &&
        BodyType.Equals(other.BodyType) &&
        WritableScopes == other.WritableScopes &&
        ReadableScopes == other.ReadableScopes;
    public override int GetHashCode() =>
        HashCode.Combine(Name, BodyType, WritableScopes, ReadableScopes);
}
