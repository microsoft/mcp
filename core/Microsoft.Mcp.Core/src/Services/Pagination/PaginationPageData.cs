// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Services.Pagination;

/// <summary>
/// Delegate that fetches a single page of results given provider-native continuation state.
/// </summary>
/// <param name="nativeState">
/// Provider-native continuation state from a previous page, or null to fetch from the beginning.
/// </param>
/// <param name="cancellationToken">A token to cancel the operation.</param>
/// <returns>A <see cref="PaginationPageData"/> with serialized items and optional next-page state.</returns>
public delegate Task<PaginationPageData> PageFetchDelegate(string? nativeState, CancellationToken cancellationToken);

/// <summary>
/// Result returned by a <see cref="PageFetchDelegate"/> containing
/// pre-serialized items and optional native continuation state.
/// </summary>
/// <param name="ItemsJson">JSON-serialized array of items for the current page.</param>
/// <param name="NextNativeState">
/// Provider-native continuation state for the next page, or null if no more pages exist.
/// </param>
public sealed record PaginationPageData(string ItemsJson, string? NextNativeState);
