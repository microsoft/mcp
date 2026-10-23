// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.IoTHub.Commands;

// Shared option validation for IoT Hub commands.
internal static class IoTHubValidation
{
    public const string InvalidHubNameError =
        "--hub-name must be 3-50 characters long and contain only letters, numbers, or hyphens, and it cannot end with a hyphen.";

    public const string InvalidResourceGroupError =
        "--resource-group must be 1-90 characters long and contain only letters, digits, underscores, hyphens, periods, or parentheses, and it cannot end with a period.";

    public static void ValidateHubName(string hubName, ValidationResult validationResult)
    {
        if (!IsValidIoTHubName(hubName))
        {
            validationResult.Errors.Add(InvalidHubNameError);
        }
    }

    // Hub and resource group names are interpolated into ARM resource IDs. A value outside the Azure naming rules can
    // add path segments, such as "rg1/../rg2", that URI normalization resolves to a different resource group.
    public static void ThrowIfInvalidResourceNames(string hubName, string resourceGroup)
    {
        if (!IsValidResourceGroupName(resourceGroup))
        {
            throw new ArgumentException(InvalidResourceGroupError, nameof(resourceGroup));
        }

        if (!IsValidIoTHubName(hubName))
        {
            throw new ArgumentException(InvalidHubNameError, nameof(hubName));
        }
    }

    // Azure resource group names are 1-90 underscores, hyphens, periods, parentheses, and letters or digits as defined by
    // Char.IsLetterOrDigit, and can't end with a period. Runes keep letters outside the Basic Multilingual Plane valid.
    public static bool IsValidResourceGroupName([NotNullWhen(true)] string? value)
    {
        if (string.IsNullOrEmpty(value) || value[^1] == '.')
        {
            return false;
        }

        var length = 0;
        foreach (var rune in value.EnumerateRunes())
        {
            if (++length > 90 ||
                !(Rune.IsLetterOrDigit(rune) || rune.Value is '_' or '-' or '.' or '(' or ')'))
            {
                return false;
            }
        }

        return true;
    }

    public static bool IsValidIoTHubName(string value)
    {
        if (value.Length is < 3 or > 50 || value[^1] == '-')
        {
            return false;
        }

        foreach (var ch in value)
        {
            var isAlphaNumeric = (ch >= 'a' && ch <= 'z') ||
                                 (ch >= 'A' && ch <= 'Z') ||
                                 (ch >= '0' && ch <= '9');
            if (!isAlphaNumeric && ch != '-')
            {
                return false;
            }
        }

        return true;
    }
}
