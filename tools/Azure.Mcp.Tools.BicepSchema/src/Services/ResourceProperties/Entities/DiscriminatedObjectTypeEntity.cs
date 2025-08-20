// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.BicepSchema.Services.ResourceProperties.Entities;

public class DiscriminatedObjectTypeEntity : ComplexType
{
    [JsonPropertyName("baseProperties")]
    public List<PropertyInfo> BaseProperties { get; init; } = [];


    [JsonPropertyName("elements")]
    public List<ComplexType> Elements { get; init; } = [];

    [JsonPropertyName("discriminator")]
    public required string Discriminator { get; init; }

    public override bool Equals(object? obj) =>
        obj is DiscriminatedObjectTypeEntity other &&
        Name == other.Name &&
        BaseProperties.SequenceEqual(other.BaseProperties) &&
        Elements.SequenceEqual(other.Elements);

    public override int GetHashCode() =>
        HashCode.Combine(
            Name,
            BaseProperties.Aggregate(0, (hash, prop) => HashCode.Combine(hash, prop)),
            Elements.Aggregate(0, (hash, elem) => HashCode.Combine(hash, elem))
        );
}
