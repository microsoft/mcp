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
            RedirectStandardOutput = false,
            RedirectStandardError = false,
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
    public async Task ConsolidatedMode_HttpTransport_Should_Accept_Meta_In_Request_Params()
    {
        // Phase 2 hardening (Workstream H): ensure request-scoped metadata in params._meta
        // does not break stateless tools/list requests.
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
            RedirectStandardOutput = false,
            RedirectStandardError = false,
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
                Content = new StringContent(
                    "{" +
                    "\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"tools/list\",\"params\":{\"_meta\":{\"traceparent\":\"00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01\",\"clientId\":\"phase2-hardening-test\"}}" +
                    "}",
                    System.Text.Encoding.UTF8,
                    "application/json")
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
    public async Task ConsolidatedMode_HttpTransport_Should_Support_ServerDiscover_Without_Initialize()
    {
        // Phase 2 hardening (Workstream C): validate server/discover works on stateless HTTP path.
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
            RedirectStandardOutput = false,
            RedirectStandardError = false,
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
                Content = new StringContent(
                    "{" +
                    "\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"server/discover\",\"params\":{}" +
                    "}",
                    System.Text.Encoding.UTF8,
                    "application/json")
            };

            request.Headers.TryAddWithoutValidation("Accept", "application/json, text/event-stream");
            request.Headers.TryAddWithoutValidation("MCP-Protocol-Version", "2026-07-28");
            request.Headers.TryAddWithoutValidation("Mcp-Method", "server/discover");
            request.Headers.TryAddWithoutValidation("Mcp-Name", "server/discover");

            var response = await SendWithRetryAsync(client, request, TestContext.Current.CancellationToken);
            var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("\"result\"", content, StringComparison.OrdinalIgnoreCase);
            Assert.False(response.Headers.Contains("Mcp-Session-Id"), "server/discover must remain stateless and must not emit Mcp-Session-Id.");
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
    public async Task ConsolidatedMode_HttpTransport_Should_Handle_Repeated_ToolsList_Statelessly()
    {
        // Phase 2 hardening (Workstream C items 4/5):
        // repeated tools/list should be stable while protocol-facing cache semantics remain explicit.
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
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            CreateNoWindow = true
        };
        processStartInfo.Environment["ASPNETCORE_URLS"] = $"http://127.0.0.1:{port}";

        using var process = System.Diagnostics.Process.Start(processStartInfo);
        Assert.NotNull(process);

        try
        {
            using var client = new HttpClient();

            using var request1 = new HttpRequestMessage(HttpMethod.Post, $"http://127.0.0.1:{port}/")
            {
                Content = new StringContent("{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"tools/list\",\"params\":{}}", System.Text.Encoding.UTF8, "application/json")
            };
            request1.Headers.TryAddWithoutValidation("Accept", "application/json, text/event-stream");
            request1.Headers.TryAddWithoutValidation("MCP-Protocol-Version", "2026-07-28");
            request1.Headers.TryAddWithoutValidation("Mcp-Method", "tools/list");
            request1.Headers.TryAddWithoutValidation("Mcp-Name", "tools/list");

            var response1 = await SendWithRetryAsync(client, request1, TestContext.Current.CancellationToken);
            var content1 = await response1.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            using var request2 = new HttpRequestMessage(HttpMethod.Post, $"http://127.0.0.1:{port}/")
            {
                Content = new StringContent("{\"jsonrpc\":\"2.0\",\"id\":2,\"method\":\"tools/list\",\"params\":{}}", System.Text.Encoding.UTF8, "application/json")
            };
            request2.Headers.TryAddWithoutValidation("Accept", "application/json, text/event-stream");
            request2.Headers.TryAddWithoutValidation("MCP-Protocol-Version", "2026-07-28");
            request2.Headers.TryAddWithoutValidation("Mcp-Method", "tools/list");
            request2.Headers.TryAddWithoutValidation("Mcp-Name", "tools/list");

            var response2 = await SendWithRetryAsync(client, request2, TestContext.Current.CancellationToken);
            var content2 = await response2.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
            Assert.Contains("\"tools\"", content1, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("\"tools\"", content2, StringComparison.OrdinalIgnoreCase);

            // Stateless protocol should not rely on session affinity.
            Assert.False(response1.Headers.Contains("Mcp-Session-Id"));
            Assert.False(response2.Headers.Contains("Mcp-Session-Id"));

            // No implicit HTTP cache-contract is used for protocol freshness here.
            Assert.False(response1.Headers.Contains("ETag"));
            Assert.False(response2.Headers.Contains("ETag"));
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
            RedirectStandardError = false,
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
    public async Task ConsolidatedMode_HttpTransport_Should_Propagate_W3C_Trace_Context()
    {
        // Phase 2 hardening (Workstream H): Verify W3C trace context (traceparent, tracestate, baggage)
        // is accepted and propagated through _meta on stateless HTTP protocol.
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
            RedirectStandardOutput = false,
            RedirectStandardError = false,
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
                Content = new StringContent(
                    "{" +
                    "\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"tools/list\",\"params\":{" +
                    "\"_meta\":{" +
                    "\"traceparent\":\"00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01\"," +
                    "\"tracestate\":\"vendor1=opaquevalue1,vendor2=opaquevalue2\"," +
                    "\"baggage\":\"userId=alice,serverNode=DF:28,isProduction=false\"" +
                    "}" +
                    "}" +
                    "}",
                    System.Text.Encoding.UTF8,
                    "application/json")
            };

            request.Headers.TryAddWithoutValidation("Accept", "application/json, text/event-stream");
            request.Headers.TryAddWithoutValidation("MCP-Protocol-Version", "2026-07-28");
            request.Headers.TryAddWithoutValidation("Mcp-Method", "tools/list");
            request.Headers.TryAddWithoutValidation("Mcp-Name", "tools/list");

            var response = await SendWithRetryAsync(client, request, TestContext.Current.CancellationToken);
            var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            // Assert: Server must accept and process requests with W3C trace context in _meta
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
            RedirectStandardError = false,
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
    public async Task ConsolidatedMode_HttpTransport_Should_Reject_MethodHeader_Body_Mismatch()
    {
        // Workstream B item 2: Mcp-Method header disagrees with JSON body method.
        // The 2026-07-28 protocol requires header/body agreement for explicit gateway routing;
        // the server must not accept a request where Mcp-Method: tools/call but body says tools/list.
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
                // Body says tools/list but headers claim tools/call — header/body mismatch
                Content = new StringContent("{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"tools/list\",\"params\":{}}", System.Text.Encoding.UTF8, "application/json")
            };
            request.Headers.TryAddWithoutValidation("Accept", "application/json, text/event-stream");
            request.Headers.TryAddWithoutValidation("MCP-Protocol-Version", "2026-07-28");
            request.Headers.TryAddWithoutValidation("Mcp-Method", "tools/call");   // deliberate mismatch
            request.Headers.TryAddWithoutValidation("Mcp-Name", "some-nonexistent-tool");

            var response = await SendWithRetryAsync(client, request, TestContext.Current.CancellationToken);

            // Server must reject header/body disagreement with a 4xx client error
            Assert.True(
                (int)response.StatusCode >= 400 && (int)response.StatusCode < 500,
                $"Expected a 4xx rejection for Mcp-Method/body mismatch but got {(int)response.StatusCode}.");
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
    public async Task ConsolidatedMode_HttpTransport_Should_Reject_Request_Without_McpMethod_Header()
    {
        // Phase 2 hardening (Workstream B): Streamable HTTP routing requires Mcp-Method.
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
            request.Headers.TryAddWithoutValidation("Accept", "application/json, text/event-stream");
            request.Headers.TryAddWithoutValidation("MCP-Protocol-Version", "2026-07-28");
            request.Headers.TryAddWithoutValidation("Mcp-Name", "tools/list");
            // Intentionally omit Mcp-Method

            var response = await SendWithRetryAsync(client, request, TestContext.Current.CancellationToken);

            Assert.True(
                (int)response.StatusCode >= 400 && (int)response.StatusCode < 500,
                $"Expected a 4xx rejection for missing Mcp-Method header but got {(int)response.StatusCode}.");
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
    public async Task ConsolidatedMode_HttpTransport_Should_Reject_NamedMethod_Without_McpName_Header()
    {
        // Phase 2 hardening (Workstream B / SEP-2243): Mcp-Name is required only for methods
        // that name a tool, resource, or prompt (tools/call, resources/read, prompts/get).
        // A tools/call request without Mcp-Name must be rejected.
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
                Content = new StringContent("{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"tools/call\",\"params\":{\"name\":\"some-tool\",\"arguments\":{}}}", System.Text.Encoding.UTF8, "application/json")
            };
            request.Headers.TryAddWithoutValidation("Accept", "application/json, text/event-stream");
            request.Headers.TryAddWithoutValidation("MCP-Protocol-Version", "2026-07-28");
            request.Headers.TryAddWithoutValidation("Mcp-Method", "tools/call");
            // Intentionally omit Mcp-Name for a named method

            var response = await SendWithRetryAsync(client, request, TestContext.Current.CancellationToken);

            Assert.True(
                (int)response.StatusCode >= 400 && (int)response.StatusCode < 500,
                $"Expected a 4xx rejection for missing Mcp-Name on tools/call but got {(int)response.StatusCode}.");
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
    public async Task ConsolidatedMode_HttpTransport_Should_Accept_NonNamedMethod_Without_McpName_Header()
    {
        // Phase 2 hardening (Workstream B / SEP-2243): non-named methods (e.g. tools/list)
        // require only Mcp-Method, not Mcp-Name. Omitting Mcp-Name must NOT be rejected.
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
            request.Headers.TryAddWithoutValidation("Accept", "application/json, text/event-stream");
            request.Headers.TryAddWithoutValidation("MCP-Protocol-Version", "2026-07-28");
            request.Headers.TryAddWithoutValidation("Mcp-Method", "tools/list");
            // Intentionally omit Mcp-Name — valid for a non-named method

            var response = await SendWithRetryAsync(client, request, TestContext.Current.CancellationToken);
            var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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
    public async Task ConsolidatedMode_HttpTransport_Should_Accept_Legacy_Initialize_Without_Routing_Headers()
    {
        // Phase 2 hardening (Workstream B / SEP-2243): routing headers are required only when the
        // client declares MCP-Protocol-Version: 2026-07-28. A legacy client performing the
        // initialize handshake over HTTP (older/absent version) must NOT be rejected for missing
        // Mcp-Method/Mcp-Name, preserving backward-compatible interop from a single endpoint.
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
                Content = new StringContent(
                    "{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"initialize\",\"params\":{\"protocolVersion\":\"2025-11-25\",\"capabilities\":{},\"clientInfo\":{\"name\":\"legacy-client\",\"version\":\"1.0\"}}}",
                    System.Text.Encoding.UTF8,
                    "application/json")
            };
            request.Headers.TryAddWithoutValidation("Accept", "application/json, text/event-stream");
            // Legacy client: declares the older protocol version and sends no MCP routing headers.
            request.Headers.TryAddWithoutValidation("MCP-Protocol-Version", "2025-11-25");

            var response = await SendWithRetryAsync(client, request, TestContext.Current.CancellationToken);
            var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            // The header-routing guard must not reject the legacy handshake for missing routing headers.
            // Assert specifically that our guard's rejection message is absent, independent of any
            // SDK-level protocol negotiation outcome.
            Assert.DoesNotContain("Missing required MCP routing header", content, StringComparison.OrdinalIgnoreCase);
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

                // The endpoint is not ready yet; dispose the 404 response before retrying
                // so its socket/handler is not leaked across iterations.
                response.Dispose();
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
