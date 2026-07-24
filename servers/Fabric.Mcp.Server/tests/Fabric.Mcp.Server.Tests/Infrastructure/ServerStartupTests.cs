// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Xunit;

namespace Fabric.Mcp.Server.Tests.Infrastructure;

public class ServerStartupTests
{
    [Fact]
    public async Task Server_Should_List_Tools_Over_Http_Root_Endpoint()
    {
        var exeName = OperatingSystem.IsWindows() ? "fabmcp.exe" : "fabmcp";
        var fabmcpPath = Path.Combine(AppContext.BaseDirectory, exeName);

        Assert.True(File.Exists(fabmcpPath), $"Executable not found at {fabmcpPath}");

        var port = GetAvailablePort();
        var processStartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = fabmcpPath,
            Arguments = "server start --transport http --dangerously-disable-http-incoming-auth",
            UseShellExecute = false,
            RedirectStandardInput = false,
            RedirectStandardOutput = false,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        processStartInfo.Environment["ASPNETCORE_URLS"] = $"http://127.0.0.1:{port}";

        using var process = System.Diagnostics.Process.Start(processStartInfo);
        Assert.NotNull(process);

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
            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, $"http://127.0.0.1:{port}/")
            {
                Content = new StringContent("{" + "\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"tools/list\",\"params\":{\"_meta\":{\"protocolVersion\":\"2026-07-28\"}}" + "}", System.Text.Encoding.UTF8, "application/json")
            };
            request.Headers.TryAddWithoutValidation("Accept", "application/json, text/event-stream");
            request.Headers.TryAddWithoutValidation("MCP-Protocol-Version", "2026-07-28");
            request.Headers.TryAddWithoutValidation("Mcp-Method", "tools/list");
            request.Headers.TryAddWithoutValidation("Mcp-Name", "tools/list");

            var response = await SendWithRetryAsync(client, request, TestContext.Current.CancellationToken);
            var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            var errorOutput = stderrBuilder.ToString();
            Assert.DoesNotContain("Unable to resolve service", errorOutput);
            Assert.DoesNotContain("InvalidOperationException", errorOutput);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("\"result\"", content, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("\"tools\"", content, StringComparison.OrdinalIgnoreCase);
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
    public async Task Server_Should_List_Tools_Without_Initialize_And_Without_DI_Errors()
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

            var listToolsRequest = """
                {"jsonrpc":"2.0","id":1,"method":"tools/list","params":{}}
                """;
            await process.StandardInput.WriteLineAsync(listToolsRequest);
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
    public async Task Server_Should_Interop_With_Legacy_Initialize_Without_DI_Errors()
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

            var initRequest = """
                {"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-11-25","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}}
                """;
            await process.StandardInput.WriteLineAsync(initRequest);
            await process.StandardInput.FlushAsync(TestContext.Current.CancellationToken);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var response = await process.StandardOutput.ReadLineAsync(cts.Token);

            var errorOutput = stderrBuilder.ToString();
            Assert.DoesNotContain("Unable to resolve service", errorOutput);
            Assert.DoesNotContain("InvalidOperationException", errorOutput);

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

    private static int GetAvailablePort()
    {
        using var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        return ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
    }

    private static async Task<HttpResponseMessage> SendWithRetryAsync(HttpClient client, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        Exception? lastException = null;

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                using var clonedRequest = CloneRequest(request);
                var response = await client.SendAsync(clonedRequest, cancellationToken);
                if (response.StatusCode != HttpStatusCode.NotFound)
                {
                    return response;
                }

                // Dispose the 404 response before retrying to avoid socket/handler leaks.
                response.Dispose();
            }
            catch (HttpRequestException ex)
            {
                lastException = ex;
            }

            await Task.Delay(250, cancellationToken);
        }

        throw new InvalidOperationException("Timed out waiting for Fabric MCP HTTP endpoint to accept requests.", lastException);
    }

    private static HttpRequestMessage CloneRequest(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Content = request.Content == null ? null : new StringContent(request.Content.ReadAsStringAsync().GetAwaiter().GetResult(), System.Text.Encoding.UTF8, request.Content.Headers.ContentType?.MediaType)
        };

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }
}
