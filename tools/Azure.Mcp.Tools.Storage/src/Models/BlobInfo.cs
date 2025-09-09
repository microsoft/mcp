// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

// Lightweight projection of BlobProperties with commonly useful metadata.
// Keep property names stable; only add new nullable properties to extend.
public sealed record BlobInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("lastModified")] DateTimeOffset? LastModified,
    [property: JsonPropertyName("eTag")] string? ETag,
    [property: JsonPropertyName("contentLength")] long? ContentLength,
    [property: JsonPropertyName("contentType")] string ContentType,
    [property: JsonPropertyName("contentHash")] byte[]? ContentHash,
    [property: JsonPropertyName("blobType")] string? BlobType,
    [property: JsonPropertyName("metadata")] IDictionary<string, string> Metadata,
    [property: JsonPropertyName("leaseStatus")] string? LeaseStatus,
    [property: JsonPropertyName("leaseState")] string? LeaseState,
    [property: JsonPropertyName("leaseDuration")] string? LeaseDuration,
    [property: JsonPropertyName("copyStatus")] string? CopyStatus,
    [property: JsonPropertyName("copySource")] Uri? CopySource,
    [property: JsonPropertyName("copyCompletedOn")] DateTimeOffset? CopyCompletedOn,
    [property: JsonPropertyName("accessTier")] string? AccessTier,
    [property: JsonPropertyName("accessTierChangedOn")] DateTimeOffset? AccessTierChangedOn,
    [property: JsonPropertyName("hasLegalHold")] bool? HasLegalHold,
    [property: JsonPropertyName("createdOn")] DateTimeOffset? CreatedOn,
    [property: JsonPropertyName("archiveStatus")] string? ArchiveStatus,
    [property: JsonPropertyName("versionId")] string? VersionId);
