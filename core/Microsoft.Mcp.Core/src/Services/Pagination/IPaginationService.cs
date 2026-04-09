// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Services.Pagination;

/// <summary>
/// Shared pagination infrastructure used by paged commands.
/// Responsible for computing request fingerprints, resolving caller bindings,
/// and loading/saving cursor records with validation.
/// </summary>
public interface IPaginationService
{
    /// <summary>
    /// Computes a SHA-256 fingerprint from the normalized semantic request parameters.
    /// Parameters that change across pages (e.g., <c>cursor</c>) must be excluded by the caller.
    /// </summary>
    /// <param name="parameters">
    /// Key-value pairs representing the semantic request shape
    /// (e.g., operation, subscription, resourceGroup, filter).
    /// Null values are excluded from the fingerprint.
    /// </param>
    /// <returns>A fingerprint string in the form <c>sha256:&lt;hex&gt;</c>.</returns>
    string ComputeRequestFingerprint(IReadOnlyDictionary<string, string?> parameters);

    /// <summary>
    /// Creates a new cursor record and stores it in the registry.
    /// </summary>
    /// <param name="provider">The provider adapter family (e.g., "arm", "cosmos", "kql").</param>
    /// <param name="operation">The logical MCP tool or operation name (e.g., "resourcegroup.list").</param>
    /// <param name="requestFingerprint">The fingerprint of the semantic request shape.</param>
    /// <param name="nativeState">Provider-native continuation state.</param>
    /// <param name="fetcher">Optional delegate that fetches the next page of results.</param>
    /// <param name="resourceMetadata">Optional metadata about the resource scope.</param>
    /// <param name="ttl">Optional cursor TTL. Defaults to 1 hour if not specified.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The opaque cursor ID for the stored record.</returns>
    ValueTask<string> SaveCursorAsync(
        string provider,
        string operation,
        string requestFingerprint,
        string nativeState,
        PageFetchDelegate? fetcher = null,
        IReadOnlyDictionary<string, string>? resourceMetadata = null,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a cursor record from the registry and validates the request fingerprint
    /// against the current request context.
    /// </summary>
    /// <param name="cursorId">The opaque cursor ID to load.</param>
    /// <param name="requestFingerprint">The fingerprint of the current request for validation.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The validated cursor record.</returns>
    /// <exception cref="InvalidCursorException">
    /// Thrown when the cursor is not found, has expired, or fails validation.
    /// </exception>
    ValueTask<CursorRecord> LoadAndValidateCursorAsync(
        string cursorId,
        string requestFingerprint,
        CancellationToken cancellationToken = default);
}
