// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Xunit;

namespace Azure.Mcp.Server.Tests.Infrastructure;

/// <summary>
/// Verifies that every supported server mode can service a stateless tools/list request
/// without an initialize handshake. This is the Phase 1 mode-coverage gate for the
/// 2026-07-28 stateless protocol migration (Workstream I item 5).
/// </summary>
public class ServerModeCoverageTests
{
    private static string AzmcpPath
    {
        get
        {
            var exeName = OperatingSystem.IsWindows() ? "azmcp.exe" : "azmcp";
            return Path.Combine(AppContext.BaseDirectory, exeName);
        }
    }

    [Fact]
    public async Task SingleMode_Should_List_Tools_Without_Initialize()
    {
        // "single" mode exposes exactly one proxy tool named "azure".
        Assert.True(File.Exists(AzmcpPath), $"Executable not found at {AzmcpPath}. Please build the Azure.Mcp.Server project first.");

        var processStartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = AzmcpPath,
            Arguments = "server start --mode single",
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = System.Diagnostics.Process.Start(processStartInfo);
        Assert.NotNull(process);

        try
        {
            await Task.Delay(500, TestContext.Current.CancellationToken);

            var listToolsRequest = """
                {"jsonrpc":"2.0","id":1,"method":"tools/list","params":{}}
                """;
            await process.StandardInput.WriteLineAsync(listToolsRequest);
            await process.StandardInput.FlushAsync(TestContext.Current.CancellationToken);

            var response = await ReadResponseAsync(process.StandardOutput);

            Assert.NotNull(response);
            Assert.Contains("\"result\"", response, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("\"tools\"", response, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill();
            }
        }
    }

    [Fact]
    public async Task NamespaceMode_Should_List_Tools_Without_Initialize()
    {
        // "namespace" mode collapses all tools within each service namespace into a single namespace-level tool.
        Assert.True(File.Exists(AzmcpPath), $"Executable not found at {AzmcpPath}. Please build the Azure.Mcp.Server project first.");

        var processStartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = AzmcpPath,
            Arguments = "server start --mode namespace",
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = System.Diagnostics.Process.Start(processStartInfo);
        Assert.NotNull(process);

        try
        {
            await Task.Delay(500, TestContext.Current.CancellationToken);

            var listToolsRequest = """
                {"jsonrpc":"2.0","id":1,"method":"tools/list","params":{}}
                """;
            await process.StandardInput.WriteLineAsync(listToolsRequest);
            await process.StandardInput.FlushAsync(TestContext.Current.CancellationToken);

            var response = await ReadResponseAsync(process.StandardOutput);

            Assert.NotNull(response);
            Assert.Contains("\"result\"", response, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("\"tools\"", response, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill();
            }
        }
    }

    [Fact]
    public async Task AllMode_Should_List_Tools_Without_Initialize()
    {
        // "all" mode exposes every individual Azure tool directly.
        // Uses a longer timeout because registering all tools takes more time on slow CI runners.
        Assert.True(File.Exists(AzmcpPath), $"Executable not found at {AzmcpPath}. Please build the Azure.Mcp.Server project first.");

        var processStartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = AzmcpPath,
            Arguments = "server start --mode all",
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = System.Diagnostics.Process.Start(processStartInfo);
        Assert.NotNull(process);

        try
        {
            await Task.Delay(500, TestContext.Current.CancellationToken);

            var listToolsRequest = """
                {"jsonrpc":"2.0","id":1,"method":"tools/list","params":{}}
                """;
            await process.StandardInput.WriteLineAsync(listToolsRequest);
            await process.StandardInput.FlushAsync(TestContext.Current.CancellationToken);

            // Allow up to 60 s: "all" mode registers every tool and is slower on macOS CI runners.
            var response = await ReadResponseAsync(process.StandardOutput, TimeSpan.FromSeconds(60));

            Assert.NotNull(response);
            Assert.Contains("\"result\"", response, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("\"tools\"", response, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill();
            }
        }
    }

    private static async Task<string?> ReadResponseAsync(StreamReader reader, TimeSpan? timeout = null)
    {
        using var cts = new CancellationTokenSource(timeout ?? TimeSpan.FromSeconds(15));
        try
        {
            return await reader.ReadLineAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }
}
