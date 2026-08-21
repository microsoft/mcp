// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.IoTHub.Models;

/// <summary>
/// A structured, typed request that the IoT Hub query compiler turns into a syntactically valid query string.
/// </summary>
public class QueryCompileRequest
{
    /// <summary>The ordered set of predicates that are combined into the query's <c>WHERE</c> clause.</summary>
    [JsonPropertyName("filters")]
    public List<QueryPredicate> Filters { get; set; } = [];

    /// <summary>The source collection to query. Defaults to <c>devices</c>.</summary>
    [JsonPropertyName("from")]
    public string From { get; set; } = "devices";

    /// <summary>An optional page-size hint returned as <c>maxCount</c> for <c>query run</c>.</summary>
    [JsonPropertyName("top")]
    public int? Top { get; set; }

    /// <summary>The logical operator (<c>AND</c> or <c>OR</c>) used to join predicates. Defaults to <c>AND</c>.</summary>
    [JsonPropertyName("logicalOperator")]
    public string LogicalOperator { get; set; } = "AND";

    /// <summary>Optional discovered field paths used to validate predicates before compiling a query.</summary>
    [JsonPropertyName("discoveredFields")]
    public QueryDiscoveredFields? DiscoveredFields { get; set; }
}
