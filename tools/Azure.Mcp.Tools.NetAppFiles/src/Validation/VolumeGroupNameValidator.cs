// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.NetAppFiles.Validation;

internal static class VolumeGroupNameValidator
{
    public const string ErrorMessage = "--volume-group must be 1-64 characters, start with an alphanumeric character, and contain only alphanumerics, underscores, and hyphens.";

    public static bool IsValid(string? volumeGroup) => IsValidName(volumeGroup, 64);

    public static bool IsValidVolumeName(string? volume) => IsValidName(volume, 64);

    public static bool IsValidCreationToken(string? creationToken) =>
        creationToken is not null &&
        creationToken.Length is >= 1 and <= 80 &&
        char.IsAsciiLetter(creationToken[0]) &&
        creationToken.All(static character => char.IsAsciiLetterOrDigit(character) || character is '_' or '-');

    private static bool IsValidName(string? name, int maximumLength) =>
        name is not null &&
        name.Length >= 1 &&
        name.Length <= maximumLength &&
        char.IsAsciiLetterOrDigit(name[0]) &&
        name.All(static character => char.IsAsciiLetterOrDigit(character) || character is '_' or '-');
}
