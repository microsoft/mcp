// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Azure.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Core.LiveTests;

public class NpxTests : IClassFixture<LiveTestSettingsFixture>
{
    private readonly LiveTestSettings _settings;

    public NpxTests(LiveTestSettingsFixture fixture)
    {
        _settings = fixture.Settings;

        if (string.IsNullOrEmpty(_settings.TestPackage))
        {
            Assert.Skip("Can only test packages ");
        }
    }

    [Fact]
    public async Task Help_command_should_return_help()
    {
        var outputLines = await RunCommand("--help");
        var concatenatedOutput = string.Join(Environment.NewLine, outputLines.Output);
        Assert.NotEmpty(concatenatedOutput);
        Assert.Contains("azmcp [command] [options]", concatenatedOutput);
    }

    private async Task<(string[] Output, string[] Error, int ExitCode)> RunCommand(params string[] arguments)
    {
        var shell = OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/sh";
        var shellArgument = OperatingSystem.IsWindows() ? "/c" : "-c";

        // Construct the npx command
        var npxCommand = $"npx -y {_settings.TestPackage} {string.Join(" ", arguments)}";

        var processStartInfo = new ProcessStartInfo
        {
            FileName = shell,
            ArgumentList = { shellArgument, npxCommand },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process
        {
            StartInfo = processStartInfo
        };

        process.Start();

        var output = new List<string>();
        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                output.Add(e.Data);
            }
        };
        process.BeginOutputReadLine();

        var error = new List<string>();
        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                error.Add(e.Data);
            }
        };
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();
        return (output.ToArray(), error.ToArray(), process.ExitCode);
    }
}
