// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;

namespace Microsoft.Mcp.Core.Options;

/// <summary>
/// Provides case-insensitive validation and completion for enum options represented as Option&lt;string&gt;.
/// </summary>
internal static class EnumOptionValidator
{
    /// <summary>
    /// Configures an option with case-insensitive enum validation and completion sources.
    /// Mirrors the behavior of AcceptOnlyFromAmong but with case-insensitive matching.
    /// </summary>
    public static void Configure(Option<string> option, Type enumType)
    {
        var names = Enum.GetNames(enumType);
        var allowed = string.Join(", ", names);
        var namesSet = new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);

        option.Validators.Add(result =>
        {
            foreach (Token token in result.Tokens)
            {
                if (!namesSet.Contains(token.Value))
                {
                    result.AddError($"Argument '{token.Value}' not recognized. Must be one of: {allowed}");
                }
            }
        });

        option.CompletionSources.Add(names);
    }
}
