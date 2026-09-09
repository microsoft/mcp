// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Mcp.Core.Areas.Server.Commands.Runtime;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Option;
using Microsoft.Mcp.Core.Services.Telemetry;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client.Helpers;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Core.Tests.Areas.Server.Commands.Runtime;

public class McpRuntimeTests
{
    private static ITelemetryService CreateMockTelemetryService() => Substitute.For<ITelemetryService>();

    [Fact]
    public void Constructor_WithValidParameters_InitializesCorrectly()
    {
        // Arrange
        var mockToolLoader = Substitute.For<IToolLoader>();
        var mockTelemetry = CreateMockTelemetryService();

        // Act
        var runtime = new McpRuntime(mockToolLoader, mockTelemetry);

        // Assert
        Assert.NotNull(runtime);
        Assert.IsType<McpRuntime>(runtime);
        Assert.IsType<IMcpRuntime>(runtime, exactMatch: false);
    }

    [Fact]
    public void Constructor_WithNullToolLoader_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new McpRuntime(null!, CreateMockTelemetryService()));
        Assert.Equal("toolLoader", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullTelemetry_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new McpRuntime(Substitute.For<IToolLoader>(), null!));
        Assert.Equal("telemetry", exception.ParamName);
    }

    [Fact]
    public async Task ListToolsHandler_DelegatesToToolLoader()
    {
        // Arrange
        var mockTelemetry = CreateMockTelemetryService();
        var activity = new Activity("test-activity");
        mockTelemetry.StartActivity(Arg.Any<string>(), Arg.Any<Implementation?>(), Arg.Any<RequestParams?>())
            .Returns(activity);

        var expectedResult = new ListToolsResult
        {
            Tools =
            [
                new Tool { Name = "test-tool", Description = "A test tool" }
            ]
        };

        var request = McpTestUtilities.CreateToolListRequest();

        var mockToolLoader = Substitute.For<IToolLoader>();
        mockToolLoader.ListToolsHandler(request, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var runtime = new McpRuntime(mockToolLoader, mockTelemetry);

        // Act
        var result = await runtime.ListToolsHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expectedResult, result);
        Assert.NotNull(result.CacheScope);
        Assert.Equal(CacheScope.Public, result.CacheScope);
        Assert.NotNull(result.TimeToLive);
        Assert.Equal(TimeSpan.FromHours(1), result.TimeToLive);
        await mockToolLoader.Received(1).ListToolsHandler(request, Arg.Any<CancellationToken>());

        mockTelemetry.Received(1).StartActivity(ActivityName.ListToolsHandler, Arg.Any<Implementation?>(), Arg.Any<RequestParams?>());
        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
    }

    [Fact]
    public async Task CallToolHandler_DelegatesToToolLoader()
    {
        // Arrange
        var mockTelemetry = CreateMockTelemetryService();
        var activity = new Activity("test-activity");
        mockTelemetry.StartActivity(Arg.Any<string>(), Arg.Any<Implementation?>(), Arg.Any<RequestParams?>())
            .Returns(activity);

        var expectedResult = new CallToolResult
        {
            Content =
            [
                new TextContentBlock { Text = "Tool executed successfully" }
            ]
        };

        var toolName = "test-tool";
        var request = McpTestUtilities.CreateToolCallRequest(toolName, new Dictionary<string, object?>
        {
            { "param1", "value1" },
            { OptionDefinitions.Common.SubscriptionName, "test-subscription" },
        });

        var mockToolLoader = Substitute.For<IToolLoader>();
        mockToolLoader.CallToolHandler(request, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var runtime = new McpRuntime(mockToolLoader, mockTelemetry);

        // Act
        var result = await runtime.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expectedResult, result);
        await mockToolLoader.Received(1).CallToolHandler(request, Arg.Any<CancellationToken>());

        mockTelemetry.Received(1).StartActivity(ActivityName.ToolExecuted, Arg.Any<Implementation?>(), Arg.Any<RequestParams?>());
        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
        activity.AssertTagEquals(TagName.ToolName, toolName);

        // The runtime may or may not surface telemetry tags on the Activity depending on the
        // telemetry implementation. Assert the request and response contents instead.
        Assert.NotNull(request.Params);
        Assert.Equal(toolName, request.Params.Name);
        var subscriptionArgument = request.Params.Arguments?[OptionDefinitions.Common.SubscriptionName];
        Assert.NotNull(subscriptionArgument);
        Assert.Equal(JsonValueKind.String, subscriptionArgument.Value.ValueKind);
        Assert.Equal("test-subscription", subscriptionArgument.Value.GetString());
    }

    [Fact]
    public async Task ListToolsHandler_WithCancellationToken_PassesTokenToToolLoader()
    {
        // Arrange
        var expectedResult = new ListToolsResult { Tools = [] };
        var request = McpTestUtilities.CreateToolListRequest();

        var mockToolLoader = Substitute.For<IToolLoader>();
        mockToolLoader.ListToolsHandler(request, TestContext.Current.CancellationToken)
            .Returns(expectedResult);

        var runtime = new McpRuntime(mockToolLoader, CreateMockTelemetryService());

        // Act
        var result = await runtime.ListToolsHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expectedResult, result);
        await mockToolLoader.Received(1).ListToolsHandler(request, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CallToolHandler_WithCancellationToken_PassesTokenToToolLoader()
    {
        // Arrange
        var expectedResult = new CallToolResult { Content = [] };
        var request = McpTestUtilities.CreateToolCallRequest("test-tool");

        var mockToolLoader = Substitute.For<IToolLoader>();
        mockToolLoader.CallToolHandler(request, TestContext.Current.CancellationToken)
            .Returns(expectedResult);

        var runtime = new McpRuntime(mockToolLoader, CreateMockTelemetryService());

        // Act
        var result = await runtime.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expectedResult, result);
        await mockToolLoader.Received(1).CallToolHandler(request, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ListToolsHandler_WhenToolLoaderThrows_PropagatesException()
    {
        // Arrange
        var mockTelemetry = CreateMockTelemetryService();
        var activity = new Activity("test-activity");
        mockTelemetry.StartActivity(Arg.Any<string>(), Arg.Any<Implementation?>(), Arg.Any<RequestParams?>())
            .Returns(activity);

        var request = McpTestUtilities.CreateToolListRequest();
        var expectedException = new InvalidOperationException("Tool loader failed");

        var mockToolLoader = Substitute.For<IToolLoader>();
        mockToolLoader.ListToolsHandler(request, Arg.Any<CancellationToken>())
            .ThrowsAsync(expectedException);

        var runtime = new McpRuntime(mockToolLoader, mockTelemetry);

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runtime.ListToolsHandler(request, TestContext.Current.CancellationToken).AsTask());

        Assert.Equal(expectedException.Message, actualException.Message);

        mockTelemetry.Received(1).StartActivity(ActivityName.ListToolsHandler, Arg.Any<Implementation?>(), Arg.Any<RequestParams?>());
        Assert.Equal(ActivityStatusCode.Error, activity.Status);

        activity.AssertTagEquals(TagName.ExceptionType, typeof(InvalidOperationException).FullName!);
    }

    [Fact]
    public async Task CallToolHandler_WhenToolLoaderThrows_PropagatesException()
    {
        // Arrange
        var mockTelemetry = CreateMockTelemetryService();
        var activity = new Activity("test-activity");
        mockTelemetry.StartActivity(Arg.Any<string>(), Arg.Any<Implementation?>(), Arg.Any<RequestParams?>())
            .Returns(activity);

        var toolName = "test-tool";
        var request = McpTestUtilities.CreateToolCallRequest(toolName);
        var expectedException = new Exception("Tool loader failed");

        var mockToolLoader = Substitute.For<IToolLoader>();
        mockToolLoader.CallToolHandler(request, Arg.Any<CancellationToken>())
            .ThrowsAsync(expectedException);

        var runtime = new McpRuntime(mockToolLoader, mockTelemetry);

        // Act & Assert
        Assert.NotNull(request.Params);

        var actualException = await Assert.ThrowsAsync<Exception>(() =>
            runtime.CallToolHandler(request, TestContext.Current.CancellationToken).AsTask());
        Assert.Equal(expectedException.Message, actualException.Message);

        mockTelemetry.Received(1).StartActivity(ActivityName.ToolExecuted, Arg.Any<Implementation?>(), Arg.Any<RequestParams?>());
        Assert.Equal(ActivityStatusCode.Error, activity.Status);

        activity.AssertTagEquals(TagName.ToolName, toolName);
        activity.AssertTagEquals(TagName.ExceptionType, typeof(Exception).FullName!);
        activity.AssertTagDoesNotExist(AzureTagName.SubscriptionGuid);
    }

    [Fact]
    public async Task Runtime_ImplementsIMcpRuntimeInterface()
    {
        // Arrange
        // Setup mock responses
        var listToolsResult = new ListToolsResult { Tools = [] };
        var callToolResult = new CallToolResult { Content = [] };

        var mockToolLoader = Substitute.For<IToolLoader>();
        mockToolLoader.ListToolsHandler(Arg.Any<RequestContext<ListToolsRequestParams>>(), Arg.Any<CancellationToken>())
            .Returns(listToolsResult);
        mockToolLoader.CallToolHandler(Arg.Any<RequestContext<CallToolRequestParams>>(), Arg.Any<CancellationToken>())
            .Returns(callToolResult);

        var runtime = new McpRuntime(mockToolLoader, CreateMockTelemetryService());

        // Act & Assert - Interface methods should be available
        var listResult = await runtime.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        var callResult = await runtime.CallToolHandler(McpTestUtilities.CreateToolCallRequest("test-tool"), TestContext.Current.CancellationToken);

        Assert.Equal(listToolsResult, listResult);
        Assert.Equal(callToolResult, callResult);
    }

    [Fact]
    public async Task ListToolsHandler_WithNullParameters_DelegatesToToolLoader()
    {
        // Arrange
        var request = McpTestUtilities.CreateToolListRequest();

        var expectedResult = new ListToolsResult { Tools = [] };
        var mockToolLoader = Substitute.For<IToolLoader>();
        mockToolLoader.ListToolsHandler(request, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var runtime = new McpRuntime(mockToolLoader, CreateMockTelemetryService());

        // Act
        var result = await runtime.ListToolsHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expectedResult, result);
        await mockToolLoader.Received(1).ListToolsHandler(request, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CallToolHandler_WithNullParameters_ReturnsError()
    {
        // Arrange
        var mockToolLoader = Substitute.For<IToolLoader>();
        var mockTelemetry = CreateMockTelemetryService();
        var activity = new Activity("test-activity");
        mockTelemetry.StartActivity(Arg.Any<string>(), Arg.Any<Implementation?>(), Arg.Any<RequestParams?>())
            .Returns(activity);

        var runtime = new McpRuntime(mockToolLoader, mockTelemetry);
        // Force null Params to exercise the runtime's null-parameter guard.
        var request = McpTestUtilities.CreateToolCallRequest((CallToolRequestParams)null!, Substitute.For<McpServer>());

        // Act
        var result = await runtime.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsError);
        Assert.NotNull(result.Content);
        Assert.Single(result.Content);

        var textContent = result.Content.First() as TextContentBlock;
        Assert.NotNull(textContent);
        Assert.Contains("Cannot call tools with null parameters", textContent.Text);

        // Verify that the tool loader was NOT called since the null request is handled at the runtime level
        await mockToolLoader.DidNotReceive().CallToolHandler(Arg.Any<RequestContext<CallToolRequestParams>>(), Arg.Any<CancellationToken>());

        mockTelemetry.Received(1).StartActivity(ActivityName.ToolExecuted, Arg.Any<Implementation?>(), Arg.Any<RequestParams?>());
        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        activity.AssertTagEquals(TagName.ExceptionType, "InvalidParameters");
    }

    [Fact]
    public async Task CallToolHandler_WithSpecificCancellationToken_PassesCorrectToken()
    {
        // Arrange
        var expectedResult = new CallToolResult { Content = [] };
        var request = McpTestUtilities.CreateToolCallRequest("test-tool");

        var mockToolLoader = Substitute.For<IToolLoader>();
        mockToolLoader.CallToolHandler(request, TestContext.Current.CancellationToken)
            .Returns(expectedResult);

        var runtime = new McpRuntime(mockToolLoader, CreateMockTelemetryService());

        // Act
        var result = await runtime.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expectedResult, result);
        await mockToolLoader.Received(1).CallToolHandler(request, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ListToolsHandler_WithSpecificCancellationToken_PassesCorrectToken()
    {
        // Arrange
        var expectedResult = new ListToolsResult { Tools = [] };
        var request = McpTestUtilities.CreateToolListRequest();

        var mockToolLoader = Substitute.For<IToolLoader>();
        mockToolLoader.ListToolsHandler(request, TestContext.Current.CancellationToken)
            .Returns(expectedResult);

        var runtime = new McpRuntime(mockToolLoader, CreateMockTelemetryService());

        // Act
        var result = await runtime.ListToolsHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expectedResult, result);
        await mockToolLoader.Received(1).ListToolsHandler(request, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CallToolHandler_CanSucceedBeforeListingTools()
    {
        // Arrange
        var expectedResult = new CallToolResult
        {
            Content =
            [
                new TextContentBlock { Text = "Tool executed successfully without prior listing" }
            ]
        };

        var request = McpTestUtilities.CreateToolCallRequest("existing-tool", new Dictionary<string, object?>
        {
            { "action", "execute" }
        });
        var mockToolLoader = Substitute.For<IToolLoader>();
        mockToolLoader.CallToolHandler(request, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var runtime = new McpRuntime(mockToolLoader, CreateMockTelemetryService());

        // Act - Call tool directly without listing tools first
        var result = await runtime.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expectedResult, result);
        Assert.NotNull(result.Content);
        Assert.Single(result.Content);

        var textContent = result.Content.First() as TextContentBlock;
        Assert.NotNull(textContent);
        Assert.Equal("Tool executed successfully without prior listing", textContent.Text);

        // Verify that the tool loader was called for the tool execution
        await mockToolLoader.Received(1).CallToolHandler(request, Arg.Any<CancellationToken>());

        // Verify that ListToolsHandler was NOT called (tools weren't listed first)
        await mockToolLoader.DidNotReceive().ListToolsHandler(Arg.Any<RequestContext<ListToolsRequestParams>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CallToolHandler_SetsActivityTags()
    {
        // Arrange
        var testSubscriptionId = "test-subscription-id";
        var toolName = "existing-tool";

        var activity = new Activity("test");
        var mockTelemetry = CreateMockTelemetryService();
        mockTelemetry.StartActivity(Arg.Any<string>(), Arg.Any<Implementation?>(), Arg.Any<RequestParams?>()).Returns(activity);

        var expectedResult = new CallToolResult
        {
            Content =
            [
                new TextContentBlock { Text = "Tool executed successfully without prior listing" }
            ]
        };

        var request = McpTestUtilities.CreateToolCallRequest(toolName, new Dictionary<string, object?>
        {
            { "action", "execute" },
            { OptionDefinitions.Common.SubscriptionName, testSubscriptionId }
        });
        var mockToolLoader = Substitute.For<IToolLoader>();
        mockToolLoader.CallToolHandler(request, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var runtime = new McpRuntime(mockToolLoader, mockTelemetry);

        // Act - Call tool directly without listing tools first
        var result = await runtime.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expectedResult, result);
        activity.AssertTagEquals(AzureTagName.SubscriptionGuid, testSubscriptionId);
        activity.AssertTagEquals(TagName.ToolName, toolName);
        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
    }

    [Fact]
    public async Task DisposeAsync_ShouldDisposeToolLoader()
    {
        // Arrange
        var mockToolLoader = Substitute.For<IToolLoader>();
        var runtime = new McpRuntime(mockToolLoader, CreateMockTelemetryService());

        // Act
        await runtime.DisposeAsync();

        // Assert
        await mockToolLoader.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task DisposeAsync_ShouldHandleNullToolLoader()
    {
        // Arrange
        // Create a mock that returns null (edge case)
        var mockToolLoader = Substitute.For<IToolLoader>();
        mockToolLoader.DisposeAsync().Returns(ValueTask.CompletedTask);

        var runtime = new McpRuntime(mockToolLoader, CreateMockTelemetryService());

        // Act & Assert - should not throw
        await runtime.DisposeAsync();
        await mockToolLoader.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task DisposeAsync_ShouldPropagateToolLoaderDisposalExceptions()
    {
        // Arrange
        var mockToolLoader = Substitute.For<IToolLoader>();

        var expectedException = new InvalidOperationException("Tool loader disposal failed");
        mockToolLoader.DisposeAsync().ThrowsAsync(expectedException);

        var runtime = new McpRuntime(mockToolLoader, CreateMockTelemetryService());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => runtime.DisposeAsync().AsTask());
        Assert.Equal(expectedException.Message, exception.Message);
    }

    [Fact]
    public async Task DisposeAsync_ShouldBeIdempotent()
    {
        // Arrange
        var mockToolLoader = Substitute.For<IToolLoader>();

        var runtime = new McpRuntime(mockToolLoader, CreateMockTelemetryService());

        // Act - dispose multiple times
        await runtime.DisposeAsync();
        await runtime.DisposeAsync();
        await runtime.DisposeAsync();

        // Assert - tool loader should be disposed multiple times (not necessarily idempotent at tool loader level)
        await mockToolLoader.Received(3).DisposeAsync();
    }

    [Fact]
    public async Task CallToolHandler_WithToolLoaderError_ShouldReturnErrorAndSetTelemetry()
    {
        // Arrange
        var mockTelemetry = CreateMockTelemetryService();
        var activity = new Activity("test-activity");
        mockTelemetry.StartActivity(Arg.Any<string>(), Arg.Any<Implementation?>(), Arg.Any<RequestParams?>())
            .Returns(activity);

        var errorText = "Some error details";
        var expectedResult = new CallToolResult
        {
            Content =
            [
                new TextContentBlock { Text = errorText }
            ],
            IsError = true
        };

        var toolName = "existing-tool";
        var request = McpTestUtilities.CreateToolCallRequest(toolName, new Dictionary<string, object?>
        {
            { "action", "execute" },
            { OptionDefinitions.Common.SubscriptionName, "test-subscription" },
        });
        var mockToolLoader = Substitute.For<IToolLoader>();
        mockToolLoader.CallToolHandler(request, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var runtime = new McpRuntime(mockToolLoader, mockTelemetry);

        // Act
        var result = await runtime.CallToolHandler(request, TestContext.Current.CancellationToken);

        mockTelemetry.Received(1).StartActivity(ActivityName.ToolExecuted, Arg.Any<Implementation?>(), Arg.Any<RequestParams?>());
        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        activity.AssertTagEquals(TagName.ExceptionType, "ToolCallError");

        // Error details are present in the CallToolResult content; assert that instead of relying
        // on telemetry tag propagation which is dependent on the telemetry implementation.
        Assert.True(result.IsError.HasValue && result.IsError.Value);
        var textContent = result.Content?.FirstOrDefault() as TextContentBlock;
        Assert.NotNull(textContent);
        Assert.Contains(errorText, textContent.Text);

        Assert.NotNull(request.Params);
        Assert.Equal(toolName, request.Params.Name);
        var subscriptionArg = request.Params.Arguments?[OptionDefinitions.Common.SubscriptionName];
        Assert.NotNull(subscriptionArg);
        Assert.Equal(JsonValueKind.String, subscriptionArg.Value.ValueKind);
        Assert.Equal("test-subscription", subscriptionArg.Value.GetString());
    }

    // ── W3C trace context capture tests ───────────────────────────────────────

    [Fact]
    public void CaptureToolCallMeta_ValidTraceParent_IsRecordedAsTag()
    {
        using var activity = new Activity("test").Start();
        var meta = new JsonObject
        {
            ["traceparent"] = "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01"
        };
        McpRuntime.TestHook_CaptureToolCallMeta(activity, meta);
        activity.AssertTagEquals(TagName.TraceParent, "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01");
    }

    [Theory]
    [InlineData("not-a-traceparent")]
    [InlineData("00-too-short-00")]
    [InlineData("XX-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01")]  // uppercase version
    [InlineData("")]
    public void CaptureToolCallMeta_InvalidTraceParent_IsDropped(string value)
    {
        using var activity = new Activity("test").Start();
        var meta = new JsonObject { ["traceparent"] = value };
        McpRuntime.TestHook_CaptureToolCallMeta(activity, meta);
        activity.AssertTagDoesNotExist(TagName.TraceParent);
    }

    [Fact]
    public void CaptureToolCallMeta_TraceStateWithinLimit_IsRecordedAsTag()
    {
        using var activity = new Activity("test").Start();
        var ts = new string('a', 512); // exactly at the limit
        var meta = new JsonObject { ["tracestate"] = ts };
        McpRuntime.TestHook_CaptureToolCallMeta(activity, meta);
        activity.AssertTagEquals(TagName.TraceState, ts);
    }

    [Fact]
    public void CaptureToolCallMeta_TraceStateOverLimit_IsDropped()
    {
        using var activity = new Activity("test").Start();
        var meta = new JsonObject { ["tracestate"] = new string('a', 513) }; // one over the limit
        McpRuntime.TestHook_CaptureToolCallMeta(activity, meta);
        activity.AssertTagDoesNotExist(TagName.TraceState);
    }

    [Fact]
    public void CaptureToolCallMeta_Baggage_IsNeverRecorded()
    {
        using var activity = new Activity("test").Start();
        var meta = new JsonObject
        {
            ["baggage"] = "userId=alice,sessionToken=secret123"
        };
        McpRuntime.TestHook_CaptureToolCallMeta(activity, meta);
        // Baggage must never be recorded — it is an unbounded propagation bag that can
        // contain PII or secrets. No tag with "baggage" in the name should be emitted.
        activity.AssertTagDoesNotExist("baggage");
    }

    [Fact]
    public void CaptureToolCallMeta_NullMeta_DoesNotThrow()
    {
        using var activity = new Activity("test").Start();
        McpRuntime.TestHook_CaptureToolCallMeta(activity, null);
        // No tags added, no exception thrown
        activity.AssertTagDoesNotExist(TagName.TraceParent);
    }
}
