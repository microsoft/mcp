// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Helpers;

public static class OptionParsingHelpers
{
    /// <summary>
    /// Parses key value pair string options to a dictionary, assuming a format of "Key=Value,Key=Value" (default separators '=' and ',')
    /// If duplicate keys are found, the last value wins.
    /// </summary>
    /// <param name="value">Value string containing key-value pairs</param>
    /// <param name="keyValueSeparator">The character that separates keys from values in the input string.</param>
    /// <param name="pairSeparator">The character that separates key-value pairs in the input string.</param>
    /// <returns>Key Value pairs as dictionary</returns>
    public static Dictionary<string, string> ParseKeyValuePairStringToDictionary(string value, char keyValueSeparator = '=', char pairSeparator = ',')
    {
        return ParseKeyValuePairStringToDictionary(value, StringComparer.OrdinalIgnoreCase, keyValueSeparator, pairSeparator);
    }

    /// <summary>
    /// Parses key value pair string options to a dictionary, assuming a format of "Key=Value,Key=Value" (default separators '=' and ',')
    /// If duplicate keys are found, the last value wins.
    /// </summary>
    /// <param name="value">Value string containing key-value pairs</param>
    /// <param name="keyComparer">The string comparer to use for comparing keys in the resulting dictionary.</param>
    /// <param name="keyValueSeparator">The character that separates keys from values in the input string.</param>
    /// <param name="pairSeparator">The character that separates key-value pairs in the input string.</param>
    /// <exception cref="ArgumentException">Thrown when the input value string is null, empty, or consists only of whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the keyComparer is null.</exception>
    /// <returns>Key Value pairs as dictionary</returns>
    public static Dictionary<string, string> ParseKeyValuePairStringToDictionary(string value, StringComparer keyComparer, char keyValueSeparator = '=', char pairSeparator = ',')
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        ArgumentNullException.ThrowIfNull(keyComparer);

        var result = new Dictionary<string, string>(keyComparer);
        var valuePairs = value
            .Split(pairSeparator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Split(keyValueSeparator, 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            .Where(x => x.Length == 2);

        foreach (var pair in valuePairs)
        {
            result[pair[0]] = pair[1];
        }

        return result;
    }
}
