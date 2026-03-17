// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Xunit;

namespace Fabric.Mcp.Server.UnitTests.Infrastructure;

public class ServerStartupTests
{
    [Fact]
    public async Task Server_Should_Initialize_Without_DI_Errors()
    {
        // Arrange
        var exeName = OperatingSystem.IsWindows() ? "fabmcp.exe" : "fabmcp";
        var fabmcpPath = Path.Combine(AppContext.BaseDirectory, exeName);

        Assert.True(File.Exists(fabmcpPath), $"Executable not found at {fabmcpPath}");

        var processStartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = fabmcpPath,
            Arguments = "server start",
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = System.Diagnostics.Process.Start(processStartInfo);
        Assert.NotNull(process);

        // Collect stderr asynchronously
        var stderrBuilder = new System.Text.StringBuilder();
        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                stderrBuilder.AppendLine(e.Data);
            }
        };
        process.BeginErrorReadLine();

        try
        {
            await Task.Delay(500, TestContext.Current.CancellationToken);

            // Send MCP initialize request
            var initRequest = """
                {"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}}
                """;
            await process.StandardInput.WriteLineAsync(initRequest);
            await process.StandardInput.FlushAsync(TestContext.Current.CancellationToken);

            // Read response - should get valid JSON, not an exception
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var response = await process.StandardOutput.ReadLineAsync(cts.Token);

            // Check stderr for DI exceptions
            var errorOutput = stderrBuilder.ToString();
            Assert.DoesNotContain("Unable to resolve service", errorOutput);
            Assert.DoesNotContain("InvalidOperationException", errorOutput);

            // Verify we got a valid response
            Assert.NotNull(response);
            Assert.Contains("\"result\"", response, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill();
            }
        }
    }
}
