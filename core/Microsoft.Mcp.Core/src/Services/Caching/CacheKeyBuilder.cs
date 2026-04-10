// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;

namespace Azure.Mcp.Core.Services.Caching;

/// <summary>
/// Utility class for building cache keys with consistent formatting.
/// Uses length-prefixed encoding to ensure unambiguous key construction.
/// </summary>
public static class CacheKeyBuilder
{
    /// <summary>
    /// Builds a cache key from a prefix and optional parts using length-prefixed encoding.
    /// Format: "prefix|length1:value1|length2:value2|..."
    /// </summary>
    /// <param name="prefix">The required prefix for the cache key.</param>
    /// <param name="parts">Optional parts to append to the key. Null values are treated as empty strings.</param>
    /// <returns>A formatted cache key string.</returns>
    /// <example>
    /// <code>
    /// var key = CacheKeyBuilder.Build("user", "123", "profile");
    /// // Returns: "user|3:123|7:profile"
    /// </code>
    /// </example>
    public static string Build(string prefix, params string?[] parts)
    {
        ArgumentNullException.ThrowIfNull(prefix);

        var sb = new StringBuilder(prefix.Length + 32);
        sb.Append(prefix);

        foreach (var part in parts)
        {
            var value = part ?? string.Empty;
            sb.Append('|');
            sb.Append(value.Length);
            sb.Append(':');
            sb.Append(value);
        }

        return sb.ToString();
    }
}
