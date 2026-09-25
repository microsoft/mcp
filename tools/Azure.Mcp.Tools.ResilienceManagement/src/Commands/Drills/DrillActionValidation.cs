// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Drills;

internal static class DrillActionValidation
{
    private static readonly HashSet<string> s_modes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Failover",
        "TestFailover"
    };

    private static readonly HashSet<string> s_attestations = new(StringComparer.OrdinalIgnoreCase)
    {
        "Success",
        "Failed"
    };

    public static void ValidateResourceNames(string serviceGroup, string drill, ValidationResult validationResult)
    {
        if (serviceGroup.Length is < 1 or > 90 || !serviceGroup.All(IsValidServiceGroupNameCharacter))
        {
            validationResult.Errors.Add("The service group name must be 1 to 90 characters and contain only ASCII letters, numbers, hyphens, underscores, periods, or parentheses.");
        }

        if (drill.Length is < 1 or > 260 || !drill.All(IsValidDrillNameCharacter))
        {
            validationResult.Errors.Add("The drill name must be 1 to 260 characters and contain only ASCII letters, numbers, hyphens, underscores, or periods.");
        }
    }

    public static void ValidateMode(string mode, ValidationResult validationResult)
    {
        if (!s_modes.Contains(mode))
        {
            validationResult.Errors.Add("The drill mode must be either Failover or TestFailover.");
        }
    }

    public static void ValidateAttestation(string attestation, ValidationResult validationResult)
    {
        if (!s_attestations.Contains(attestation))
        {
            validationResult.Errors.Add("The drill attestation must be either Success or Failed.");
        }
    }

    public static string NormalizeMode(string mode) =>
        mode.Equals("Failover", StringComparison.OrdinalIgnoreCase) ? "Failover" : "TestFailover";

    public static string NormalizeAttestation(string attestation) =>
        attestation.Equals("Success", StringComparison.OrdinalIgnoreCase) ? "Success" : "Failed";

    private static bool IsValidDrillNameCharacter(char character) =>
        character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '-' or '_' or '.';

    private static bool IsValidServiceGroupNameCharacter(char character) =>
        IsValidDrillNameCharacter(character) || character is '(' or ')';
}
