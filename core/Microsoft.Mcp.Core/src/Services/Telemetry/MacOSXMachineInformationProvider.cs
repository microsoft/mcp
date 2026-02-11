// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Core.Services.Telemetry;

[SupportedOSPlatform("osx")]
internal class MacOSXMachineInformationProvider(ILogger<MacOSXMachineInformationProvider> logger)
    : UnixMachineInformationProvider(logger)
{
    /// <summary>
    /// Retrieves the storage path for application data on macOS.
    /// </summary>
    /// <remarks>This method determines the appropriate storage directory by first attempting to retrieve the
    /// user's profile directory using <see cref="Environment.SpecialFolder.UserProfile"/>. If that is unavailable, it
    /// falls back to the "HOME"  environment variable. If neither is available, an <see
    /// cref="InvalidOperationException"/> is thrown.  The returned path is typically located under the
    /// "Library/Application Support" directory within the user's home folder.</remarks>
    /// <returns>A string representing the full path to the "Library/Application Support" directory in the user's home folder.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if the user's profile directory and the "HOME"
    /// environment variable are both unavailable or invalid.</exception>
    public override string GetStoragePath()
    {
        var userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        // If this comes back as null or empty/whitespace, try environment variable.
        if (string.IsNullOrWhiteSpace(userDir))
        {
            userDir = Environment.GetEnvironmentVariable("HOME");
        }

        if (string.IsNullOrWhiteSpace(userDir))
        {
            throw new InvalidOperationException("macOS: Unable to get UserProfile or $HOME folder.");
        }

        var subdirectoryPath = Path.Combine(userDir, "Library", "Application Support");
        return subdirectoryPath;
    }
}
