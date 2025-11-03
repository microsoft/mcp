// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Xunit;

namespace Azure.Mcp.Server.UnitTests.Infrastructure;

public class CommandMetadataSyncTests
{
    private static readonly string _repoRoot = GetRepoRoot();

    [Fact]
    public void AzCommandsMetadata_Should_Be_Synchronized()
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

        // Act - Run the update script
        var updateResult = RunCommand(
            "pwsh",
            $"-NoProfile -ExecutionPolicy Bypass -File \"{updateScriptPath}\" -AzmcpPath \"{azmcpPath}\" -DocsPath \"{docsPath}\"",
            _repoRoot);

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

    private static (int ExitCode, string Output, string Error) RunCommand(string command, string arguments, string workingDirectory)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = processInfo };
        var outputBuilder = new System.Text.StringBuilder();
        var errorBuilder = new System.Text.StringBuilder();

        process.OutputDataReceived += (sender, args) =>
        {
            if (args.Data != null)
            {
                outputBuilder.AppendLine(args.Data);
            }
        };

        process.ErrorDataReceived += (sender, args) =>
        {
            if (args.Data != null)
            {
                errorBuilder.AppendLine(args.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        return (process.ExitCode, outputBuilder.ToString(), errorBuilder.ToString());
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
