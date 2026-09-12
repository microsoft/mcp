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
    Title = "Search ADME/OSDU Records",
    Description = """
        Search ADME/OSDU records across one or more exact or wildcard kinds using indexed criteria.
        Returns full records, selected fields, or aggregate counts.

        Optional parameters: Lucene query, returned-field projection, spatial filter (geographic, bounding box),
        sort (ascending, descending), aggregation, owner-scoped query, and highlighted fields.

        Pagination modes: offset and cursor (point-in-time snapshot, bulk processing, more than 10000 results,
        search_after).
        """,
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    OperationPlane = ToolOperationPlane.Data,
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
            options.SearchAfter,
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
        || options.CursorPaginationMode
        || options.SearchAfter;
}
