// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Search.Models;

public sealed record FieldInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("key")] bool? Key,
    [property: JsonPropertyName("searchable")] bool? Searchable,
    [property: JsonPropertyName("filterable")] bool? Filterable,
    [property: JsonPropertyName("sortable")] bool? Sortable,
    [property: JsonPropertyName("facetable")] bool? Facetable,
    [property: JsonPropertyName("retrievable")] bool? Retrievable);
