// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Options;

public static class ParseResultExtensions
{
    public static bool HasAnyRetryOptions(this System.CommandLine.Parsing.ParseResult parseResult) =>
        // Check if any retry-related options were specified on the command line
        parseResult.CommandResult.Children
            .Any(token => token.Symbol.Name.StartsWith("retry-", StringComparison.OrdinalIgnoreCase));
}
