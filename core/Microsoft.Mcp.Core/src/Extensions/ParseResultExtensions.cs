// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Extensions;

public static class ParseResultExtensions
{
    public static bool TryGetValue<T>(this ParseResult parseResult, Option<T> option, out T? value)
        => parseResult.CommandResult.TryGetValue(option, out value);

    public static T? GetValueOrDefault<T>(this ParseResult parseResult, Option<T> option)
        => parseResult.CommandResult.GetValueOrDefault(option);
}
