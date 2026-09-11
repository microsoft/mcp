// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Adme.Options.Search;

public sealed class SearchOptions
{
    [Option(Description = "One or more OSDU kind selectors 'authority:source:type:version' with optional '*' wildcards, e.g. ['osdu:wks:master-data--Well:1.0.0']. Several kinds are searched as a union. Use the narrowest kind; each '*' widens the scan and its cost.")]
    public required string[] Kind { get; set; }

    [Option(Description = "Optional Lucene filter over indexed fields, e.g. 'data.FieldID:*Volve*'. Omit to match all records of the kind.")]
    public string? Query { get; set; }

    [Option(Description = "Page size (1-1000). Defaults to 10. Pair a large page with returnedFields.", DefaultValue = 10)]
    public int Limit { get; set; } = 10;

    [Option(Description = "Use cursor pagination for a point-in-time snapshot, bulk processing, or more than 10000 results. Defaults to false for real-time query pagination. A supplied cursor also selects cursor pagination.")]
    public bool CursorPaginationMode { get; set; }

    [Option(Description = "Starting offset for query pagination. Cannot be combined with cursor pagination, and offset + limit must be <= 10000.")]
    public int? Offset { get; set; }

    [Option(Description = "Continuation token from a previous cursor response. Supplying it selects cursor pagination; continue until results is empty. Cannot be combined with offset or aggregateBy and expires after about 1 minute.")]
    public string? Cursor { get; set; }

    [Option(Description = "Optional field paths to project, e.g. ['id','kind','data.FacilityName']. Omit to return full records.")]
    public string[]? ReturnedFields { get; set; }

    [Option(Description = "Optional field to aggregate on for distinct values/counts, e.g. 'kind' or 'data.FacilityName.keyword'. Uses query pagination and cannot be combined with cursor.")]
    public string? AggregateBy { get; set; }

    [Option(Description = "Set true to request an exact totalCount. Cursor searches always return an exact total count.")]
    public bool TrackTotalCount { get; set; }

    [Option(Description = "Optional sort criteria. Provide a JSON object with equal-length 'field' and 'order' arrays, e.g. {\"field\":[\"data.Name.keyword\",\"id\"],\"order\":[\"DESC\",\"ASC\"]}.")]
    public string? Sort { get; set; }

    [Option(Description = "Geo-spatial filter (e.g. byBoundingBox, byDistance). Pass a JSON object. A 'field' that is not a geo-point yields 0 results rather than an error.")]
    public string? SpatialFilter { get; set; }

    [Option(Description = "If true, returns only records the user owns. Defaults to false.")]
    public bool? QueryAsOwner { get; set; }

    [Option(Description = "Optional fields to exclude from the payload; may be ignored by ADME, so prefer returnedFields.")]
    public string[]? ExcludedFields { get; set; }

    [Option(Description = "Optional fields to highlight with matched snippets in the response.")]
    public string[]? HighlightedFields { get; set; }

    [Option(Description = "Optional phrase for 'did you mean' spell-check; availability depends on ADME configuration.")]
    public string? SuggestPhrase { get; set; }

    [Option(Description = "The service endpoint, for example 'https://contoso.energy.azure.com'.")]
    public required string Endpoint { get; set; }

    [Option(Description = "The data partition to target, for example 'contoso-dp1'.")]
    public required string DataPartition { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
