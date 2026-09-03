// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Adme.Models.Search;

namespace Azure.Mcp.Tools.Adme.Services;

public interface ISearchService
{
    Task<SearchQueryResponse> QueryAsync(
        string endpoint,
        string dataPartition,
        SearchQueryRequest request,
        string? tenant,
        CancellationToken cancellationToken);

    Task<SearchCursorResponse> QueryWithCursorAsync(
        string endpoint,
        string dataPartition,
        SearchCursorRequest request,
        string? tenant,
        CancellationToken cancellationToken);
}