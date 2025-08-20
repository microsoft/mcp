// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Helpers;

namespace Azure.Mcp.Core.Commands;

/// <summary>
/// Extensions for parsing command options from dictionary arguments
/// </summary>
public static class CommandExtensions
{
    /// <summary>
    /// Parse command options directly from a dictionary of arguments
    /// </summary>
    /// <param name="command">The command to parse options for</param>
    /// <param name="arguments">Dictionary of argument name/value pairs</param>
    /// <returns>ParseResult containing the parsed arguments</returns>
    public static ParseResult ParseFromDictionary(this Command command, IReadOnlyDictionary<string, JsonElement>? arguments)
    {
        if (arguments == null || arguments.Count == 0)
        {
            return command.Parse(Array.Empty<string>());
        }

        var args = new List<string>();

        foreach (var (key, value) in arguments)
        {
            var option = command.Options.FirstOrDefault(o =>
                o.Name.Equals(key, StringComparison.OrdinalIgnoreCase));

            if (option == null)
            {
                continue;
            }

            if (value.ValueKind == JsonValueKind.Null)
            {
                continue;
            }

            // Handle arrays for options that accept multiple values (collections)
            if (value.ValueKind == JsonValueKind.Array && IsArrayOption(option))
            {
                foreach (var arrayElement in value.EnumerateArray())
                {
                    args.Add($"--{option.Name}");

                    var elementValue = ConvertJsonElementToString(arrayElement);

                    if (elementValue != null)
                    {
                        args.Add(elementValue);
                    }
                }
            }
            else
            {
                args.Add($"--{option.Name}");

                var strValue = ConvertJsonElementToString(value);

                if (strValue != null)
                {
                    args.Add(strValue);
                }
            }
        }

        return command.Parse([.. args]);
    }

    /// <summary>
    /// Determines if an option expects a collection type that should be treated as an array
    /// </summary>
    /// <param name="option">The option to check</param>
    /// <returns>True if the option expects a collection type, false otherwise</returns>
    private static bool IsArrayOption(Option option)
    {
        return CollectionTypeHelper.IsArrayType(option.ValueType);
    }

    /// <summary>
    /// Converts a JsonElement to its string representation for command-line arguments
    /// </summary>
    /// <param name="element">The JsonElement to convert</param>
    /// <returns>String representation of the element, or null if conversion fails</returns>
    private static string? ConvertJsonElementToString(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Number => element.GetRawText(),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Array => string.Join(" ", element.EnumerateArray().Select(e => e.GetString() ?? string.Empty)),
            _ => element.GetRawText()
        };
    }
}
