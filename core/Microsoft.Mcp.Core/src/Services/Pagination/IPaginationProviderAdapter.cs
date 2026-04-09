// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Services.Pagination;

/// <summary>
/// Contract for provider-specific pagination handlers.
/// Each adapter knows how to start and resume pagination using the native
/// continuation mechanism of its provider (e.g., ARM skipToken, Cosmos continuation token).
/// </summary>
/// <typeparam name="TItem">The type of items returned per page.</typeparam>
public interface IPaginationProviderAdapter<TItem>
{
    /// <summary>
    /// The provider identifier (e.g., "kql", "arm", "cosmos").
    /// Must match the <see cref="CursorRecord.Provider"/> value stored during save.
    /// </summary>
    string Provider { get; }

    /// <summary>
    /// Fetches the first page of results for an initial request.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="PageResult{TItem}"/> with the first page of items and native state for the next page
    /// (null if all results fit in a single page).
    /// </returns>
    Task<PageResult<TItem>> FetchFirstPageAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumes pagination using provider-native state from a stored cursor.
    /// </summary>
    /// <param name="nativeState">
    /// The opaque provider-native continuation state previously returned
    /// from <see cref="FetchFirstPageAsync"/> or a prior <see cref="FetchNextPageAsync"/> call.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="PageResult{TItem}"/> with the next page of items and native state for the following page
    /// (null if no more pages exist).
    /// </returns>
    Task<PageResult<TItem>> FetchNextPageAsync(string nativeState, CancellationToken cancellationToken = default);
}
