// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Reflection;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Xunit;

namespace Azure.Mcp.Tests.Client.Helpers;

public static class McpTestUtilities
{
    /// <summary>Gets the first text contents in the list.</summary>
    public static string? GetFirstText(IList<ContentBlock> contents)
    {
        foreach (var c in contents)
        {
            if (c is EmbeddedResourceBlock { Resource: TextResourceContents { MimeType: "application/json" } text })
            {
                return text.Text;
            }
            else if (c is TextContentBlock tc)
            {
                return tc.Text;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the path to the azmcp executable, handling OS-specific executable naming.
    /// </summary>
    /// <returns>The full path to the azmcp executable.</returns>
    public static string GetAzMcpExecutablePath()
    {
        string testAssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        string executableName = OperatingSystem.IsWindows() ? "azmcp.exe" : "azmcp";
        return Path.Combine(testAssemblyPath, executableName);
    }

    /// <summary>
    /// Creates and initializes an MCP client based on the configured transport type.
    /// Supports both HTTP and STDIO transports.
    /// </summary>
    /// <param name="executablePath">The path to the azmcp executable.</param>
    /// <param name="arguments">Command-line arguments for the server process.</param>
    /// <param name="environmentVariables">Environment variables to set for the server process.</param>
    /// <param name="storeHttpServerProcess">Optional callback to store the HTTP server process instance for cleanup.</param>
    /// <param name="output">Optional test output helper for logging.</param>
    /// <param name="testPackage">Optional NPM test package name for STDIO mode.</param>
    /// <param name="settingsDirectory">Optional settings directory for NPM test package.</param>
    /// <returns>A tuple containing the initialized MCP client and optional server URL (for HTTP transport).</returns>
    public static async Task<(McpClient? Client, string? ServerUrl)> CreateMcpClientAsync(
        string executablePath,
        List<string> arguments,
        Dictionary<string, string?> environmentVariables,
        Action<Process>? storeHttpServerProcess = null,
        ITestOutputHelper? output = null,
        string? testPackage = null,
        string? settingsDirectory = null,
        bool disableAuthentication = true)
    {
        bool useHttp = string.Equals(
            Environment.GetEnvironmentVariable("MCP_TEST_TRANSPORT"),
            "http",
            StringComparison.OrdinalIgnoreCase);

        if (useHttp)
        {
            string serverUrl = await StartHttpServerAsync(
                executablePath,
                arguments,
                environmentVariables,
                storeHttpServerProcess,
                output, disableAuthentication);

            var transport = new HttpClientTransport(new()
            {
                Endpoint = new Uri(serverUrl),
                TransportMode = HttpTransportMode.StreamableHttp
            });

            McpClient? client = null;
            if (disableAuthentication)
            {
                // Authenticated sessions should use HttpClient
                client = await McpClient.CreateAsync(transport);
            }

            output?.WriteLine($"HTTP test client initialized at {serverUrl}");

            return (client, serverUrl);
        }
        else
        {
            var clientTransport = CreateStdioTransport(
                executablePath,
                arguments,
                environmentVariables,
                output,
                testPackage,
                settingsDirectory);
            output?.WriteLine("Attempting to start MCP Client");

            var client = await McpClient.CreateAsync(clientTransport);
            output?.WriteLine("MCP client initialized successfully");

            return (client, null);
        }
    }

    /// <summary>
    /// Starts an HTTP server and returns the server URL.
    /// </summary>
    /// <param name="executablePath">The path to the azmcp executable.</param>
    /// <param name="arguments">Command-line arguments for the server process.</param>
    /// <param name="environmentVariables">Environment variables to set for the server process.</param>
    /// <param name="startHttpServerProcess">Callback to store the started process instance.</param>
    /// <param name="output">Optional test output helper for logging.</param>
    /// <returns>The server URL.</returns>
    private static async Task<string> StartHttpServerAsync(
        string executablePath,
        List<string> arguments,
        Dictionary<string, string?> environmentVariables,
        Action<Process>? startHttpServerProcess,
        ITestOutputHelper? output, bool disableAuthentication = true)
    {
        var port = GetAvailablePort();
        var serverUrl = $"http://localhost:{port}";
        environmentVariables["ASPNETCORE_URLS"] = serverUrl;

        var process = await StartHttpServerProcessAndWaitForReadinessAsync(
            executablePath,
            arguments,
            environmentVariables,
            serverUrl,
            output,
            disableAuthentication: disableAuthentication);

        startHttpServerProcess?.Invoke(process);

        return serverUrl;
    }

    /// <summary>
    /// Creates a STDIO transport for the MCP client.
    /// </summary>
    /// <param name="executablePath">The path to the azmcp executable.</param>
    /// <param name="arguments">Command-line arguments for the server process.</param>
    /// <param name="environmentVariables">Environment variables to set for the server process.</param>
    /// <param name="output">Optional test output helper for logging.</param>
    /// <param name="testPackage">Optional NPM test package name.</param>
    /// <param name="settingsDirectory">Optional settings directory for NPM test package.</param>
    /// <returns>A configured STDIO client transport.</returns>
    private static IClientTransport CreateStdioTransport(
        string executablePath,
        List<string> arguments,
        Dictionary<string, string?> environmentVariables,
        ITestOutputHelper? output = null,
        string? testPackage = null,
        string? settingsDirectory = null)
    {
        StdioClientTransportOptions transportOptions = new()
        {
            Name = "Test Server",
            Command = executablePath,
            Arguments = arguments.ToArray(),
            StandardErrorLines = line => output?.WriteLine($"[MCP Server] {line}"),
            EnvironmentVariables = environmentVariables
        };

        if (!string.IsNullOrEmpty(testPackage))
        {
            if (!string.IsNullOrEmpty(settingsDirectory))
            {
                Environment.CurrentDirectory = settingsDirectory;
            }
            transportOptions.Command = "npx";
            transportOptions.Arguments = ["-y", testPackage, .. arguments];
        }

        return new StdioClientTransport(transportOptions);
    }

    /// <summary>
    /// Gets an available TCP port by binding to port 0 and releasing it.
    /// </summary>
    /// <returns>An available port number.</returns>
    private static int GetAvailablePort()
    {
        using var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    /// <summary>
    /// Starts an HTTP server process and waits for it to be ready.
    /// </summary>
    /// <param name="executablePath">The path to the azmcp executable.</param>
    /// <param name="processArguments">Command-line arguments for the server process.</param>
    /// <param name="environmentVariables">Environment variables to set for the server process.</param>
    /// <param name="serverUrl">The server URL to check for readiness.</param>
    /// <param name="output">Optional test output helper for logging.</param>
    /// <param name="timeoutSeconds">Timeout in seconds to wait for server readiness.</param>
    /// <param name="pollIntervalMs">Poll interval in milliseconds.</param>
    /// <param name="disableAuthentication">Whether authentication is disabled.</param>
    /// <returns>The started Process instance.</returns>
    public static async Task<Process> StartHttpServerProcessAndWaitForReadinessAsync(
        string executablePath,
        List<string> processArguments,
        Dictionary<string, string?> environmentVariables,
        string serverUrl,
        ITestOutputHelper? output = null,
        int timeoutSeconds = 30,
        int pollIntervalMs = 500,
        bool disableAuthentication = true)
    {
        var process = StartHttpServerProcess(
            executablePath,
            processArguments,
            environmentVariables,
            output,
            disableAuthentication);

        // Invert: disableAuthentication=false means authenticationEnabled=true
        await WaitForServerReadinessAsync(serverUrl, timeoutSeconds, pollIntervalMs, authenticationEnabled: !disableAuthentication);

        return process;
    }

    /// <summary>
    /// Waits for an HTTP server to become ready by polling the tools/list endpoint.
    /// </summary>
    /// <param name="serverUrl">The server URL to check for readiness.</param>
    /// <param name="timeoutSeconds">Timeout in seconds to wait for server readiness.</param>
    /// <param name="pollIntervalMs">Poll interval in milliseconds.</param>
    /// <param name="authenticationEnabled">Whether authentication is enabled on the server.</param>
    public static async Task WaitForServerReadinessAsync(
        string serverUrl,
        int timeoutSeconds = 30,
        int pollIntervalMs = 500,
        bool authenticationEnabled = false)
    {
        using var httpClient = new HttpClient();
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < timeout)
        {
            try
            {
                var request = new
                {
                    jsonrpc = "2.0",
                    method = "tools/list",
                    @params = new { },
                    id = Guid.NewGuid().ToString()
                };

                var content = new System.Net.Http.StringContent(
                    System.Text.Json.JsonSerializer.Serialize(request),
                    System.Text.Encoding.UTF8,
                    "application/json");
                using var requestMessage = new HttpRequestMessage(HttpMethod.Post, serverUrl)
                {
                    Content = content
                };
                requestMessage.Headers.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                requestMessage.Headers.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/event-stream"));
                using var resp = await httpClient.SendAsync(requestMessage);

                // If authentication is enabled, 401 Unauthorized means server is ready
                // If authentication is disabled, we need a success status code
                if (resp.IsSuccessStatusCode || (authenticationEnabled && resp.StatusCode == System.Net.HttpStatusCode.Unauthorized))
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
                // Server not yet available, continue polling
            }
            await Task.Delay(pollIntervalMs);
        }

        throw new TimeoutException($"Server at {serverUrl} did not become ready within {timeoutSeconds} seconds");
    }

    /// <summary>
    /// Starts an HTTP server process with the specified configuration.
    /// </summary>
    /// <param name="executablePath">The path to the azmcp executable.</param>
    /// <param name="processArguments">Command-line arguments for the server process.</param>
    /// <param name="environmentVariables">Environment variables to set for the server process.</param>
    /// <param name="output">Optional test output helper for logging.</param>
    /// <returns>The started Process instance.</returns>
    public static Process StartHttpServerProcess(
        string executablePath,
        List<string> processArguments,
        Dictionary<string, string?> environmentVariables,
        ITestOutputHelper? output = null, bool disableAuthentication = true)
    {
        processArguments.AddRange(["--transport", "http", "--outgoing-auth-strategy", "UseHostingEnvironmentIdentity"]);
        if (disableAuthentication)
        {
            processArguments.Add("--dangerously-disable-http-incoming-auth");
        }

        var processStartInfo = new System.Diagnostics.ProcessStartInfo(executablePath, string.Join(" ", processArguments))
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        foreach (var kvp in environmentVariables)
        {
            if (kvp.Value != null)
            {
                processStartInfo.Environment[kvp.Key] = kvp.Value;
            }
        }

        var process = System.Diagnostics.Process.Start(processStartInfo);

        if (process == null)
        {
            throw new InvalidOperationException("Failed to start HTTP server process.");
        }

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                try
                {
                    output?.WriteLine($"[HTTP Server stdout] {e.Data}");
                }
                catch (InvalidOperationException)
                {
                    // Test has completed; ignore output
                }
            }
        };
        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                try
                {
                    output?.WriteLine($"[HTTP Server stderr] {e.Data}");
                }
                catch (InvalidOperationException)
                {
                    // Test has completed; ignore output
                }
            }
        };

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        return process;
    }
}
