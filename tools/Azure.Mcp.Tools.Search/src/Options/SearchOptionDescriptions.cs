// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Search.Options;

internal static class SearchOptionDescriptions
{
    internal const string Service = "The name of the Azure AI Search service (e.g., my-search-service).";
    internal const string Index = "The name of the search index within the Azure AI Search service.";
    internal const string KnowledgeBase = "The name of the knowledge base within the Azure AI Search service.";
    internal const string QueryType = "The query syntax to use when searching the index. 'simple' uses the simple query syntax, "
        + "'full' (default) uses the full Lucene query syntax which supports field-scoped queries, fuzzy search and regular expressions, "
        + "and 'semantic' applies semantic ranking, which requires the index to have a semantic configuration.";
}
