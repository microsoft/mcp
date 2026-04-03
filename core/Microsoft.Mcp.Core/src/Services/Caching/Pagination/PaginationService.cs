// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Microsoft.Mcp.Core.Services.Caching.Pagination;

/// <summary>
/// Default implementation of <see cref="IPaginationService"/>.
/// Orchestrates fingerprinting, caller binding resolution, and cursor lifecycle management.
/// </summary>
public sealed class PaginationService(
    ICursorRegistry cursorRegistry,
    ICallerIdentityResolver callerIdentityResolver,
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

    public ValueTask<CallerBinding> ResolveCallerBindingAsync(CancellationToken cancellationToken = default) =>
        callerIdentityResolver.ResolveAsync(cancellationToken);

    public async ValueTask<string> SaveCursorAsync(
        string provider,
        string operation,
        string requestFingerprint,
        CallerBinding callerBinding,
        string nativeState,
        IReadOnlyDictionary<string, string>? resourceMetadata = null,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(provider);
        ArgumentException.ThrowIfNullOrEmpty(operation);
        ArgumentException.ThrowIfNullOrEmpty(requestFingerprint);
        ArgumentNullException.ThrowIfNull(callerBinding);
        ArgumentException.ThrowIfNullOrEmpty(nativeState);

        var effectiveTtl = ttl ?? DefaultTtl;
        var now = DateTimeOffset.UtcNow;

        var record = new CursorRecord
        {
            CursorId = CursorRegistry.GenerateCursorId(),
            Provider = provider,
            Operation = operation,
            RequestFingerprint = requestFingerprint,
            CallerBinding = callerBinding,
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

        if (record.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            logger.LogWarning("Cursor {CursorId} has expired (expiry={ExpiresAtUtc}).", cursorId, record.ExpiresAtUtc);
            await cursorRegistry.DeleteAsync(cursorId, cancellationToken);
            throw new InvalidCursorException(
                InvalidCursorReason.Expired,
                "The specified cursor has expired.");
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

        var currentBinding = await callerIdentityResolver.ResolveAsync(cancellationToken);
        if (!CallerBindingsMatch(record.CallerBinding, currentBinding))
        {
            logger.LogWarning("Cursor {CursorId} caller binding mismatch. StoredMode={StoredMode}, CurrentMode={CurrentMode}",
                cursorId, record.CallerBinding.Mode, currentBinding.Mode);
            throw new InvalidCursorException(
                InvalidCursorReason.CallerBindingMismatch,
                "The specified cursor is invalid or has expired.");
        }

        logger.LogDebug("Cursor {CursorId} validated successfully for {Operation}.", cursorId, record.Operation);
        return record;
    }

    internal static bool CallerBindingsMatch(CallerBinding stored, CallerBinding current) =>
        string.Equals(stored.Mode, current.Mode, StringComparison.Ordinal) &&
        string.Equals(stored.TenantId, current.TenantId, StringComparison.Ordinal) &&
        string.Equals(stored.PrincipalIdHash, current.PrincipalIdHash, StringComparison.Ordinal);
}
