// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Buffers.Text;
using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.Mcp.Core.Services.Caching.Pagination;

/// <summary>
/// Implementation of <see cref="ICursorRegistry"/> that stores cursor records directly
/// in <see cref="IMemoryCache"/> with TTL derived from <see cref="CursorRecord.ExpiresAtUtc"/>.
/// </summary>
public sealed class CursorRegistry(IMemoryCache memoryCache) : ICursorRegistry
{
    internal const string CursorPrefix = "cur_";

    public ValueTask<string> SetAsync(CursorRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);

        var ttl = record.ExpiresAtUtc - DateTimeOffset.UtcNow;
        if (ttl <= TimeSpan.Zero)
        {
            throw new ArgumentException("Cursor record has already expired.", nameof(record));
        }

        memoryCache.Set(record.CursorId, record, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        });

        return new ValueTask<string>(record.CursorId);
    }

    public ValueTask<CursorRecord?> GetAsync(string cursorId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(cursorId);

        memoryCache.TryGetValue(cursorId, out CursorRecord? record);
        return new ValueTask<CursorRecord?>(record);
    }

    public ValueTask DeleteAsync(string cursorId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(cursorId);

        memoryCache.Remove(cursorId);
        return default;
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
