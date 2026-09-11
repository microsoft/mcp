// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Xunit;

namespace Azure.Mcp.Server.Tests.Infrastructure;

public class CliOutputTests
{
    private static string AzmcpPath
    {
        get
        {
            var executableName = OperatingSystem.IsWindows() ? "azmcp.exe" : "azmcp";
            return Path.Combine(AppContext.BaseDirectory, executableName);
        }
    }

    [Fact]
    public async Task BareInvocation_WritesDiagnosticsAndHelpToStandardError()
    {
        var result = await RunAzmcpAsync();

        Assert.NotEqual(0, result.ExitCode);
        Assert.Empty(result.StandardOutput);
        Assert.Contains("azmcp [command] [options]", result.StandardError);

        var helpStart = result.StandardError.IndexOf("Azure.Mcp.Server", StringComparison.Ordinal);
        Assert.True(helpStart >= 0, "Expected the root help in standard error.");
        Assert.False(
            string.IsNullOrWhiteSpace(result.StandardError[..helpStart]),
            "Expected a localized parse diagnostic before the root help.");
    }

    [Fact]
    public async Task ExplicitHelp_WritesHelpToStandardOutput()
    {
        var result = await RunAzmcpAsync("--help");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Azure.Mcp.Server", result.StandardOutput);
        Assert.Contains("azmcp [command] [options]", result.StandardOutput);
        Assert.Empty(result.StandardError);
    }

    private static async Task<CliResult> RunAzmcpAsync(params string[] arguments)
    {
        Assert.True(File.Exists(AzmcpPath), $"Executable not found at {AzmcpPath}.");

        var startInfo = new ProcessStartInfo
        {
            FileName = AzmcpPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = new Process { StartInfo = startInfo };
        Assert.True(process.Start());

        try
        {
            var standardOutput = process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
            var standardError = process.StandardError.ReadToEndAsync(TestContext.Current.CancellationToken);

            await process.WaitForExitAsync(TestContext.Current.CancellationToken);

            return new CliResult(process.ExitCode, await standardOutput, await standardError);
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
    }

    private sealed record CliResult(int ExitCode, string StandardOutput, string StandardError);
}
