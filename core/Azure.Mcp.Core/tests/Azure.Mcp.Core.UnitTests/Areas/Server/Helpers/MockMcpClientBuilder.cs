// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Areas.Server;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using NSubstitute;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Helpers;

/// <summary>
/// A builder for creating mock <see cref="IMcpClient"/> instances for testing purposes.
/// Provides a fluent API for registering mock tools with custom handlers.
/// </summary>
public sealed class MockMcpClientBuilder
{
    private readonly Dictionary<string, MockTool> _tools = new();

    /// <summary>
    /// Adds a mock tool with a custom Tool object and handler using fluent API.
    /// </summary>
    /// <param name="tool">The Tool object containing name, description, and schema.</param>
    /// <param name="handler">The handler function to execute when the tool is called.</param>
    /// <returns>The current instance for method chaining.</returns>
    public MockMcpClientBuilder AddTool(Tool tool, Func<IReadOnlyDictionary<string, object?>?, CallToolResult> handler)
    {
        var mockTool = new MockTool(tool, handler);
        _tools[tool.Name] = mockTool;
        return this;
    }

    /// <summary>
    /// Adds a mock tool with a custom handler using fluent API.
    /// </summary>
    /// <param name="name">The name of the tool.</param>
    /// <param name="description">The description of the tool.</param>
    /// <param name="handler">The handler function to execute when the tool is called.</param>
    /// <returns>The current instance for method chaining.</returns>
    public MockMcpClientBuilder AddTool(string name, string description, Func<IReadOnlyDictionary<string, object?>?, CallToolResult> handler)
    {
        var tool = new Tool
        {
            Name = name,
            Description = description,
            InputSchema = JsonDocument.Parse("""{"type": "object", "properties": {}}""").RootElement
        };
        return AddTool(tool, handler);
    }

    /// <summary>
    /// Adds a mock tool with a custom handler using fluent API.
    /// </summary>
    /// <param name="name">The name of the tool.</param>
    /// <param name="description">The description of the tool.</param>
    /// <param name="handler">The handler function to execute when the tool is called.</param>
    /// <returns>The current instance for method chaining.</returns>
    public MockMcpClientBuilder AddTool(string name, string description, Func<CallToolResult> handler)
    {
        return AddTool(name, description, _ => handler());
    }

    /// <summary>
    /// Adds a mock tool with a simple text response.
    /// </summary>
    /// <param name="name">The name of the tool.</param>
    /// <param name="description">The description of the tool.</param>
    /// <param name="response">The text response to return.</param>
    /// <returns>The current instance for method chaining.</returns>
    public MockMcpClientBuilder AddTool(string name, string description, string response)
    {
        return AddTool(name, description, () => new CallToolResult
        {
            Content = [new TextContentBlock { Text = response }],
            IsError = false
        });
    }

    /// <summary>
    /// Adds a mock tool that returns an error response.
    /// </summary>
    /// <param name="name">The name of the tool.</param>
    /// <param name="description">The description of the tool.</param>
    /// <param name="errorMessage">The error message to return.</param>
    /// <returns>The current instance for method chaining.</returns>
    public MockMcpClientBuilder AddErrorTool(string name, string description, string errorMessage)
    {
        return AddTool(name, description, () => new CallToolResult
        {
            Content = [new TextContentBlock { Text = errorMessage }],
            IsError = true
        });
    }

    /// <summary>
    /// Removes a tool from the mock client.
    /// </summary>
    /// <param name="name">The name of the tool to remove.</param>
    /// <returns>The current instance for method chaining.</returns>
    public MockMcpClientBuilder RemoveTool(string name)
    {
        _tools.Remove(name);
        return this;
    }

    /// <summary>
    /// Clears all registered tools.
    /// </summary>
    /// <returns>The current instance for method chaining.</returns>
    public MockMcpClientBuilder ClearTools()
    {
        _tools.Clear();
        return this;
    }

