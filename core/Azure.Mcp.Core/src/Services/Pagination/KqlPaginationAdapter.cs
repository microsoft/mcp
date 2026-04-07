// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Microsoft.Mcp.Core.Services.Caching.Pagination;

namespace Azure.Mcp.Core.Services.Pagination;

/// <summary>
/// Pagination provider adapter for Azure Resource Graph (KQL) queries.
/// Uses the Resource Graph <c>skipToken</c> as the native continuation state.
/// </summary>
/// <typeparam name="T">The type of items returned per page.</typeparam>
public sealed class KqlPaginationAdapter<T>(
    Func<string?, Task<PagedResourceQueryResults<T>>> queryExecutor) : IPaginationProviderAdapter<T>
{
    public const string ProviderName = "kql";

    public string Provider => ProviderName;

    public async Task<PageResult<T>> FetchFirstPageAsync(CancellationToken cancellationToken = default)
    {
        var result = await queryExecutor(null);
        return new PageResult<T>(result.Results, result.SkipToken);
    }

    public async Task<PageResult<T>> FetchNextPageAsync(string nativeState, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(nativeState);

        var result = await queryExecutor(nativeState);
        return new PageResult<T>(result.Results, result.SkipToken);
    }
}
