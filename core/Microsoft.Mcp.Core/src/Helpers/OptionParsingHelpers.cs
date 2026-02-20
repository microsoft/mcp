namespace Azure.Mcp.Core.Helpers;

public static class OptionParsingHelpers
{
    /// <summary>
    /// Parses key value pair string options to a dictionary, assuming a format of "Key=Value,Key=Value" (default separators '=' and ',')
    /// If duplicate keys are found, the last value wins.
    /// </summary>
    /// <param name="value">Value string containing key-value pairs</param>
    /// <returns>Key Value pairs as dictionary</returns>
    public static Dictionary<string, string> ParseKeyValuePairStringToDictionary(string value, char keyValueSeparator = '=', char pairSeparator = ',')
    {
        return ParseKeyValuePairStringToDictionary(value, StringComparer.OrdinalIgnoreCase, keyValueSeparator, pairSeparator);
    }

    /// <summary>
    /// Parses key value pair string options to a dictionary, assuming a format of "Key=Value,Key=Value" (default separators '=' and ',')
    /// If duplicate keys are found, the last value wins.
    /// </summary>
    /// <param name="header">Value string containing key-value pairs</param>
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
