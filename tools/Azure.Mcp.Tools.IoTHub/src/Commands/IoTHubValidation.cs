// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.IoTHub.Commands;

// Shared option validation for IoT Hub commands.
internal static class IoTHubValidation
{
    public const string InvalidHubNameError =
        "--hub-name must be 3-50 characters long and contain only letters, numbers, or hyphens, and it cannot end with a hyphen.";

    public static void ValidateHubName(string hubName, ValidationResult validationResult)
    {
        if (!IsValidIoTHubName(hubName))
        {
            validationResult.Errors.Add(InvalidHubNameError);
        }
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
