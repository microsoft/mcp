// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Services.Caching.Pagination;

namespace Azure.Mcp.Core.Services.Pagination;

/// <summary>
/// Pagination provider adapter for Azure SDK <see cref="AsyncPageable{T}"/> sources.
/// Uses the SDK continuation token as the native continuation state.
/// </summary>
/// <typeparam name="TSource">The Azure SDK item type returned by the pageable.</typeparam>
/// <typeparam name="TResult">The domain model type exposed to callers.</typeparam>
public sealed class AsyncPageablePaginationAdapter<TSource, TResult>(
    Func<string?, int?, Task<AsyncPageablePaginationAdapter<TSource, TResult>.PageableResult>> pageableFactory,
    Func<TSource, TResult> converter,
    int pageSize = 10) : IPaginationProviderAdapter<TResult>
    where TSource : notnull
{
    public const string ProviderName = "asyncpageable";

    public string Provider => ProviderName;

    public async Task<PageResult<TResult>> FetchFirstPageAsync(CancellationToken cancellationToken = default)
    {
        return await FetchPageAsync(null, cancellationToken);
    }

    public async Task<PageResult<TResult>> FetchNextPageAsync(string nativeState, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(nativeState);
        return await FetchPageAsync(nativeState, cancellationToken);
    }

    private async Task<PageResult<TResult>> FetchPageAsync(string? continuationToken, CancellationToken cancellationToken)
    {
        var result = await pageableFactory(continuationToken, pageSize);
        var items = new List<TResult>();
        string? nextContinuationToken = null;

        await foreach (var page in result.Pageable.AsPages(continuationToken, pageSize).WithCancellation(cancellationToken))
        {
            foreach (var item in page.Values)
            {
                items.Add(converter(item));
            }

            nextContinuationToken = page.ContinuationToken;
            break; // Only fetch one page
        }

        return new PageResult<TResult>(items, nextContinuationToken);
    }

    /// <summary>
    /// Wraps the <see cref="AsyncPageable{T}"/> returned by a service call.
    /// </summary>
    /// <param name="Pageable">The async pageable to iterate over.</param>
    public sealed record PageableResult(AsyncPageable<TSource> Pageable);
}
