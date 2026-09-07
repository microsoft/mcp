// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.UsagePlans;

internal static class UsagePlanResourceNameValidator
{
    public static void Validate(string name, string resourceType, ValidationResult validationResult)
    {
        if (name.Length is < 3 or > 24 || !name.All(IsAsciiLetterNumberOrHyphen))
        {
            validationResult.Errors.Add($"The {resourceType} name must be 3 to 24 characters and contain only ASCII letters, numbers, or hyphens.");
        }
    }

    private static bool IsAsciiLetterNumberOrHyphen(char character) =>
        character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '-';
}
