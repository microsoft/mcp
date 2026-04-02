// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Services.Caching.Pagination;

/// <summary>
/// Registry for managing opaque pagination cursors backed by <see cref="ICacheService"/>.
/// Stores <see cref="CursorRecord"/> entries keyed by their public cursor ID.
/// </summary>
public interface ICursorRegistry
{
    /// <summary>
    /// Stores a cursor record and returns the public cursor ID.
    /// </summary>
    /// <param name="record">The cursor record to store.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The opaque cursor ID that was stored.</returns>
    ValueTask<string> SetAsync(CursorRecord record, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a cursor record by its public cursor ID.
    /// Returns null if the cursor does not exist or has expired.
    /// </summary>
    /// <param name="cursorId">The opaque cursor ID.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The stored cursor record, or null if not found or expired.</returns>
    ValueTask<CursorRecord?> GetAsync(string cursorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a cursor record by its public cursor ID.
    /// </summary>
    /// <param name="cursorId">The opaque cursor ID to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    ValueTask DeleteAsync(string cursorId, CancellationToken cancellationToken = default);
}
