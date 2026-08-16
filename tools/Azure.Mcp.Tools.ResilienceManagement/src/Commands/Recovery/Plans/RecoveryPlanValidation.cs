// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Plans;

internal static class RecoveryPlanValidation
{
    public static void ValidateName(string recoveryPlan, ValidationResult validationResult)
    {
        if (recoveryPlan.Length is < 5 or > 24 || !recoveryPlan.All(IsValidNameCharacter))
        {
            validationResult.Errors.Add("The recovery plan name must be 5 to 24 characters and contain only ASCII letters, numbers, or hyphens.");
        }
    }

    private static bool IsValidNameCharacter(char character) =>
        character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '-';
}