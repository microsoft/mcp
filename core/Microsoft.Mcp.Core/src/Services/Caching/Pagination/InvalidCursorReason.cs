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

    /// <summary>The request fingerprint does not match the stored fingerprint.</summary>
    FingerprintMismatch,

    /// <summary>The cursor record version is not supported.</summary>
    UnsupportedVersion,
}
