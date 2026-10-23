// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

public class CatalogSearchOptions
{
    [Option(Description = "The text query for the search. Supports searching across display name, description and workspace name of the catalog entry. Omit when using continuation-token.")]
    public string? Search { get; set; }

    [Option(Description = "The filter for the search. Supports filtering by type of entries. Supported operators: eq (Equals), ne (Not Equals), or (Logical OR), () (Parentheses for grouping). Example: \"Type eq 'Report' or Type eq 'Lakehouse'\". Supported item types include Report, Lakehouse, Notebook, Warehouse, SemanticModel, KQLDatabase, and DataPipeline; for the full list see the Catalog Search API reference at https://learn.microsoft.com/rest/api/fabric/core/catalog/search. Omit when using continuation-token.")]
    public string? Filter { get; set; }

    [Option(Description = "The page size that needs to be returned. Must be between 1 and 1000.")]
    public int? PageSize { get; set; }

    [Option(Description = "The unchanged token returned by the previous page. Carries the original search, filter, and page size; may be sent alone and must not accompany search or filter.")]
    public string? ContinuationToken { get; set; }
}
