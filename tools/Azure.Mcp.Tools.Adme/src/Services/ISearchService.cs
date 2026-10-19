// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Adme.Models;
using Azure.Mcp.Tools.Adme.Models.Search;

namespace Azure.Mcp.Tools.Adme.Services;

public interface ISearchService
{
    Task<AdmeResponse<SearchQueryResponse>> QueryAsync(
        string endpoint,
        string dataPartition,
        SearchQueryRequest request,
        string? tenant,
        string? authAppId = null,
        CancellationToken cancellationToken = default);

    Task<AdmeResponse<SearchCursorResponse>> QueryWithCursorAsync(
        string endpoint,
        string dataPartition,
        SearchCursorRequest request,
        bool searchAfter,
        string? tenant,
        string? authAppId = null,
        CancellationToken cancellationToken = default);
}
