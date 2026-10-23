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

    /// <summary>
    /// The comparison to use when comparing Azure subscription IDs. Subscription
    /// IDs are GUIDs whose casing is not significant, so they are compared
    /// case-insensitively.
    /// </summary>
    public static StringComparison SubscriptionId => StringComparison.OrdinalIgnoreCase;

    /// <summary>
    /// The comparison to use when comparing Azure subscription display names,
    /// which are matched case-insensitively.
    /// </summary>
    public static StringComparison SubscriptionDisplayName => StringComparison.OrdinalIgnoreCase;

    /// <summary>
    /// The comparison to use when comparing Azure resource group names, which
    /// Azure Resource Manager treats case-insensitively.
    /// </summary>
    public static StringComparison ResourceGroup => StringComparison.OrdinalIgnoreCase;

    /// <summary>
    /// The comparison to use when matching an Azure resource by its name. Azure
    /// resource names are matched case-insensitively for lookup purposes.
    /// </summary>
    public static StringComparison ResourceName => StringComparison.OrdinalIgnoreCase;
}
