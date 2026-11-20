// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;

namespace Microsoft.Mcp.Core.Helpers;

/// <summary>
/// Helpers for extracting telemetry-safe diagnostics from MSAL exceptions.
/// </summary>
public static partial class MsalErrorHelper
{
    /// <summary>
    /// Extracts the Microsoft Entra ID error code (for example, <c>AADSTS50013</c>) from an MSAL exception message.
    /// MSAL error codes such as <c>invalid_request</c> are generic; the AADSTS code identifies the specific cause.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <returns>The first AADSTS error code in the message, or <see langword="null"/> if none is present.</returns>
    public static string? GetAadStsErrorCode(string? message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return null;
        }

        var match = AadStsErrorCodeRegex().Match(message);
        return match.Success ? match.Value : null;
    }

    [GeneratedRegex(@"\bAADSTS\d+\b")]
    private static partial Regex AadStsErrorCodeRegex();
}
