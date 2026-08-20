// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Utilities;

internal static class ComputeUtilities
{
    internal static readonly string[] s_validSshKeyPrefixes =
[
    "ssh-rsa ",
        "ssh-ed25519 ",
        "ssh-dss ",
        "ecdsa-sha2-nistp256 ",
        "ecdsa-sha2-nistp384 ",
        "ecdsa-sha2-nistp521 ",
        "sk-ssh-ed25519@openssh.com ",
        "sk-ecdsa-sha2-nistp256@openssh.com ",
    ];

    /// <summary>
    /// Determines the OS type based on the provided osType parameter or image name.
    /// If osType is explicitly provided, it is used. Otherwise, the image name is analyzed
    /// to detect Windows images. Defaults to Linux if no Windows indicators are found.
    /// </summary>
    /// <param name="osType">Explicit OS type (e.g., "windows", "linux").</param>
    /// <param name="image">Image name or alias to analyze.</param>
    /// <returns>The detected OS type, either "windows" or "linux".</returns>
    internal static string DetermineOsType(string? osType, string? image)
    {
        if (!string.IsNullOrEmpty(osType))
        {
            return osType;
        }

        if (!string.IsNullOrEmpty(image))
        {
            var lowerImage = image.ToLowerInvariant();
            // StartsWith("win"): alias-style names like "Win2022Datacenter", "Win11Pro"
            // Contains("windows"): URN offer/publisher names like "MicrosoftWindowsServer:WindowsServer2022:..."
            // Token StartsWith("win"): SKU components like "vs-2022-comm-latest-win11-n-gen2"
            //   (split on URN/name separators so "twin-ubuntu" token "twin" does NOT match)
            if (lowerImage.StartsWith("win") ||
                lowerImage.Contains("windows") ||
                lowerImage.Split(':', '-', '_', ' ').Any(t => t.StartsWith("win")))
            {
                return "windows";
            }
        }

        return "linux";
    }

    internal static bool ValidateHttpModeSshPublicKey(string sshPublicKey, out string errorMessage)
    {
        errorMessage = string.Empty;
        if (!IsValidSshPublicKeyContent(sshPublicKey))
        {
            errorMessage = LooksLikeFilePath(sshPublicKey)
                ? "The provided SSH public key appears to be a file path. " +
                  "In remote HTTP mode, file paths cannot be resolved on the server. " +
                  "Please provide the SSH public key content directly (e.g., 'ssh-rsa AAAA...', 'ssh-ed25519 AAAA...')."
                : "The provided SSH public key does not appear to be valid key content. " +
                  "Please provide the SSH public key content directly (e.g., 'ssh-rsa AAAA...', 'ssh-ed25519 AAAA...').";
            return false;
        }

        return true;
    }

    internal static string ResolveSshPublicKey(string sshPublicKey, bool isHttpMode)
    {
        if (!isHttpMode)
        {
            // In stdio mode, allow resolving file paths for convenience
            if (File.Exists(sshPublicKey))
            {
                return File.ReadAllText(sshPublicKey).Trim();
            }
        }
        else
        {
            // In HTTP mode, file path resolution is not allowed for security
            if (!ValidateHttpModeSshPublicKey(sshPublicKey, out var errorMessage))
            {
                throw new ArgumentException(errorMessage);
            }
        }

        return sshPublicKey.Trim();
    }

    internal static bool LooksLikeFilePath(string value)
    {
        var trimmed = value.Trim();
        return trimmed.Contains('/') || trimmed.Contains('\\') || trimmed.EndsWith(".pub", StringComparison.OrdinalIgnoreCase);
    }

    internal static bool IsValidSshPublicKeyContent(string value)
    {
        var trimmed = value.Trim();
        return Array.Exists(s_validSshKeyPrefixes, prefix => trimmed.StartsWith(prefix, StringComparison.Ordinal));
    }
}
