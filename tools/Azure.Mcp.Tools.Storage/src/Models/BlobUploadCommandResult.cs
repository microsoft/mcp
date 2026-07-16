// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Storage.Models;

public sealed record BlobUploadCommandResult(
    [property: JsonPropertyName("blob")] string Blob,
    [property: JsonPropertyName("container")] string Container,
    [property: JsonPropertyName("uploadedFile")] string UploadedFile,
    [property: JsonPropertyName("lastModified")] DateTimeOffset LastModified,
    [property: JsonPropertyName("eTag")] string ETag,
    [property: JsonPropertyName("md5Hash")] string? MD5Hash
);
