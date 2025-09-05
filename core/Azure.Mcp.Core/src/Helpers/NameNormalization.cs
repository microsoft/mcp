// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Helpers;

/// <summary>
/// Normalization helpers for command and option identifiers.
/// </summary>
public static class NameNormalization
{
    /// <summary>
    /// Normalizes an option or alias name by removing common prefix characters like '-' or '/'.
    /// This produces a consistent, prefix-free key for comparisons and schema generation.
    /// </summary>
    /// <param name="name">The raw option name or alias (may include leading '-' or '/').</param>
    /// <returns>The normalized name without leading prefix characters, or empty string if null.</returns>
    public static string NormalizeOptionName(string? name) => (name ?? string.Empty).TrimStart('-', '/');
}
