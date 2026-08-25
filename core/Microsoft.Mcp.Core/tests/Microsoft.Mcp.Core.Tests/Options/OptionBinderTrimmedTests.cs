// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Options;

public sealed class OptionBinderTrimmedTests
{
    [Fact]
    public async Task TrimmedPublish_PreservesNestedOptionContainerProperties()
    {
        var sourcePath = Assembly.GetExecutingAssembly()
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(attribute => attribute.Key == "SourcePath")
            .Value!;
        var projectPath = Path.GetFullPath(
            Path.Combine(sourcePath, "..", "TrimmedOptionContainerApp", "TrimmedOptionContainerApp.csproj"));
        var publishPath = Path.Combine(Path.GetTempPath(), nameof(OptionBinderTrimmedTests), Guid.NewGuid().ToString("N"));

        try
        {
            var publishResult = await RunProcessAsync(
                "dotnet",
                [
                    "publish",
                    projectPath,
                    "--configuration", "Release",
                    "--runtime", RuntimeInformation.RuntimeIdentifier,
                    "--self-contained", "true",
                    "--output", publishPath,
                    "/p:PublishTrimmed=true"
                ]);

            Assert.True(
                publishResult.ExitCode == 0,
                $"Trimmed publish failed.{Environment.NewLine}{publishResult.StandardOutput}{Environment.NewLine}{publishResult.StandardError}");

            var executableName = OperatingSystem.IsWindows()
                ? "TrimmedOptionContainerApp.exe"
                : "TrimmedOptionContainerApp";
            var executablePath = Path.Combine(publishPath, executableName);
            var executionResult = await RunProcessAsync(executablePath, []);

            Assert.True(
                executionResult.ExitCode == 0,
                $"Trimmed executable did not discover and bind nested retry options.{Environment.NewLine}" +
                $"{executionResult.StandardOutput}{Environment.NewLine}{executionResult.StandardError}");
        }
        finally
        {
            if (Directory.Exists(publishPath))
            {
                Directory.Delete(publishPath, recursive: true);
            }
        }
    }

    private static async Task<(int ExitCode, string StandardOutput, string StandardError)> RunProcessAsync(
        string fileName,
        string[] arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Failed to start '{fileName}'.");
        var standardOutput = process.StandardOutput.ReadToEndAsync();
        var standardError = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync(TestContext.Current.CancellationToken);
        return (process.ExitCode, await standardOutput, await standardError);
    }
}
