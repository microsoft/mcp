// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;

namespace Azure.Mcp.Core.Extensions;

public static class CommandResultExtensions
{
    /// <summary>
    /// Safely attempts to get the value for an option from a <see cref="CommandResult"/>.
    /// This first checks for an explicit <see cref="OptionResult"/> with tokens (i.e. the
    /// option was supplied). If present, it attempts to read the typed value via the
    /// underlying <see cref="ParseResult.GetValue{T}(Option{T})"/> in a try/catch to avoid
    /// System.CommandLine throwing for missing/invalid options. If the option wasn't supplied
    /// explicitly, this method still attempts to call <c>parseResult.GetValue(option)</c> which
    /// will return a default value if the option defines a default value factory.
    /// Returns <c>true</c> if a value (or a default value) was obtained, otherwise <c>false</c>.
    /// </summary>
    public static bool TryGetValue<T>(this CommandResult commandResult, Option<T> option, out T? value)
    {
        // Look for an explicit OptionResult for this option on the command (supplied by user)
        var optionResult = commandResult.Children
            .OfType<OptionResult>()
            .FirstOrDefault(r => r.Option == option);

        // If the option result is present and has tokens, prefer that value (guarded)
        if (optionResult != null && optionResult.Tokens != null && optionResult.Tokens.Count > 0)
        {
            try
            {
                value = commandResult.GetValue(option);
                return true;
            }
            catch
            {
                value = default;
                return false;
            }
        }

        // Option was not explicitly supplied. Try to obtain a default or computed value
        // from the ParseResult. This may throw for some SCL states, so catch and treat as absent.
        try
        {
            value = commandResult.GetValue(option);
            // If the returned value is null/empty for reference types, treat as absent.
            if (value is string s && string.IsNullOrWhiteSpace(s))
            {
                return false;
            }

            return true;
        }
        catch
        {
            value = default;
            return false;
        }
    }
}
