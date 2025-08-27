// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;

namespace Azure.Mcp.Core.Options;

public static class ParseResultExtensions
{
    public static bool HasAnyRetryOptions(this System.CommandLine.ParseResult parseResult)
    {
        // Compare normalized names (trim leading '-' or '/') so we don't depend on alias formatting
        static string Normalize(string s) => (s ?? string.Empty).TrimStart('-', '/');

        var retryNames = new[]
        {
            Models.Option.OptionDefinitions.RetryPolicy.DelayName,
            Models.Option.OptionDefinitions.RetryPolicy.MaxDelayName,
            Models.Option.OptionDefinitions.RetryPolicy.MaxRetriesName,
            Models.Option.OptionDefinitions.RetryPolicy.ModeName,
            Models.Option.OptionDefinitions.RetryPolicy.NetworkTimeoutName,
        };

        foreach (var child in parseResult.CommandResult.Children)
        {
            if (child is OptionResult optionResult)
            {
                var option = optionResult.Option;
                if (option is null)
                {
                    continue;
                }

                var name = Normalize(option.Name);
                if (retryNames.Any(rn => string.Equals(rn, name, StringComparison.OrdinalIgnoreCase)))
                    return true;

                var aliases = option.Aliases ?? Array.Empty<string>();
                if (aliases.Any(a => retryNames.Any(rn => string.Equals(rn, Normalize(a), StringComparison.OrdinalIgnoreCase))))
                    return true;
            }
        }

        return false;
    }
}
