// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;

namespace Azure.Mcp.Core.Options;

public static class ParseResultExtensions
{
    public static bool HasAnyRetryOptions(this System.CommandLine.ParseResult parseResult)
    {
        foreach (var child in parseResult.CommandResult.Children)
        {
            if (child is OptionResult optionResult)
            {
                var option = optionResult.Option;
                if (option is null)
                {
                    continue;
                }

                var aliases = option.Aliases;
                if (aliases.Any(a => string.Equals(a, $"--{Models.Option.OptionDefinitions.RetryPolicy.DelayName}", StringComparison.OrdinalIgnoreCase)) ||
                    aliases.Any(a => string.Equals(a, $"--{Models.Option.OptionDefinitions.RetryPolicy.MaxDelayName}", StringComparison.OrdinalIgnoreCase)) ||
                    aliases.Any(a => string.Equals(a, $"--{Models.Option.OptionDefinitions.RetryPolicy.MaxRetriesName}", StringComparison.OrdinalIgnoreCase)) ||
                    aliases.Any(a => string.Equals(a, $"--{Models.Option.OptionDefinitions.RetryPolicy.ModeName}", StringComparison.OrdinalIgnoreCase)) ||
                    aliases.Any(a => string.Equals(a, $"--{Models.Option.OptionDefinitions.RetryPolicy.NetworkTimeoutName}", StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
