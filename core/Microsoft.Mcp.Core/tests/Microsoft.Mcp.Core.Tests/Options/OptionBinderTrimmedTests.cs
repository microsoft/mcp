// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Options;

public sealed class OptionBinderTrimmedTests
{
    // Restoring a runtime pack and running the trimmer is slow, but it must stay bounded so a stalled
    // restore fails this test instead of hanging the whole unit test run.
    private static readonly TimeSpan PublishTimeout = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan ExecutionTimeout = TimeSpan.FromMinutes(2);

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
                ],
                PublishTimeout);

            Assert.True(
                publishResult.ExitCode == 0,
                $"Trimmed publish failed.{Environment.NewLine}{publishResult.StandardOutput}{Environment.NewLine}{publishResult.StandardError}");

            var executableName = OperatingSystem.IsWindows()
                ? "TrimmedOptionContainerApp.exe"
                : "TrimmedOptionContainerApp";
            var executablePath = Path.Combine(publishPath, executableName);
            var executionResult = await RunProcessAsync(executablePath, [], ExecutionTimeout);

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
        string[] arguments,
        TimeSpan timeout)
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

        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeoutSource.CancelAfter(timeout);

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Failed to start '{fileName}'.");
        var standardOutput = process.StandardOutput.ReadToEndAsync(timeoutSource.Token);
        var standardError = process.StandardError.ReadToEndAsync(timeoutSource.Token);

        try
        {
            await process.WaitForExitAsync(timeoutSource.Token);
        }
        catch (OperationCanceledException)
        {
            // Dispose only releases the wrapper, so the spawned tree has to be terminated explicitly.
            KillProcessTree(process);

            TestContext.Current.CancellationToken.ThrowIfCancellationRequested();
            throw new TimeoutException($"'{fileName}' did not exit within {timeout}.");
        }

        return (process.ExitCode, await standardOutput, await standardError);
    }

    private static void KillProcessTree(Process process)
    {
        try
        {
            process.Kill(entireProcessTree: true);
        }
        catch (InvalidOperationException)
        {
            // The process exited between the cancellation and this call.
        }
    }
}
