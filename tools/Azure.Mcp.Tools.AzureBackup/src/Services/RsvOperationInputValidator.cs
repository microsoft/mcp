// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Services;

internal static class RsvOperationInputValidator
{
    internal static IEnumerable<string> Validate(
        string? operation, string? vault, string? resourceGroup,
        string? container, string? protectedItem, string? fabric)
    {
        // Microsoft.RecoveryServices/vaults: 2-50, ASCII alphanumeric/hyphen, starts with a letter.
        if (vault is not { Length: >= 2 and <= 50 } || !char.IsAsciiLetter(vault[0]) ||
            vault.Any(c => !char.IsAsciiLetterOrDigit(c) && c != '-'))
        {
            yield return "--vault must be 2-50 letters, digits or hyphens and start with a letter.";
        }

        // ARM resource groups allow Unicode letters/digits and these four punctuation characters.
        if (resourceGroup is not { Length: >= 1 and <= 90 } || resourceGroup.EndsWith('.') ||
            resourceGroup.Any(c => !char.IsLetterOrDigit(c) && c is not ('_' or '-' or '.' or '(' or ')')))
        {
            yield return "--resource-group must be 1-90 letters, digits, underscores, hyphens, periods or parentheses and cannot end in a period.";
        }

        // These are service-generated opaque identifiers, not GUIDs or user-created resource names.
        // Reject URL structure and pre-encoding, but retain semicolons, colons, plus and equals
        // used in real container names and operation tokens. Encode each segment at the boundary.
        if (!IsSafeSegment(operation))
        {
            yield return "--operation must be a nonempty decoded operation ID, without path separators, percent encoding, query, fragment or control characters.";
        }
        if ((container is null) != (protectedItem is null))
        {
            yield return "--container and --protected-item must be supplied together.";
        }
        if (container is not null && !IsSafeSegment(container))
        {
            yield return "--container must be a nonempty decoded RSV container name, not a path or URL.";
        }
        if (protectedItem is not null && !IsSafeSegment(protectedItem))
        {
            yield return "--protected-item must be a nonempty decoded RSV item name, not a path or URL.";
        }
        if (fabric is not null && (!IsSafeSegment(fabric) || container is null || protectedItem is null))
        {
            yield return "--fabric must be a decoded fabric name and requires both --container and --protected-item.";
        }
    }

    internal static bool IsSafeSegment(string? value) =>
        !string.IsNullOrWhiteSpace(value) && value is not ("." or "..") &&
        !value.Any(c => char.IsControl(c) || c is '/' or '\\' or '?' or '#' or '%');
}
