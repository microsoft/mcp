// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Sockets;
using Azure.Mcp.Core.Areas.Server.Commands.Discovery;
using Azure.Mcp.Core.Areas.Server.Models;
using ModelContextProtocol.Client;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Commands.Discovery;

public class RegistryServerProviderTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange
        string testId = "testProvider";
        var serverInfo = new RegistryServerInfo
        {
            Description = "Test Description"
        };

        // Act
        var provider = new RegistryServerProvider(testId, serverInfo);

        // Assert
        Assert.NotNull(provider);
        Assert.IsType<RegistryServerProvider>(provider);
    }

    [Fact]
    public void CreateMetadata_ReturnsExpectedMetadata()
    {
        // Arrange
        string testId = "testProvider";
        var serverInfo = new RegistryServerInfo
        {
            Description = "Test Description"
        };
        var provider = new RegistryServerProvider(testId, serverInfo);

        // Act
        var metadata = provider.CreateMetadata();

        // Assert
        Assert.NotNull(metadata);
        Assert.Equal(testId, metadata.Id);
        Assert.Equal(testId, metadata.Name);
        Assert.Equal(serverInfo.Description, metadata.Description);
    }

    [Fact]
    public void CreateMetadata_EmptyDescription_ReturnsEmptyString()
    {
        // Arrange
        string testId = "testProvider";
        var serverInfo = new RegistryServerInfo
        {
            Description = null
        };
        var provider = new RegistryServerProvider(testId, serverInfo);

        // Act
        var metadata = provider.CreateMetadata();

        // Assert
        Assert.NotNull(metadata);
        Assert.Equal(testId, metadata.Id);
        Assert.Equal(testId, metadata.Name);
        Assert.Equal(string.Empty, metadata.Description);
    }

    [Fact]
    public async Task CreateClientAsync_WithUrlReturning404_ThrowsHttpRequestException()
    {
        // Arrange
        string testId = "sseProvider";
        using var server = new MockHttpTestServer();
        var serverInfo = new RegistryServerInfo
        {
            Description = "Test SSE Provider",
            Url = $"{server.Endpoint}/mcp"
        };
        var provider = new RegistryServerProvider(testId, serverInfo);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => provider.CreateClientAsync(new McpClientOptions()));

        Assert.Contains("404", exception.Message);
    }

    [Fact]
    public async Task CreateClientAsync_WithStdioType_CreatesStdioClient()
    {
        // Arrange
        string testId = "stdioProvider";
        var serverInfo = new RegistryServerInfo
        {
            Description = "Test Stdio Provider",
            Type = "stdio",
            Command = "echo",
            Args = ["hello world"]
        };
        var provider = new RegistryServerProvider(testId, serverInfo);

        // Act & Assert - Should not throw, but the subprocess won't actually start correctly in test
        // Without mocking, we can't easily verify the full client creation
        // This test is just to verify the code path for stdio client creation
        var exception = await Record.ExceptionAsync(() => provider.CreateClientAsync(new McpClientOptions()));

        // We expect some kind of exception during the subprocess startup, but not an InvalidOperationException
        // about missing command or invalid transport
        Assert.NotNull(exception);
        Assert.IsNotType<InvalidOperationException>(exception);
    }

    [Fact]
    public async Task CreateClientAsync_WithEnvVariables_MergesWithSystemEnvironment()
    {
        // Arrange
        string testId = "envProvider";
        var serverInfo = new RegistryServerInfo
        {
            Description = "Test Env Provider",
            Type = "stdio",
            Command = "echo",
            Args = ["hello world"],
            Env = new Dictionary<string, string>
                {
                    { "TEST_VAR", "test value" }
                }
        };
        var provider = new RegistryServerProvider(testId, serverInfo);

        // Act & Assert - Should not throw, but the subprocess won't actually start correctly in test
        var exception = await Record.ExceptionAsync(() => provider.CreateClientAsync(new McpClientOptions()));

        // We expect some kind of exception during the subprocess startup, but not an InvalidOperationException
        // about missing command or invalid transport
        Assert.NotNull(exception);
        Assert.IsNotType<InvalidOperationException>(exception);
    }

    [Fact]
    public async Task CreateClientAsync_NoUrlOrType_ThrowsInvalidOperationException()
    {
        // Arrange
        string testId = "invalidProvider";
        var serverInfo = new RegistryServerInfo
        {
            Description = "Invalid Provider - No Transport"
            // No Url or Type specified
        };
        var provider = new RegistryServerProvider(testId, serverInfo);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => provider.CreateClientAsync(new McpClientOptions()));

        Assert.Contains($"Registry server '{testId}' does not have a valid url or type for transport.",
            exception.Message);
    }

    [Fact]
    public async Task CreateClientAsync_StdioWithoutCommand_ThrowsInvalidOperationException()
    {
        // Arrange
        string testId = "invalidStdioProvider";
        var serverInfo = new RegistryServerInfo
        {
            Description = "Invalid Stdio Provider - No Command",
            Type = "stdio"
            // No Command specified
        };
        var provider = new RegistryServerProvider(testId, serverInfo);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => provider.CreateClientAsync(new McpClientOptions()));

        Assert.Contains($"Registry server '{testId}' does not have a valid command for stdio transport.",
            exception.Message);
    }
}

internal sealed class MockHttpTestServer : IDisposable
{
    private readonly HttpListener _listener;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly TaskCompletionSource _ready;
    public string Endpoint { get; }

    public MockHttpTestServer()
    {
        var port = GetAvailablePort();
        Endpoint = $"http://127.0.0.1:{port}";

        _listener = new HttpListener();
        _listener.Prefixes.Add($"{Endpoint}/");
        _listener.Start();

        _cancellationTokenSource = new CancellationTokenSource();
        _ready = new TaskCompletionSource();

        _ = Task.Run(async () =>
        {
            try
            {
                _ready.SetResult();

                while (_listener.IsListening && !_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    var context = await _listener.GetContextAsync();
                    if (context.Request.Url?.AbsolutePath == "/mcp")
                    {
                        context.Response.StatusCode = 404;
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                    context.Response.Close();
                }
            }
            catch (Exception ex) when (ex is ObjectDisposedException or HttpListenerException)
            {
                // expected when listener is disposed or stopped.
                if (!_ready.Task.IsCompleted)
                {
                    _ready.SetException(ex);
                }
            }
        }, _cancellationTokenSource.Token);

        _ready.Task.Wait(TimeSpan.FromSeconds(10));
    }

    private static int GetAvailablePort()
    {
        using var tempListener = new TcpListener(IPAddress.Loopback, 0);
        tempListener.Start();
        var port = ((IPEndPoint)tempListener.LocalEndpoint).Port;
        tempListener.Stop();
        return port;
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _listener.Stop();
        _listener.Close();
        _cancellationTokenSource.Dispose();
    }
}
