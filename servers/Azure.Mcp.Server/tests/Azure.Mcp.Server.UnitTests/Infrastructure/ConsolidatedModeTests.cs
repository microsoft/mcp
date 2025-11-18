// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Xunit;

namespace Azure.Mcp.Server.UnitTests.Infrastructure;

public class ConsolidatedModeTests
{
    private static readonly string _repoRoot = GetRepoRoot();

    [Fact]
    public async Task ConsolidatedMode_Should_List_Tools_Successfully()
    {
        // Arrange
        var exeName = OperatingSystem.IsWindows() ? "azmcp.exe" : "azmcp";
        var azmcpPath = Path.Combine(_repoRoot, "servers", "Azure.Mcp.Server", "src", "bin", "Debug", "net9.0", exeName);

        Assert.True(File.Exists(azmcpPath), $"Executable not found at {azmcpPath}. Please build the Azure.Mcp.Server project first.");

        // Act - Start the server process with consolidated mode
        var processStartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = azmcpPath,
            Arguments = "server start --mode consolidated",
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
            // Give the process a moment to start up
            await Task.Delay(500, TestContext.Current.CancellationToken);

            // Send initialize request first (required by MCP protocol)
            var initRequest = """
                {"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"test-client","version":"1.0"}}}
                """;
            await process.StandardInput.WriteLineAsync(initRequest);
            await process.StandardInput.FlushAsync(TestContext.Current.CancellationToken);

            // Read initialize response
            var initResponse = await ReadJsonRpcResponseAsync(process.StandardOutput);
            Assert.NotNull(initResponse);
            Assert.Contains("\"result\"", initResponse, StringComparison.OrdinalIgnoreCase);

            // Send initialized notification
            var initializedNotification = """
                {"jsonrpc":"2.0","method":"notifications/initialized"}
                """;
            await process.StandardInput.WriteLineAsync(initializedNotification);
            await process.StandardInput.FlushAsync(TestContext.Current.CancellationToken);

            // Send tools/list request
            var listToolsRequest = """
                {"jsonrpc":"2.0","id":2,"method":"tools/list","params":{}}
                """;
            await process.StandardInput.WriteLineAsync(listToolsRequest);
            await process.StandardInput.FlushAsync(TestContext.Current.CancellationToken);

            // Read tools/list response
            var listToolsResponse = await ReadJsonRpcResponseAsync(process.StandardOutput);
            Assert.NotNull(listToolsResponse);

            // Assert - Verify we got tools back
            Assert.Contains("\"result\"", listToolsResponse, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("\"tools\"", listToolsResponse, StringComparison.OrdinalIgnoreCase);

            // Kill the process gracefully
            process.Kill();
            await process.WaitForExitAsync(TestContext.Current.CancellationToken);
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill();
            }
        }
    }

    private static async Task<string?> ReadJsonRpcResponseAsync(System.IO.StreamReader reader)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        try
        {
            return await reader.ReadLineAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
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
