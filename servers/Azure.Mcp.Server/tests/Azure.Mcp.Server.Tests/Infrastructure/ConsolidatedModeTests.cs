// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Xunit;

namespace Azure.Mcp.Server.Tests.Infrastructure;

public class ConsolidatedModeTests
{
    [Fact]
    public async Task ConsolidatedMode_HttpTransport_Should_List_Tools_On_Root_Endpoint()
    {
        var exeName = OperatingSystem.IsWindows() ? "azmcp.exe" : "azmcp";
        var azmcpPath = Path.Combine(AppContext.BaseDirectory, exeName);

        Assert.True(File.Exists(azmcpPath), $"Executable not found at {azmcpPath}. Please build the Azure.Mcp.Server project first.");

        var port = GetAvailablePort();
        var processStartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = azmcpPath,
            Arguments = "server start --mode consolidated --transport http --dangerously-disable-http-incoming-auth",
            UseShellExecute = false,
            RedirectStandardInput = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        processStartInfo.Environment["ASPNETCORE_URLS"] = $"http://127.0.0.1:{port}";

        using var process = System.Diagnostics.Process.Start(processStartInfo);
        Assert.NotNull(process);

        try
        {
            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, $"http://127.0.0.1:{port}/")
            {
                Content = new StringContent("{" + "\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"tools/list\",\"params\":{}" + "}", System.Text.Encoding.UTF8, "application/json")
            };
            request.Headers.TryAddWithoutValidation("Accept", "application/json, text/event-stream");
            request.Headers.TryAddWithoutValidation("MCP-Protocol-Version", "2026-07-28");
            request.Headers.TryAddWithoutValidation("Mcp-Method", "tools/list");
            request.Headers.TryAddWithoutValidation("Mcp-Name", "tools/list");

            var response = await SendWithRetryAsync(client, request, TestContext.Current.CancellationToken);
            var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("\"result\"", content, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("\"tools\"", content, StringComparison.OrdinalIgnoreCase);
            // Workstream I item 7 / Workstream B: stateless protocol must not set Mcp-Session-Id
            Assert.False(response.Headers.Contains("Mcp-Session-Id"), "Server must not set Mcp-Session-Id under the 2026-07-28 stateless protocol.");
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
    public async Task ConsolidatedMode_Should_List_Tools_Without_Initialize()
    {
        // Arrange
        var exeName = OperatingSystem.IsWindows() ? "azmcp.exe" : "azmcp";
        var azmcpPath = Path.Combine(AppContext.BaseDirectory, exeName);

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

            // Send tools/list request directly on stateless protocol path.
            var listToolsRequest = """
                {"jsonrpc":"2.0","id":1,"method":"tools/list","params":{}}
                """;
            await process.StandardInput.WriteLineAsync(listToolsRequest);
            await process.StandardInput.FlushAsync(TestContext.Current.CancellationToken);

            // Read tools/list response
            var listToolsResponse = await ReadJsonRpcResponseAsync(process.StandardOutput);
            Assert.NotNull(listToolsResponse);

            // Assert - Verify we got tools back
            Assert.Contains("\"result\"", listToolsResponse, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("\"tools\"", listToolsResponse, StringComparison.OrdinalIgnoreCase);
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
    public async Task ConsolidatedMode_Should_Interop_With_Legacy_Initialize_Handshake()
    {
        // Arrange
        var exeName = OperatingSystem.IsWindows() ? "azmcp.exe" : "azmcp";
        var azmcpPath = Path.Combine(AppContext.BaseDirectory, exeName);

        Assert.True(File.Exists(azmcpPath), $"Executable not found at {azmcpPath}. Please build the Azure.Mcp.Server project first.");

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
            await Task.Delay(500, TestContext.Current.CancellationToken);

            var initRequest = """
                {"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-11-25","capabilities":{},"clientInfo":{"name":"test-client","version":"1.0"}}}
                """;
            await process.StandardInput.WriteLineAsync(initRequest);
            await process.StandardInput.FlushAsync(TestContext.Current.CancellationToken);

            var initResponse = await ReadJsonRpcResponseAsync(process.StandardOutput);
            Assert.NotNull(initResponse);
            Assert.Contains("\"result\"", initResponse, StringComparison.OrdinalIgnoreCase);

            var initializedNotification = """
                {"jsonrpc":"2.0","method":"notifications/initialized"}
                """;
            await process.StandardInput.WriteLineAsync(initializedNotification);
            await process.StandardInput.FlushAsync(TestContext.Current.CancellationToken);

            var listToolsRequest = """
                {"jsonrpc":"2.0","id":2,"method":"tools/list","params":{}}
                """;
            await process.StandardInput.WriteLineAsync(listToolsRequest);
            await process.StandardInput.FlushAsync(TestContext.Current.CancellationToken);

            var listToolsResponse = await ReadJsonRpcResponseAsync(process.StandardOutput);
            Assert.NotNull(listToolsResponse);
            Assert.Contains("\"result\"", listToolsResponse, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("\"tools\"", listToolsResponse, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill();
            }
        }
    }

    private static async Task<string?> ReadJsonRpcResponseAsync(StreamReader reader)
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

    [Fact]
    public async Task ConsolidatedMode_HttpTransport_Should_Reject_Request_Without_Accept_Header()
    {
        var exeName = OperatingSystem.IsWindows() ? "azmcp.exe" : "azmcp";
        var azmcpPath = Path.Combine(AppContext.BaseDirectory, exeName);

        Assert.True(File.Exists(azmcpPath), $"Executable not found at {azmcpPath}.");

        var port = GetAvailablePort();
        var processStartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = azmcpPath,
            Arguments = "server start --mode consolidated --transport http --dangerously-disable-http-incoming-auth",
            UseShellExecute = false,
            CreateNoWindow = true
        };
        processStartInfo.Environment["ASPNETCORE_URLS"] = $"http://127.0.0.1:{port}";

        using var process = System.Diagnostics.Process.Start(processStartInfo);
        Assert.NotNull(process);

        try
        {
            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, $"http://127.0.0.1:{port}/")
            {
                Content = new StringContent("{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"tools/list\",\"params\":{}}", System.Text.Encoding.UTF8, "application/json")
            };
            // Intentionally omit Accept header — server must respond 406

            var response = await SendWithRetryAsync(client, request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotAcceptable, response.StatusCode);
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
            }
            catch (HttpRequestException ex)
            {
                lastException = ex;
            }

            await Task.Delay(250, cancellationToken);
        }

        throw new InvalidOperationException("Timed out waiting for Azure MCP HTTP endpoint to accept requests.", lastException);
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
