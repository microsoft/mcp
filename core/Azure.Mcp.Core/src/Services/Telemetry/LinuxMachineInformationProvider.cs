// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Core.Services.Telemetry;

[SupportedOSPlatform("linux")]

internal class LinuxMachineInformationProvider(ILogger<LinuxMachineInformationProvider> logger) : UnixMachineInformationProvider(logger)
{
    private const string PrimaryPathEnvVar = "XDG_CACHE_HOME";
    private const string SecondaryPathSubDirectory = ".cache";

    /// <summary>
    /// Gets the base folder for the cache to be stored.
    /// The final path should be $HOME\.cache\Microsoft\DeveloperTools.
    /// </summary>
    public override string GetStoragePath()
    {
        var userDir = Environment.GetEnvironmentVariable(PrimaryPathEnvVar);

        // If this comes back as null or empty/whitespace, try environment variable.
        if (string.IsNullOrWhiteSpace(userDir))
        {
            var rootPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            // If the secondary path is still null/empty/whitespace, then throw as it will lead
            // to us caching the data in the wrong directory otherwise.
            if (string.IsNullOrWhiteSpace(rootPath))
            {
                throw new InvalidOperationException("linux: Unable to get UserProfile or $HOME folder.");
            }

            userDir = Path.Combine(rootPath, SecondaryPathSubDirectory);
        }

        return userDir;
    }
}
