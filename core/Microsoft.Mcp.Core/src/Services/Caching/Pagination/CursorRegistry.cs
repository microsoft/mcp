// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Buffers.Text;
using System.Security.Cryptography;

namespace Microsoft.Mcp.Core.Services.Caching.Pagination;

/// <summary>
/// Implementation of <see cref="ICursorRegistry"/> that delegates storage to <see cref="ICacheService"/>.
/// Cursor records are stored under a dedicated cache group with TTL derived from
/// <see cref="CursorRecord.ExpiresAtUtc"/>.
/// </summary>
public sealed class CursorRegistry(ICacheService cacheService) : ICursorRegistry
{
    internal const string CacheGroup = "cursors";
    internal const string CursorPrefix = "cur_";

    public async ValueTask<string> SetAsync(CursorRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);

        var ttl = record.ExpiresAtUtc - DateTimeOffset.UtcNow;
        if (ttl <= TimeSpan.Zero)
        {
            throw new ArgumentException("Cursor record has already expired.", nameof(record));
        }

        await cacheService.SetAsync(CacheGroup, record.CursorId, record, ttl, cancellationToken);

        return record.CursorId;
    }

    public async ValueTask<CursorRecord?> GetAsync(string cursorId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(cursorId);

        return await cacheService.GetAsync<CursorRecord>(CacheGroup, cursorId, cancellationToken: cancellationToken);
    }

    public async ValueTask DeleteAsync(string cursorId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(cursorId);

        await cacheService.DeleteAsync(CacheGroup, cursorId, cancellationToken);
    }

    /// <summary>
    /// Generates a new opaque cursor ID with the <c>cur_</c> prefix.
    /// Uses a cryptographically random 16-byte value encoded as base64url for compactness.
    /// </summary>
    public static string GenerateCursorId()
    {
        Span<byte> bytes = stackalloc byte[16];
        RandomNumberGenerator.Fill(bytes);

        return string.Concat(CursorPrefix, Base64Url.EncodeToString(bytes));
    }
}
