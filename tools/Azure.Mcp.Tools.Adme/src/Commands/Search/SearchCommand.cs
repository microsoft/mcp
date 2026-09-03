// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Adme.Models.Search;
using Azure.Mcp.Tools.Adme.Options.Search;
using Azure.Mcp.Tools.Adme.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Adme.Commands.Search;

[CommandMetadata(
    Id = "7490b3ca-7c75-4f03-bf37-bceb50821d91",
    Name = "search",
    Title = "Search ADME Records",
    Description = """
        Search ADME records when record ID/kind is unknown by indexed fields. 
        Use Lucene filters, wildcard or multiple kinds, projection, aggregation and exact counts,
        sorting, geographic/bounding-box filters, query as owner, highlighting, or stable cursor pagination for bulk
        retrieval or more than 10000 results. Continue snapshots with a cursor.

        KIND requires one or more 'authority:source:type:version' selectors and supports '*' in any segment.
        Use the narrowest kind because wildcards increase scan cost. Use schema commands to discover kinds
        and field names if unsure.

        QUERY is an optional Lucene filter over indexed fields. Payload fields use 'data.'; add '.keyword'
        for exact, case-sensitive, or null matches. Quote full record IDs; leading wildcards are unsupported.
        No matches return an empty result.

        CURSORPAGINATIONMODE starts a snapshot for bulk or more than 10000 results; false uses real-time
        query pagination. Pass cursor for fetching subsequent pages.
        A CURSOR also selects cursor mode. Cursor mode rejects OFFSET and AGGREGATEBY; query mode requires
        OFFSET plus LIMIT to be at most 10000.

        LIMIT is 1-1000 and defaults to 10. RETURNEDFIELDS projects paths. AGGREGATEBY groups one field into
        up to 1000 buckets. SORT requires equal-length field and order arrays. SPATIALFILTER requires a
        geo-point field and supported shape. EXCLUDEDFIELDS and SUGGESTPHRASE are accepted but ineffective.
        """,
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false)]
public sealed class SearchCommand(ISearchService searchService)
    : AuthenticatedCommand<SearchOptions, SearchResponse>
{
    private readonly ISearchService _searchService = searchService;

    public override void ValidateOptions(SearchOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        AdmeServiceValidator.ValidateTarget(options.Endpoint, options.DataPartition, validationResult);
        AdmeServiceValidator.ValidateSearch(
            options.Kind, options.Limit, options.Sort, options.SpatialFilter, validationResult, options.Offset);

        if (UsesCursorPagination(options)
            && (options.Offset is not null || !string.IsNullOrWhiteSpace(options.AggregateBy)))
        {
            validationResult.Errors.Add(
                "Cursor pagination cannot be combined with --offset or --aggregate-by.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, SearchOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = UsesCursorPagination(options)
                ? await QueryWithCursorAsync(options, cancellationToken)
                : await QueryAsync(options, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, AdmeJsonContext.Default.SearchResponse);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }

    private async Task<SearchResponse> QueryAsync(SearchOptions options, CancellationToken cancellationToken)
    {
        var response = await _searchService.QueryAsync(
            options.Endpoint,
            options.DataPartition,
            new SearchQueryRequest
            {
                Kind = options.Kind,
                Query = AdmeServiceHelper.Normalize(options.Query),
                SuggestPhrase = AdmeServiceHelper.Normalize(options.SuggestPhrase),
                Limit = options.Limit,
                Offset = options.Offset,
                ReturnedFields = AdmeServiceHelper.Normalize(options.ReturnedFields),
                AggregateBy = AdmeServiceHelper.Normalize(options.AggregateBy),
                TrackTotalCount = options.TrackTotalCount ? true : null,
                Sort = AdmeServiceHelper.ParseSort(options.Sort),
                SpatialFilter = AdmeServiceHelper.ParseJsonObject(options.SpatialFilter),
                QueryAsOwner = options.QueryAsOwner,
                ExcludedFields = AdmeServiceHelper.Normalize(options.ExcludedFields),
                HighlightedFields = AdmeServiceHelper.Normalize(options.HighlightedFields),
            },
            options.Tenant,
            cancellationToken);

        return new SearchResponse
        {
            Results = response.Results,
            TotalCount = response.TotalCount,
            Aggregations = response.Aggregations,
            PhraseSuggestions = response.PhraseSuggestions,
        };
    }

    private async Task<SearchResponse> QueryWithCursorAsync(
        SearchOptions options,
        CancellationToken cancellationToken)
    {
        var response = await _searchService.QueryWithCursorAsync(
            options.Endpoint,
            options.DataPartition,
            new SearchCursorRequest
            {
                Kind = options.Kind,
                Query = AdmeServiceHelper.Normalize(options.Query),
                SuggestPhrase = AdmeServiceHelper.Normalize(options.SuggestPhrase),
                Limit = options.Limit,
                ReturnedFields = AdmeServiceHelper.Normalize(options.ReturnedFields),
                Cursor = AdmeServiceHelper.Normalize(options.Cursor),
                Sort = AdmeServiceHelper.ParseSort(options.Sort),
                SpatialFilter = AdmeServiceHelper.ParseJsonObject(options.SpatialFilter),
                QueryAsOwner = options.QueryAsOwner,
                ExcludedFields = AdmeServiceHelper.Normalize(options.ExcludedFields),
                HighlightedFields = AdmeServiceHelper.Normalize(options.HighlightedFields),
            },
            options.Tenant,
            cancellationToken);

        return new SearchResponse
        {
            Results = response.Results,
            TotalCount = response.TotalCount,
            Cursor = response.Cursor,
        };
    }

    private static bool UsesCursorPagination(SearchOptions options) =>
        !string.IsNullOrWhiteSpace(options.Cursor)
        || options.CursorPaginationMode;
}
