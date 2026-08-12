// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests;

public sealed class ResilienceManagementTestCleanupFixture() : IAsyncLifetime
{
    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        if (!LiveTestSettings.TryLoadTestSettings(out var settings) || settings.TestMode == TestMode.Playback)
        {
            return;
        }

        var cleanupScript = Path.Combine(settings.SettingsDirectory, "remove-test-resources-pre.ps1");
        if (!File.Exists(cleanupScript))
        {
            Console.Error.WriteLine($"WARNING: Resilience Management cleanup script was not found at '{cleanupScript}'.");
            return;
        }

        try
        {
            var startInfo = new ProcessStartInfo("pwsh")
            {
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            startInfo.ArgumentList.Add("-NoLogo");
            startInfo.ArgumentList.Add("-NoProfile");
            startInfo.ArgumentList.Add("-NonInteractive");
            startInfo.ArgumentList.Add("-File");
            startInfo.ArgumentList.Add(cleanupScript);
            startInfo.ArgumentList.Add("-ResourceGroupName");
            startInfo.ArgumentList.Add(settings.ResourceGroupName);
            startInfo.ArgumentList.Add("-TestSettingsPath");
            startInfo.ArgumentList.Add(Path.Combine(settings.SettingsDirectory, LiveTestSettings.TestSettingsFileName));

            using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start Resilience Management cleanup.");
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();
            var output = await outputTask;
            var error = await errorTask;

            if (!string.IsNullOrWhiteSpace(output))
            {
                Console.WriteLine(output);
            }

            if (process.ExitCode != 0)
            {
                Console.Error.WriteLine($"WARNING: Resilience Management cleanup exited with code {process.ExitCode}: {error}");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"WARNING: Resilience Management cleanup failed: {ex.Message}");
        }
    }
}
