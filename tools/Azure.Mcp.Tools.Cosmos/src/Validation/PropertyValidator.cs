// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;
using Microsoft.Mcp.Core.Helpers;

namespace Azure.Mcp.Tools.Cosmos.Validation;

/// <summary>
/// Validates property identifiers used inside SQL fragments that cannot be parameterized,
/// such as the property names interpolated into <c>FullTextContains(c.{property}, ...)</c>
/// and <c>VectorDistance(c.{vector}, ...)</c>.
/// </summary>
internal static class PropertyValidator
{
    private static readonly Regex PropertyPattern = RegexHelper.CreateRegex(
        @"^[A-Za-z_][A-Za-z0-9_]*(\.[A-Za-z_][A-Za-z0-9_]*)*$",
        RegexOptions.Compiled);

    /// <summary>
    /// Returns <c>true</c> if the value is a safe dot-delimited property identifier
    /// (letters, digits, and underscores only).
    /// </summary>
    public static bool IsValid(string? value) =>
        !string.IsNullOrWhiteSpace(value) && PropertyPattern.IsMatch(value);
}
