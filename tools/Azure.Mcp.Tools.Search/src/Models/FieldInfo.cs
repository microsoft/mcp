// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Search.Models;

public sealed record FieldInfo(
    string Name,
    string Type,
    bool? Key,
    bool? Searchable,
    bool? Filterable,
    bool? Sortable,
    bool? Facetable,
    bool? Retrievable);
