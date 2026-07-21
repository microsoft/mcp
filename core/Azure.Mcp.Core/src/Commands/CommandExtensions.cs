// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Buffers;
using System.Text;
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
            return command.Parse([]);
        }

        var args = new List<string>();

        foreach (var (key, value) in arguments)
        {
            // lookup by normalized name or any alias (case-insensitive)
            var option = command.Options.FirstOrDefault(o =>
                string.Equals(NameNormalization.NormalizeOptionName(o.Name), key, StringComparison.OrdinalIgnoreCase)
                || o.Aliases.Any(a => string.Equals(NameNormalization.NormalizeOptionName(a), key, StringComparison.OrdinalIgnoreCase)));

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
                    args.Add(GetOptionToken(option));

                    var elementValue = ConvertJsonElementToString(arrayElement);

                    if (elementValue != null)
                    {
                        args.Add(elementValue);
                    }
                }
            }
            else
            {
                args.Add(GetOptionToken(option));

                var strValue = ConvertJsonElementToString(value);

                if (strValue != null)
                {
                    args.Add(strValue);
                }
            }
        }

        return command.Parse([.. args]);
    }

    public static ParseResult ParseFromRawMcpToolInput(this Command command, IReadOnlyDictionary<string, JsonElement>? arguments)
    {
        var args = new List<string>();

        // Try to find an option named "raw-mcp-tool-input" (normalized), otherwise fall back to first option
        var option = command.Options.FirstOrDefault(o =>
            string.Equals(NameNormalization.NormalizeOptionName(o.Name), "raw-mcp-tool-input", StringComparison.OrdinalIgnoreCase)
            || o.Aliases.Any(a => string.Equals(NameNormalization.NormalizeOptionName(a), "raw-mcp-tool-input", StringComparison.OrdinalIgnoreCase))
        );

        option ??= command.Options.FirstOrDefault();

        if (option == null)
        {
            return command.Parse([]);
        }

        args.Add(GetOptionToken(option));

        if (arguments == null || arguments.Count == 0)
        {
            args.Add("{}");
        }
        else
        {
            var buffer = new ArrayBufferWriter<byte>();
            using (var jsonWriter = new Utf8JsonWriter(buffer))
            {
                jsonWriter.WriteStartObject();
                foreach (var argument in arguments)
                {
                    jsonWriter.WritePropertyName(argument.Key);
                    argument.Value.WriteTo(jsonWriter);
                }
                jsonWriter.WriteEndObject();
            }
            args.Add(Encoding.UTF8.GetString(buffer.WrittenSpan));
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

    private static string GetOptionToken(Option option)
    {
        // Prefer an alias that already contains a prefix
        var aliasWithDash = option.Aliases.FirstOrDefault(a => a.StartsWith('-') || a.StartsWith('/'));
        if (!string.IsNullOrEmpty(aliasWithDash))
            return aliasWithDash;

        return option.Name.StartsWith('-') || option.Name.StartsWith('/') ? option.Name : $"--{option.Name}";
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
