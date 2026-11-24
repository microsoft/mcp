// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;
using Microsoft.Mcp.Core.Services.Host;
using Microsoft.Mcp.Core.Services.ProcessExecution;

namespace Azure.Mcp.Tools.Extension.Services;

internal sealed partial class AzqrCliService(IExternalProcessService processService, IFileSystem fileSystem, IEnvironmentVariables environmentVariables) : IAzqrCliService
{
    private const int VersionCheckTimeoutSeconds = 5;
    private static readonly Version s_minimumSupportedVersion = new(3, 0, 0);
    private readonly Lazy<string?> _supportedExecutablePath = new(() => FindSupportedExecutablePath(
        processService,
        environmentVariables.GetEnvironmentVariable("PATH"),
        OperatingSystem.IsWindows(),
        fileSystem));

    public string? GetSupportedExecutablePath()
    {
        fileSystem.EnsureAccess();
        return _supportedExecutablePath.Value;
    }

    internal static string? FindSupportedExecutablePath(
        IExternalProcessService processService,
        string? pathEnvironment,
        bool isWindows,
        IFileSystem fileSystem)
    {
        var executablePath = FindExecutablePath(pathEnvironment, isWindows, fileSystem);
        if (executablePath is null)
        {
            return null;
        }

        try
        {
            var result = processService.ExecuteAsync(
                executablePath,
                "--version",
                operationTimeoutSeconds: VersionCheckTimeoutSeconds).GetAwaiter().GetResult();

            return result.ExitCode == 0 && IsSupportedVersion($"{result.Output}\n{result.Error}")
                ? executablePath
                : null;
        }
        catch
        {
            return null;
        }
    }

    internal static string? FindExecutablePath(string? pathEnvironment, bool isWindows, IFileSystem fileSystem)
    {
        if (string.IsNullOrWhiteSpace(pathEnvironment))
        {
            return null;
        }

        var executableName = isWindows ? "azqr.exe" : "azqr";
        foreach (var directory in pathEnvironment.Split(
            Path.PathSeparator,
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var executablePath = Path.Combine(directory.Trim('"'), executableName);
            if (fileSystem.FileExists(executablePath))
            {
                return executablePath;
            }
        }

        return null;
    }

    internal static bool IsSupportedVersion(string output)
    {
        var match = VersionRegex().Match(output);
        return match.Success
            && Version.TryParse(match.Value, out var installedVersion)
            && installedVersion >= s_minimumSupportedVersion;
    }

    [GeneratedRegex(@"(?<!\d)\d+\.\d+\.\d+(?!\d)", RegexOptions.CultureInvariant)]
    private static partial Regex VersionRegex();
}
