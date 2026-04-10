// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;

namespace Azure.Mcp.Tools.Kusto.Validation;

/// <summary>
/// Validates Kusto identifiers (table names, database names) to prevent KQL injection.
/// Kusto identifiers follow specific naming rules:
/// - Letters, digits, underscores, spaces, hyphens, and periods are allowed
/// - Must start with a letter or underscore
/// - Maximum length of 1024 characters
/// This prevents injection attacks where a user-supplied table name could contain
/// KQL operators like "|" to chain additional commands.
/// </summary>
internal static class KustoIdentifierValidator
{
    private const int MaxIdentifierLength = 1024;

    // Kusto identifiers: letters, digits, underscores, spaces, hyphens, periods.
    // Must start with a letter or underscore.
    private static readonly Regex ValidIdentifierPattern = RegexHelper.CreateRegex(
        @"^[A-Za-z_][A-Za-z0-9_ .\-]*$",
        RegexOptions.Compiled);

    /// <summary>
    /// Validates that the given identifier is a safe Kusto table/database name.
    /// Throws <see cref="CommandValidationException"/> for invalid input.
    /// </summary>
    public static void ValidateIdentifier(string? identifier, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            throw new CommandValidationException($"{parameterName} cannot be empty.");
        }

        if (identifier.Length > MaxIdentifierLength)
        {
            throw new CommandValidationException(
                $"{parameterName} length exceeds maximum of {MaxIdentifierLength} characters.");
        }

        // Reject any KQL operators or injection characters
        if (identifier.IndexOfAny(['|', ';', '(', ')', '{', '}', '[', ']', '<', '>', '\'', '"', '`', '/', '\\', '\n', '\r']) != -1)
        {
            throw new CommandValidationException(
                $"{parameterName} contains invalid characters. Only letters, digits, underscores, spaces, hyphens, and periods are allowed.");
        }

        if (!ValidIdentifierPattern.IsMatch(identifier))
        {
            throw new CommandValidationException(
                $"{parameterName} is not a valid Kusto identifier. Must start with a letter or underscore and contain only letters, digits, underscores, spaces, hyphens, and periods.");
        }
    }
}
