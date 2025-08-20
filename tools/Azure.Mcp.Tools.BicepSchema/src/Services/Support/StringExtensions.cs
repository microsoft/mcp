// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.BicepSchema.Services.Support;

public static class StringExtensions
{
    public static bool ContainsOrdinalInsensitively(this string input, string value)
    {
        return input.Contains(value, StringComparison.OrdinalIgnoreCase);
    }
    public static bool StartsWithOrdinalInsensitively(this string original, string prefix)
    {
        return original.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
    }

    public static IOrderedEnumerable<TSource> OrderByAscendingOrdinalInsensitively<TSource>(this IEnumerable<TSource> source, Func<TSource, string> keySelector)
    {
        return source.OrderByAscending(keySelector, StringComparer.OrdinalIgnoreCase);
    }

    public static IOrderedEnumerable<TSource> OrderByAscending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
    {
        return source.OrderBy(keySelector, comparer);
    }

    public static bool EqualsOrdinalInsensitively(this string source, string other)
    {
        return source?.Equals(other, StringComparison.OrdinalIgnoreCase) ?? false;
    }

    public static string JoinWithComma(this IEnumerable<string> source)
    {
        return string.Join(", ", source);
    }

    public static string? NullIfEmptyOrWhitespace(this string? source)
    {
        return string.IsNullOrWhiteSpace(source) ? null : source;
    }
}