    /// <summary>
    /// Builds and returns a mock <see cref="IMcpClient"/> instance configured with the registered tools.
    /// </summary>
    /// <returns>A mock <see cref="IMcpClient"/> instance.</returns>
    public IMcpClient Build()
    {
        var mockClient = Substitute.For<IMcpClient>();

        // Setup tools/list response
        mockClient.SendRequestAsync(
            Arg.Is<JsonRpcRequest>(req => req.Method == "tools/list"),
            Arg.Any<CancellationToken>())
            .Returns(callInfo => HandleListToolsRequest(callInfo.Arg<JsonRpcRequest>()));

        // Setup tools/call response
        mockClient.SendRequestAsync(
            Arg.Is<JsonRpcRequest>(req => req.Method == "tools/call"),
            Arg.Any<CancellationToken>())
            .Returns(callInfo => HandleCallToolRequest(callInfo.Arg<JsonRpcRequest>()));

        return mockClient;
    }

    /// <summary>
    /// Handles the tools/list request and returns registered tools.
    /// </summary>
    private Task<JsonRpcResponse> HandleListToolsRequest(JsonRpcRequest request)
    {
        var tools = _tools.Values.Select(mockTool => mockTool.Tool).ToList();

        var result = new ListToolsResult { Tools = tools };
        var json = JsonSerializer.SerializeToNode(result, ServerJsonContext.Default.ListToolsResult);

        return Task.FromResult(new JsonRpcResponse
        {
            Id = request.Id,
            Result = json
        });
    }

    /// <summary>
    /// Handles the tools/call request and executes the registered tool.
    /// </summary>
    private Task<JsonRpcResponse> HandleCallToolRequest(JsonRpcRequest request)
    {
        try
        {
            var callParams = JsonSerializer.Deserialize<CallToolRequestParams>(request.Params!);

            if (callParams?.Name == null || !_tools.TryGetValue(callParams.Name, out var mockTool))
            {
                var errorResult = new CallToolResult
                {
                    Content = [new TextContentBlock { Text = $"Tool '{callParams?.Name ?? "null"}' not found" }],
                    IsError = true
                };

                var errorJson = JsonSerializer.SerializeToNode(errorResult);
                return Task.FromResult(new JsonRpcResponse
                {
                    Id = request.Id,
                    Result = errorJson
                });
            }

            // Convert JsonElement arguments to dictionary
            var arguments = callParams.Arguments?.ToDictionary(
                kvp => kvp.Key,
                kvp => ConvertJsonElementToObject(kvp.Value)
            );

            var result = mockTool.Handler(arguments);
            var resultJson = JsonSerializer.SerializeToNode(result);

            return Task.FromResult(new JsonRpcResponse
            {
                Id = request.Id,
                Result = resultJson
            });
        }
        catch (Exception ex)
        {
            var errorResult = new CallToolResult
            {
                Content = [new TextContentBlock { Text = $"Error calling tool: {ex.Message}" }],
                IsError = true
            };

            var errorJson = JsonSerializer.SerializeToNode(errorResult);
            return Task.FromResult(new JsonRpcResponse
            {
                Id = request.Id,
                Result = errorJson
            });
        }
    }

    /// <summary>
    /// Converts a JsonElement to an appropriate object type.
    /// </summary>
    private static object? ConvertJsonElementToObject(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number when element.TryGetInt32(out var intValue) => intValue,
            JsonValueKind.Number when element.TryGetInt64(out var longValue) => longValue,
            JsonValueKind.Number when element.TryGetDouble(out var doubleValue) => doubleValue,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element // Keep as JsonElement for complex types
        };
    }
}

/// <summary>
/// Internal representation of a mock tool.
/// </summary>
internal sealed class MockTool(
    Tool tool,
    Func<IReadOnlyDictionary<string, object?>?, CallToolResult> handler)
{
    public Tool Tool { get; } = tool;
    public Func<IReadOnlyDictionary<string, object?>?, CallToolResult> Handler { get; } = handler;
}
