// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client.Helpers;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Xunit;

namespace Azure.Mcp.Core.LiveTests;

public class ClientToolTests(LiveTestFixture liveTestFixture) : IClassFixture<LiveTestFixture>
{
    private readonly IMcpClient _client = liveTestFixture.Client;

    [Fact]
    public async Task Should_List_Tools()
    {
        var tools = await _client.ListToolsAsync(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotEmpty(tools);
    }

    [Fact]
    public async Task Client_Should_Invoke_Tool_Successfully()
    {
        var result = await _client.CallToolAsync("azmcp_subscription_list", new Dictionary<string, object?> { },
            cancellationToken: TestContext.Current.CancellationToken);

        string? content = McpTestUtilities.GetFirstText(result.Content);

        Assert.False(string.IsNullOrWhiteSpace(content));

        var root = JsonSerializer.Deserialize<JsonElement>(content!);
        Assert.Equal(JsonValueKind.Object, root.ValueKind);

        var results = root.AssertProperty("results");
        var subscriptionsArray = results.AssertProperty("subscriptions");
        Assert.Equal(JsonValueKind.Array, subscriptionsArray.ValueKind);

        Assert.NotEmpty(subscriptionsArray.EnumerateArray());
    }

    [Fact]
    public async Task Client_Should_Handle_Invalid_Tools()
    {
        var result = await _client.CallToolAsync("non_existent_tool", new Dictionary<string, object?>(), cancellationToken: TestContext.Current.CancellationToken);

        // When calling a non-existent tool, the server should return an error response
        Assert.True(result.IsError, "Expected error response for non-existent tool");

        string? content = McpTestUtilities.GetFirstText(result.Content);
        Assert.False(string.IsNullOrWhiteSpace(content), "Expected error message content");
        Assert.Contains("The tool non_existent_tool was not found", content);
    }

    [Fact]
    public async Task Client_Should_Ping_Server_Successfully()
    {
        await _client.PingAsync(cancellationToken: TestContext.Current.CancellationToken);
        // If no exception is thrown, the ping was successful.
    }

    [Fact]
    public async Task Should_Error_When_Resources_List_Not_Supported()
    {
        var ex = await Assert.ThrowsAsync<McpException>(async () => await _client.ListResourcesAsync(cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("Request failed", ex.Message);
        Assert.Equal(McpErrorCode.MethodNotFound, ex.ErrorCode);
    }

    [Fact]
    public async Task Should_Error_When_Resources_Read_Not_Supported()
    {
        var ex = await Assert.ThrowsAsync<McpException>(async () => await _client.ReadResourceAsync("test://resource", cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("Request failed", ex.Message);
        Assert.Equal(McpErrorCode.MethodNotFound, ex.ErrorCode);
    }

    [Fact]
    public async Task Should_Error_When_Resources_Templates_List_Not_Supported()
    {
        var ex = await Assert.ThrowsAsync<McpException>(async () => await _client.ListResourceTemplatesAsync(cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("Request failed", ex.Message);
        Assert.Equal(McpErrorCode.MethodNotFound, ex.ErrorCode);
    }

    [Fact]
    public async Task Should_Error_When_Resources_Subscribe_Not_Supported()
    {
        var ex = await Assert.ThrowsAsync<McpException>(async () => await _client.SubscribeToResourceAsync("test://resource", cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("Request failed", ex.Message);
        Assert.Equal(McpErrorCode.MethodNotFound, ex.ErrorCode);
    }

    [Fact]
    public async Task Should_Error_When_Resources_Unsubscribe_Not_Supported()
    {
        var ex = await Assert.ThrowsAsync<McpException>(async () => await _client.UnsubscribeFromResourceAsync("test://resource", cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("Request failed", ex.Message);
        Assert.Equal(McpErrorCode.MethodNotFound, ex.ErrorCode);
    }

    [Fact]
    public async Task Should_Not_Hang_On_Logging_SetLevel_Not_Supported()
    {
        await _client.SetLoggingLevel(LoggingLevel.Info, cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Should_Error_When_Prompts_List_Not_Supported()
    {
        var ex = await Assert.ThrowsAsync<McpException>(async () => await _client.ListPromptsAsync(cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("Request failed", ex.Message);
        Assert.Equal(McpErrorCode.MethodNotFound, ex.ErrorCode);
    }

    [Fact]
    public async Task Should_Error_When_Prompts_Get_Not_Supported()
    {
        var ex = await Assert.ThrowsAsync<McpException>(async () => await _client.GetPromptAsync("unsupported_prompt", cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("Request failed", ex.Message);
        Assert.Equal(McpErrorCode.MethodNotFound, ex.ErrorCode);
    }
}
