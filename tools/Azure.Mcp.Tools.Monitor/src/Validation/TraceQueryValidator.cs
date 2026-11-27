// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Monitor.Validation;

internal static class TraceQueryValidator
{
    internal static bool IsFieldReference(string? value) =>
        value is { Length: > 0 and <= 1024 } &&
        Regex.IsMatch(value, @"\A[a-zA-Z_][a-zA-Z0-9_.]*\z", RegexOptions.NonBacktracking);

    public static void ValidateWindow(DateTimeOffset start, DateTimeOffset end, ValidationResult result)
    {
        if (start >= end || end - start > TimeSpan.FromDays(1))
        {
            result.AddError("The time range must be positive and no longer than one day.", "Invalid time range.");
        }
    }
}
