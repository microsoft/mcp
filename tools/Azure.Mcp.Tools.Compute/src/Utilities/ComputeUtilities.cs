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
            if (lowerImage.Contains("win") || lowerImage.Contains("windows"))
            {
                return "windows";
            }
        }

        return "linux";
    }
}
