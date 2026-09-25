// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Compute.Models;

public sealed record GalleryApplicationInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("tags")] IReadOnlyDictionary<string, string>? Tags);
