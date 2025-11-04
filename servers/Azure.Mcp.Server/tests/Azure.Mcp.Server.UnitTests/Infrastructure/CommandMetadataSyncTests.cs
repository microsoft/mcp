// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.ProcessExecution;
using Xunit;

namespace Azure.Mcp.Server.UnitTests.Infrastructure;

public class CommandMetadataSyncTests
{
    private static readonly string _repoRoot = GetRepoRoot();

    [Fact]
    public async Task AzCommandsMetadata_Should_Be_Synchronized()
    {
        // Arrange
        var docsPath = Path.Combine(_repoRoot, "servers", "Azure.Mcp.Server", "docs", "azmcp-commands.md");
        var updateScriptPath = Path.Combine(_repoRoot, "eng", "scripts", "Update-AzCommandsMetadata.ps1");

        // Verify files exist
        Assert.True(File.Exists(docsPath), $"Documentation file not found at {docsPath}");
        Assert.True(File.Exists(updateScriptPath), $"Update script not found at {updateScriptPath}");

        // Determine the executable path (OS-specific) - assumes build has already happened
        var exeName = OperatingSystem.IsWindows() ? "azmcp.exe" : "azmcp";
        var azmcpPath = Path.Combine(_repoRoot, "servers", "Azure.Mcp.Server", "src", "bin", "Debug", "net9.0", exeName);

        Assert.True(File.Exists(azmcpPath), $"Executable not found at {azmcpPath}. Please build the Azure.Mcp.Server project first.");

        // Get the original content before running the update script
        var originalContent = File.ReadAllText(docsPath);

        // Act - Run the update script using ExternalProcessService
        var processService = new ExternalProcessService();
        var pwshPath = FindPowerShellExecutable();
        var arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{updateScriptPath}\" -AzmcpPath \"{azmcpPath}\" -DocsPath \"{docsPath}\"";

        var updateResult = await processService.ExecuteAsync(pwshPath, arguments);

        Assert.True(updateResult.ExitCode == 0,
            $"Update script failed with exit code {updateResult.ExitCode}. Output: {updateResult.Output}. Error: {updateResult.Error}");

        // Assert - Check if the file was modified
        var updatedContent = File.ReadAllText(docsPath);

        Assert.True(originalContent == updatedContent,
            "The azmcp-commands.md file is out of sync with tool metadata.\n\n" +
            "To fix this issue:\n" +
            "  1. Run the following command from the repository root:\n" +
            $"     .\\eng\\scripts\\Update-AzCommandsMetadata.ps1\n" +
            "  2. Review the changes to servers\\Azure.Mcp.Server\\docs\\azmcp-commands.md\n" +
            "  3. Commit the updated file with your changes\n\n" +
            "This ensures that tool metadata in the documentation stays synchronized with the actual command implementations.");
    }

    private static string FindPowerShellExecutable()
    {
        // Try common PowerShell paths
        var commonPaths = OperatingSystem.IsWindows()
            ? new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "PowerShell", "7", "pwsh.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "PowerShell", "pwsh.exe"),
                "C:\\Program Files\\PowerShell\\7\\pwsh.exe",
                "C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe"
            }
            : new[] { "/usr/local/bin/pwsh", "/usr/bin/pwsh" };

        foreach (var path in commonPaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        // Search in PATH environment variable
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (pathEnv != null)
        {
            var exeName = OperatingSystem.IsWindows() ? "pwsh.exe" : "pwsh";
            foreach (var directory in pathEnv.Split(Path.PathSeparator))
            {
                var fullPath = Path.Combine(directory, exeName);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            // Also try powershell.exe on Windows as fallback
            if (OperatingSystem.IsWindows())
            {
                foreach (var directory in pathEnv.Split(Path.PathSeparator))
                {
                    var fullPath = Path.Combine(directory, "powershell.exe");
                    if (File.Exists(fullPath))
                    {
                        return fullPath;
                    }
                }
            }
        }

        throw new FileNotFoundException("PowerShell executable not found. Please ensure PowerShell 7+ (pwsh) or Windows PowerShell is installed.");
    }

    private static string GetRepoRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var dir = new DirectoryInfo(currentDir);

        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "global.json")) &&
                Directory.Exists(Path.Combine(dir.FullName, "servers")))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not find repository root containing global.json and servers directory");
    }
}
