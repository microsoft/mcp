// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Utilities;

internal static class ComputeUtilities
{
    /// <summary>
    /// Determines the OS type based on the provided osType parameter or image name.
    /// If osType is explicitly provided, it is used. Otherwise, the image name is analyzed
    /// to detect Windows images. Defaults to Linux if no Windows indicators are found.
    /// </summary>
    /// <param name="osType">Explicit OS type (e.g., "windows", "linux").</param>
    /// <param name="image">Image name or alias to analyze.</param>
    /// <returns>The detected OS type, either "windows" or "linux".</returns>
    public static string DetermineOsType(string? osType, string? image)
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
}
