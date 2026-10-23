// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Search.Options.Index;

/// <summary>
/// The query syntax used when querying an Azure AI Search index.
/// </summary>
public enum IndexQueryType
{
    /// <summary>
    /// Simple query syntax, which supports symbols such as +, * and "".
    /// </summary>
    Simple,

    /// <summary>
    /// Full Lucene query syntax, which supports field-scoped queries, fuzzy search, regular expressions and more.
    /// </summary>
    Full,

    /// <summary>
    /// Semantic ranking, which requires the index to have a semantic configuration.
    /// </summary>
    Semantic
}
