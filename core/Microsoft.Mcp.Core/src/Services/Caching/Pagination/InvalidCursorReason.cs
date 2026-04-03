// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Services.Caching.Pagination;

/// <summary>
/// Internal diagnostic reasons for cursor validation failures.
/// Used for logging and metrics while the external error is always <see cref="InvalidCursorException"/>.
/// </summary>
public enum InvalidCursorReason
{
    /// <summary>The cursor ID was not found in the registry (may have expired from the cache).</summary>
    NotFound,

    /// <summary>The cursor record exists but its TTL has expired.</summary>
    Expired,

    /// <summary>The request fingerprint does not match the stored fingerprint.</summary>
    FingerprintMismatch,

    /// <summary>The caller binding does not match the stored binding.</summary>
    CallerBindingMismatch,

    /// <summary>The cursor record version is not supported.</summary>
    UnsupportedVersion,
}
