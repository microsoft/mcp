// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Helpers;

/// <summary>
/// Centralizes the <see cref="StringComparison"/> values to use when comparing
/// well-known string values, making the intended comparison semantics explicit
/// at each call site.
/// </summary>
public static class StringComparisons
{
    /// <summary>
    /// The comparison to use when comparing Azure tenant IDs. Tenant IDs are
    /// GUIDs whose casing is not significant, so they are compared
    /// case-insensitively.
    /// </summary>
    public static StringComparison TenantId => StringComparison.OrdinalIgnoreCase;
}
