// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Storage.Models;

public record FileShareItemInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("isDirectory")] bool IsDirectory,
    [property: JsonPropertyName("size")] long? Size,
    [property: JsonPropertyName("lastModified")] DateTimeOffset? LastModified,
    [property: JsonPropertyName("etag")] string? ETag);
