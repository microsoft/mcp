// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Services.Pagination;

/// <summary>
/// Result returned by a <see cref="IPaginationProviderAdapter{TItem}"/> when fetching a page.
/// </summary>
/// <typeparam name="TItem">The type of items in the page.</typeparam>
/// <param name="Items">The items returned for this page.</param>
/// <param name="NativeState">
/// Provider-native continuation state for the next page, or null if no more pages exist.
/// Opaque to everything outside the owning provider adapter.
/// </param>
public sealed record PageResult<TItem>(List<TItem> Items, string? NativeState);
