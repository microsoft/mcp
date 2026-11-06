// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Core.Tests.Helpers;

public sealed class McpTestUtilitiesTests() : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"Azure MCP output {Guid.NewGuid():N}");

    [Theory]
    [InlineData("Debug", "", false)]
    [InlineData("Release", "win-x64", false)]
    [InlineData("Debug", "", true)]
    public void GetAzMcpExecutablePath_ResolvesRelativeBuildOutput(string configuration, string runtimeIdentifier, bool colocated)
    {
        string testOutput = Path.Combine(_directory, "tests", configuration);
        string serverOutput = colocated
            ? testOutput
            : Path.Combine(_directory, "server", configuration, "net10.0", runtimeIdentifier);
        Directory.CreateDirectory(testOutput);
        Directory.CreateDirectory(serverOutput);
        string executablePath = Path.Combine(serverOutput, OperatingSystem.IsWindows() ? "azmcp.exe" : "azmcp");
        File.WriteAllText(executablePath, string.Empty);
        File.WriteAllText(Path.Combine(testOutput, "AzureMcpServer.path"),
            Path.GetRelativePath(testOutput, serverOutput) + Environment.NewLine);

        Assert.Equal(executablePath, McpTestUtilities.GetAzMcpExecutablePath(testOutput));
    }

    [Fact]
    public void GetAzMcpExecutablePath_MissingLocation_ReportsBuildPrerequisite()
    {
        var exception = Assert.Throws<FileNotFoundException>(() => McpTestUtilities.GetAzMcpExecutablePath(_directory));

        Assert.Equal(Path.Combine(_directory, "AzureMcpServer.path"), exception.FileName);
        Assert.Contains("Build the consuming test project", exception.Message);
    }

    [Fact]
    public void GetAzMcpExecutablePath_EmptyLocation_ReportsInvalidFile()
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(Path.Combine(_directory, "AzureMcpServer.path"), Environment.NewLine);

        var exception = Assert.Throws<InvalidDataException>(() => McpTestUtilities.GetAzMcpExecutablePath(_directory));

        Assert.Contains("is empty", exception.Message);
    }

    [Fact]
    public void GetAzMcpExecutablePath_MissingExecutable_ReportsExpectedPath()
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(Path.Combine(_directory, "AzureMcpServer.path"), "missing server");

        var exception = Assert.Throws<FileNotFoundException>(() => McpTestUtilities.GetAzMcpExecutablePath(_directory));

        Assert.Equal(Path.Combine(_directory, "missing server", OperatingSystem.IsWindows() ? "azmcp.exe" : "azmcp"), exception.FileName);
        Assert.Contains("Build the consuming test project", exception.Message);
    }

    [Fact]
    public void GetAzMcpExecutablePath_UsesEvaluatedServerOutput()
    {
        string path = McpTestUtilities.GetAzMcpExecutablePath();
        string location = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "AzureMcpServer.path")).Trim();
        string expectedDirectory = Path.GetFullPath(location, AppContext.BaseDirectory);
        string executableName = OperatingSystem.IsWindows() ? "azmcp.exe" : "azmcp";

        Assert.True(File.Exists(path));
        Assert.Equal(Path.Combine(expectedDirectory, executableName), path);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }
}
