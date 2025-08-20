// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Runtime.InteropServices;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Services.ProcessExecution;
using Azure.Mcp.Tools.Extension.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Extension.UnitTests;

[Trait("Area", "Extension")]
public sealed class AzdCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IExternalProcessService _processService;
    private readonly ILogger<AzdCommand> _logger;

    private const int TempDirSuffixLength = 8;

    public AzdCommandTests()
    {
        _processService = Substitute.For<IExternalProcessService>();
        _logger = Substitute.For<ILogger<AzdCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_processService);
        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSuccessResult_WhenCommandExecutesSuccessfully()
    {
        // Arrange
        var command = new AzdCommand(_logger);
        var parser = new Parser(command.GetCommand());
        var args = parser.Parse("--command \"env list\" --cwd test-dir");
        var context = new CommandContext(_serviceProvider);

        var expectedOutput = "env1\nenv2";

        _processService.ExecuteAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int>())
            .Returns(new ProcessResult(0, expectedOutput, string.Empty, "azd env list --cwd test-dir --no-prompt"));

        var tempDir = CreateTempAzdCliDirectory();

        try
        {
            var azdPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Path.Combine(tempDir, "azd.exe")
                : Path.Combine(tempDir, "azd");

            File.WriteAllText(azdPath, "mock azd executable");

            var originalPath = Environment.GetEnvironmentVariable("PATH");
            try
            {
                Environment.SetEnvironmentVariable("PATH", tempDir + Path.PathSeparator + originalPath);
                AzdCommand.ClearCachedAzdPath();

                // Act
                var response = await command.ExecuteAsync(context, args);

                // Assert
                Assert.NotNull(response);
                Assert.Equal(200, response.Status);
                Assert.NotNull(response.Results);

                await _processService.Received(1).ExecuteAsync(
                    Arg.Any<string>(),
                    Arg.Is<string>(cmd => cmd.Contains("env list") && cmd.Contains("--cwd test-dir") && cmd.Contains("--no-prompt")),
                    Arg.Any<int>());
            }
            finally
            {
                CleanupPath(originalPath);
            }
        }
        finally
        {
            CleanupTempDirectory(tempDir);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsErrorResponse_WhenCommandFails()
    {
        // Arrange
        var command = new AzdCommand(_logger);
        var parser = new Parser(command.GetCommand());
        var args = parser.Parse("--command \"env invalid-command\" --cwd test-dir");
        var context = new CommandContext(_serviceProvider);

        var errorMessage = "Error: azd env: 'invalid-command' is not an azd command.";

        _processService.ExecuteAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int>())
            .Returns(new ProcessResult(1, string.Empty, errorMessage, "azd env invalid-command --cwd test-dir --no-prompt"));

        var tempDir = CreateTempAzdCliDirectory();

        try
        {
            var azdPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Path.Combine(tempDir, "azd.exe")
                : Path.Combine(tempDir, "azd");

            File.WriteAllText(azdPath, "mock azd executable");

            var originalPath = Environment.GetEnvironmentVariable("PATH");
            try
            {
                Environment.SetEnvironmentVariable("PATH", tempDir + Path.PathSeparator + originalPath);
                AzdCommand.ClearCachedAzdPath();

                // Act
                var response = await command.ExecuteAsync(context, args);

                // Assert
                Assert.NotNull(response);
                Assert.Equal(500, response.Status);
                Assert.Equal(errorMessage, response.Message);
            }
            finally
            {
                CleanupPath(originalPath);
            }
        }
        finally
        {
            CleanupTempDirectory(tempDir);
        }
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException_AndSetsException()
    {
        // Arrange
        var command = new AzdCommand(_logger);
        var parser = new Parser(command.GetCommand());
        var args = parser.Parse("--command \"env list\" --cwd test-dir");
        var context = new CommandContext(_serviceProvider);

        var exceptionMessage = "Azure Developer CLI executable not found";

        _processService.ExecuteAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int>())
            .ThrowsAsync(new FileNotFoundException(exceptionMessage));

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.Contains(exceptionMessage, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBadRequest_WhenMissingRequiredOptions()
    {
        // Arrange
        var command = new AzdCommand(_logger);
        var parser = new Parser(command.GetCommand());
        var args = parser.Parse("--command \"env list\"");
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(400, response.Status);
    }

    [Theory]
    [InlineData("up")]
    [InlineData("down")]
    [InlineData("deploy")]
    [InlineData("package")]
    [InlineData("provision")]
    public async Task ExecuteAsync_WithLongRunningCommands_ReturnsError(string longRunningCommand)
    {
        // Arrange
        var command = new AzdCommand(_logger);
        var parser = new Parser(command.GetCommand());
        var args = parser.Parse($"--command {longRunningCommand} --cwd test-dir");
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("long-running command", response.Message);
        Assert.Contains($"azd {longRunningCommand}", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithLearnOptionAndNoCommand_ReturnsBestPractices()
    {
        // Arrange
        var command = new AzdCommand(_logger);
        var parser = new Parser(command.GetCommand());
        var args = parser.Parse("--learn --cwd test-dir");
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Message);
        Assert.Contains("best practices", response.Message.ToLowerInvariant());
    }

    [Fact]
    public async Task ExecuteAsync_WithEnvironmentOption_IncludesEnvironmentInCommand()
    {
        // Arrange
        var command = new AzdCommand(_logger);
        var parser = new Parser(command.GetCommand());
        var args = parser.Parse("--command \"env list\" --cwd test-dir --environment dev");
        var context = new CommandContext(_serviceProvider);

        _processService.ExecuteAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int>())
            .Returns(new ProcessResult(0, "output", "", "command"));

        var tempDir = CreateTempAzdCliDirectory();

        try
        {
            var azdPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Path.Combine(tempDir, "azd.exe")
                : Path.Combine(tempDir, "azd");

            File.WriteAllText(azdPath, "mock azd executable");

            var originalPath = Environment.GetEnvironmentVariable("PATH");
            try
            {
                Environment.SetEnvironmentVariable("PATH", tempDir + Path.PathSeparator + originalPath);
                AzdCommand.ClearCachedAzdPath();

                // Act
                var response = await command.ExecuteAsync(context, args);

                // Assert
                Assert.Equal(200, response.Status);
                await _processService.Received(1).ExecuteAsync(
                    Arg.Any<string>(),
                    Arg.Is<string>(cmd => cmd.Contains("-e dev")),
                    Arg.Any<int>());
            }
            finally
            {
                CleanupPath(originalPath);
            }
        }
        finally
        {
            CleanupTempDirectory(tempDir);
        }
    }

    [Fact]
    public void FindAzdCliPath_WithTemporaryTestEnvironment_FindsExecutable()
    {
        AzdCommand.ClearCachedAzdPath();

        var tempDir = CreateTempAzdCliDirectory();

        try
        {
            var azdPath = Path.Combine(tempDir, "azd");
            var azdExePath = Path.Combine(tempDir, "azd.exe");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                File.WriteAllText(azdExePath, "@echo off\necho Azure Developer CLI");
            }
            else
            {
                File.WriteAllText(azdPath, "#!/bin/bash\necho 'Azure Developer CLI'");
            }

            var originalPath = Environment.GetEnvironmentVariable("PATH");

            try
            {
                Environment.SetEnvironmentVariable("PATH", tempDir + Path.PathSeparator + originalPath);

                AzdCommand.ClearCachedAzdPath();

                var result = AzdCommand.FindAzdCliPath();

                Assert.Equal(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? azdExePath : azdPath, result);
            }
            finally
            {
                CleanupPath(originalPath);
            }
        }
        finally
        {
            CleanupTempDirectory(tempDir);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithComplexCommand_HandlesArgumentsProperly()
    {
        // Arrange
        var command = new AzdCommand(_logger);
        var parser = new Parser(command.GetCommand());
        var args = parser.Parse("--command \"env get-values --output json\" --cwd test-dir");
        var context = new CommandContext(_serviceProvider);

        var expectedOutput = """{"AZURE_LOCATION":"eastus","AZURE_SUBSCRIPTION_ID":"12345"}""";

        _processService.ExecuteAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int>())
            .Returns(new ProcessResult(0, expectedOutput, string.Empty, "azd env get-values --output json --cwd test-dir --no-prompt"));

        var tempDir = CreateTempAzdCliDirectory();

        try
        {
            var azdPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Path.Combine(tempDir, "azd.exe")
                : Path.Combine(tempDir, "azd");

            File.WriteAllText(azdPath, "mock azd executable");

            var originalPath = Environment.GetEnvironmentVariable("PATH");
            try
            {
                Environment.SetEnvironmentVariable("PATH", tempDir + Path.PathSeparator + originalPath);
                AzdCommand.ClearCachedAzdPath();

                // Act
                var response = await command.ExecuteAsync(context, args);

                // Assert
                Assert.NotNull(response);
                Assert.Equal(200, response.Status);
                Assert.NotNull(response.Results);

                await _processService.Received(1).ExecuteAsync(
                    Arg.Any<string>(),
                    Arg.Is<string>(cmd => cmd.Contains("env get-values") && cmd.Contains("--output json") && cmd.Contains("--no-prompt")),
                    Arg.Any<int>());
            }
            finally
            {
                CleanupPath(originalPath);
            }
        }
        finally
        {
            CleanupTempDirectory(tempDir);
        }
    }

    [Theory]
    [InlineData("help")]
    [InlineData("version")]
    [InlineData("config show")]
    [InlineData("template list")]
    public async Task ExecuteAsync_WithInfoCommands_ExecutesSuccessfully(string infoCommand)
    {
        // Arrange
        var command = new AzdCommand(_logger);
        var parser = new Parser(command.GetCommand());
        var args = parser.Parse($"--command \"{infoCommand}\" --cwd test-dir");
        var context = new CommandContext(_serviceProvider);

        var expectedOutput = $"Mock output for {infoCommand}";

        _processService.ExecuteAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int>())
            .Returns(new ProcessResult(0, expectedOutput, string.Empty, $"azd {infoCommand} --cwd test-dir --no-prompt"));

        var tempDir = CreateTempAzdCliDirectory();

        try
        {
            var azdPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Path.Combine(tempDir, "azd.exe")
                : Path.Combine(tempDir, "azd");

            File.WriteAllText(azdPath, "mock azd executable");

            var originalPath = Environment.GetEnvironmentVariable("PATH");
            try
            {
                Environment.SetEnvironmentVariable("PATH", tempDir + Path.PathSeparator + originalPath);
                AzdCommand.ClearCachedAzdPath();

                // Act
                var response = await command.ExecuteAsync(context, args);

                // Assert
                Assert.Equal(200, response.Status);
                Assert.NotNull(response.Results);
            }
            finally
            {
                CleanupPath(originalPath);
            }
        }
        finally
        {
            CleanupTempDirectory(tempDir);
        }
    }

    private static string CreateTempAzdCliDirectory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "AzdTest_" + Guid.NewGuid().ToString("N")[..TempDirSuffixLength]);
        Directory.CreateDirectory(tempDir);
        return tempDir;
    }

    private static void CleanupTempDirectory(string tempDir)
    {
        if (Directory.Exists(tempDir))
        {
            Directory.Delete(tempDir, true);
        }
    }

    private static void CleanupPath(string? originalPath)
    {
        Environment.SetEnvironmentVariable("PATH", originalPath);
        AzdCommand.ClearCachedAzdPath();
    }
}
