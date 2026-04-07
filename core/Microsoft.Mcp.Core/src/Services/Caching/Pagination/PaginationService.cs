// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Microsoft.Mcp.Core.Services.Caching.Pagination;

/// <summary>
/// Default implementation of <see cref="IPaginationService"/>.
/// Orchestrates fingerprinting and cursor lifecycle management.
/// </summary>
public sealed class PaginationService(
    ICursorRegistry cursorRegistry,
    ILogger<PaginationService> logger) : IPaginationService
{
    internal const int SupportedVersion = 1;
    internal static readonly TimeSpan DefaultTtl = TimeSpan.FromHours(1);

    public string ComputeRequestFingerprint(IReadOnlyDictionary<string, string?> parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        // Sort keys for deterministic ordering, exclude null values.
        var builder = new StringBuilder();
        foreach (var kvp in parameters.OrderBy(static p => p.Key, StringComparer.Ordinal))
        {
            if (kvp.Value is null)
            {
                continue;
            }

            builder.Append(kvp.Key);
            builder.Append('=');
            builder.Append(kvp.Value);
            builder.Append('\n');
        }

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString()));
        return string.Concat("sha256:", Convert.ToHexStringLower(hash));
    }

    public async ValueTask<string> SaveCursorAsync(
        string provider,
        string operation,
        string requestFingerprint,
        string nativeState,
        IReadOnlyDictionary<string, string>? resourceMetadata = null,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(provider);
        ArgumentException.ThrowIfNullOrEmpty(operation);
        ArgumentException.ThrowIfNullOrEmpty(requestFingerprint);
        ArgumentException.ThrowIfNullOrEmpty(nativeState);

        var effectiveTtl = ttl ?? DefaultTtl;
        var now = DateTimeOffset.UtcNow;

        var record = new CursorRecord
        {
            CursorId = CursorRegistry.GenerateCursorId(),
            Provider = provider,
            Operation = operation,
            RequestFingerprint = requestFingerprint,
            NativeState = nativeState,
            ResourceMetadata = resourceMetadata,
            CreatedAtUtc = now,
            ExpiresAtUtc = now + effectiveTtl,
        };

        var cursorId = await cursorRegistry.SetAsync(record, cancellationToken);
        logger.LogDebug("Saved cursor {CursorId} for {Operation} (provider={Provider}, ttl={Ttl})",
            cursorId, operation, provider, effectiveTtl);

        return cursorId;
    }

    public async ValueTask<CursorRecord> LoadAndValidateCursorAsync(
        string cursorId,
        string requestFingerprint,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(cursorId);
        ArgumentException.ThrowIfNullOrEmpty(requestFingerprint);

        var record = await cursorRegistry.GetAsync(cursorId, cancellationToken);
        if (record is null)
        {
            logger.LogWarning("Cursor {CursorId} not found in registry.", cursorId);
            throw new InvalidCursorException(
                InvalidCursorReason.NotFound,
                "The specified cursor is invalid or has expired.");
        }

        if (record.Version != SupportedVersion)
        {
            logger.LogWarning("Cursor {CursorId} has unsupported version {Version}.", cursorId, record.Version);
            throw new InvalidCursorException(
                InvalidCursorReason.UnsupportedVersion,
                "The specified cursor is invalid or has expired.");
        }

        if (!string.Equals(record.RequestFingerprint, requestFingerprint, StringComparison.Ordinal))
        {
            logger.LogWarning("Cursor {CursorId} fingerprint mismatch. Expected={Expected}, Actual={Actual}",
                cursorId, record.RequestFingerprint, requestFingerprint);
            throw new InvalidCursorException(
                InvalidCursorReason.FingerprintMismatch,
                "The specified cursor is invalid or has expired.");
        }

        logger.LogDebug("Cursor {CursorId} validated successfully for {Operation}.", cursorId, record.Operation);
        return record;
    }
}
