// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Adme.Models.Search;
using Microsoft.Mcp.Core.Services.Azure.Authentication;

namespace Azure.Mcp.Tools.Adme.Services;

public sealed class SearchService(
    IAzureTokenCredentialProvider credentialProvider,
    IHttpClientFactory httpClientFactory) : ISearchService
{
    private const string BasePath = "/api/search/v2";

    private readonly IAzureTokenCredentialProvider _credentialProvider = credentialProvider;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public Task<SearchQueryResponse> QueryAsync(
        string endpoint,
        string dataPartition,
        SearchQueryRequest request,
        string? tenant,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateKinds(request.Kind);
        return AdmeServiceHelper.PostAsync(
            _credentialProvider, _httpClientFactory, endpoint, dataPartition, tenant,
            $"{BasePath}/query", request, AdmeJsonContext.Default.SearchQueryRequest,
            AdmeJsonContext.Default.SearchQueryResponse, extraHeaders: null, cancellationToken);
    }

    public Task<SearchCursorResponse> QueryWithCursorAsync(
        string endpoint,
        string dataPartition,
        SearchCursorRequest request,
        string? tenant,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateKinds(request.Kind);
        return AdmeServiceHelper.PostAsync(
            _credentialProvider, _httpClientFactory, endpoint, dataPartition, tenant,
            $"{BasePath}/query_with_cursor", request, AdmeJsonContext.Default.SearchCursorRequest,
            AdmeJsonContext.Default.SearchCursorResponse, extraHeaders: null, cancellationToken,
            disableRetries: true);
    }

    private static void ValidateKinds(IReadOnlyList<string> kinds)
    {
        ArgumentNullException.ThrowIfNull(kinds);
        if (kinds.Count == 0 || kinds.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("At least one non-empty kind is required.", nameof(kinds));
        }
    }
}