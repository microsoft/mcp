// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

// Lightweight projection of ContainerProperties with commonly useful metadata.
// Keep property names stable; only add new nullable properties to extend.
public sealed record ContainerInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("lastModified")] DateTimeOffset LastModified,
    [property: JsonPropertyName("eTag")] string? ETag,
    [property: JsonPropertyName("metadata")] IDictionary<string, string> Metadata,
    [property: JsonPropertyName("leaseStatus")] string? LeaseStatus,
    [property: JsonPropertyName("leaseState")] string? LeaseState,
    [property: JsonPropertyName("leaseDuration")] string? LeaseDuration,
    [property: JsonPropertyName("publicAccess")] string? PublicAccess,
    [property: JsonPropertyName("hasImmutabilityPolicy")] bool? HasImmutabilityPolicy,
    [property: JsonPropertyName("hasLegalHold")] bool? HasLegalHold,
    [property: JsonPropertyName("deletedOn")] DateTimeOffset? DeletedOn,
    [property: JsonPropertyName("remainingRetentionDays")] int? RemainingRetentionDays,
    [property: JsonPropertyName("hasImmutableStorageWithVersioning")] bool HasImmutableStorageWithVersioning);
