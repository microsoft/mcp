// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Extension.Services;
using Microsoft.Mcp.Core.Services.ProcessExecution;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Extension.Tests;

public sealed class AzqrCliServiceTests
{
    [Fact]
    public void FindSupportedExecutablePath_EmptyPath_ReturnsNull()
    {
        var processService = Substitute.For<IExternalProcessService>();

        var result = AzqrCliService.FindSupportedExecutablePath(processService, string.Empty, isWindows: true);

        Assert.Null(result);
    }

    [Theory]
    [InlineData("azqr version 2.9.0", false)]
    [InlineData("azqr version 3.0.0", true)]
    [InlineData("azqr version 4.0.1", true)]
    public void FindSupportedExecutablePath_DiscoveredExecutable_RequiresSupportedVersion(
        string versionOutput,
        bool expectedSupported)
    {
        var processService = Substitute.For<IExternalProcessService>();
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var executablePath = Path.Combine(directory, "azqr.exe");
        Directory.CreateDirectory(directory);
        File.WriteAllText(executablePath, string.Empty);

        processService.ExecuteAsync(
            executablePath,
            "--version",
            Arg.Any<IDictionary<string, string>?>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns(new ProcessResult(0, versionOutput, string.Empty, $"{executablePath} --version"));

        try
        {
            var result = AzqrCliService.FindSupportedExecutablePath(processService, directory, isWindows: true);

            Assert.Equal(expectedSupported ? executablePath : null, result);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Theory]
    [InlineData("azqr version 2.9.0", false)]
    [InlineData("azqr version 3.0.0", true)]
    [InlineData("azqr version 3.2.0-preview.1", true)]
    [InlineData("azqr version 4.0.1", true)]
    [InlineData("unknown", false)]
    public void IsSupportedVersion_ReturnsExpectedResult(string output, bool expected)
    {
        Assert.Equal(expected, AzqrCliService.IsSupportedVersion(output));
    }
}