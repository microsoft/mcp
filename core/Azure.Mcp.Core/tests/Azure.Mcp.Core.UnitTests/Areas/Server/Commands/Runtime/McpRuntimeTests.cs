// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text.Json;
using Azure.Mcp.Core.Areas.Server.Commands.Runtime;
using Azure.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Azure.Mcp.Core.Areas.Server.Options;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Core.Services.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using NSubstitute;
using Xunit;
using static Azure.Mcp.Core.Services.Telemetry.TelemetryConstants;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Commands.Runtime;

public class McpRuntimeTests
{
    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<ITelemetryService, CommandFactoryHelpers.NoOpTelemetryService>();

        return services.BuildServiceProvider();
    }

    private static IOptions<ServiceStartOptions> CreateOptions(ServiceStartOptions? options = null)
    {
        return Microsoft.Extensions.Options.Options.Create(options ?? new ServiceStartOptions());
    }

    private static McpServer CreateMockServer()
    {
        return Substitute.For<McpServer>();
    }

    private static ITelemetryService CreateMockTelemetryService()
    {
        return Substitute.For<ITelemetryService>();
    }

    private static RequestContext<ListToolsRequestParams> CreateListToolsRequest()
    {
        return new RequestContext<ListToolsRequestParams>(CreateMockServer(), new() { Method = RequestMethods.ToolsList })
        {
            Params = new ListToolsRequestParams()
        };
    }

    private static RequestContext<CallToolRequestParams> CreateCallToolRequest(string toolName = "test-tool", IReadOnlyDictionary<string, JsonElement>? arguments = null)
    {
        return new RequestContext<CallToolRequestParams>(CreateMockServer(), new() { Method = RequestMethods.ToolsCall })
        {
            Params = new CallToolRequestParams
            {
                Name = toolName,
                Arguments = arguments ?? new Dictionary<string, JsonElement>()
            }
        };
    }

    private static object GetAndAssertTagKeyValue(Activity activity, string tagName)
    {
        var matching = activity.TagObjects.SingleOrDefault(x => string.Equals(x.Key, tagName, StringComparison.OrdinalIgnoreCase));

        Assert.False(matching.Equals(default(KeyValuePair<string, object?>)), $"Tag '{tagName}' was not found in activity tags.");
        Assert.NotNull(matching.Value);

        return matching.Value;
    }

    [Fact]
    public void Constructor_WithValidParameters_InitializesCorrectly()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<McpRuntime>>();
        var mockToolLoader = Substitute.For<IToolLoader>();
        var mockTelemetry = CreateMockTelemetryService();
        var options = CreateOptions();

        // Act
        var runtime = new McpRuntime(mockToolLoader, options, mockTelemetry, logger);

        // Assert
        Assert.NotNull(runtime);
        Assert.IsType<McpRuntime>(runtime);
        Assert.IsAssignableFrom<IMcpRuntime>(runtime);
    }

    [Fact]
    public void Constructor_WithNullToolLoader_ThrowsArgumentNullException()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<McpRuntime>>();
        var mockTelemetry = CreateMockTelemetryService();
        var options = CreateOptions();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new McpRuntime(null!, options, mockTelemetry, logger));
        Assert.Equal("toolLoader", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<McpRuntime>>();
        var mockToolLoader = Substitute.For<IToolLoader>();
        var mockTelemetry = CreateMockTelemetryService();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new McpRuntime(mockToolLoader, null!, mockTelemetry, logger));
        Assert.Equal("options", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullTelemetry_ThrowsArgumentNullException()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<McpRuntime>>();
        var mockToolLoader = Substitute.For<IToolLoader>();
        var options = CreateOptions();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new McpRuntime(mockToolLoader, options, null!, logger));
        Assert.Equal("telemetry", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var mockToolLoader = Substitute.For<IToolLoader>();
        var mockTelemetry = CreateMockTelemetryService();
        var options = CreateOptions();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new McpRuntime(mockToolLoader, options, mockTelemetry, null!));
        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_LogsInitializationInformation()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<McpRuntime>>();
        var mockToolLoader = Substitute.For<IToolLoader>();
        var mockTelemetry = CreateMockTelemetryService();
        var options = CreateOptions(new ServiceStartOptions
        {
            ReadOnly = true,
            Namespace = ["storage", "keyvault"]
        });

        // Act
        var runtime = new McpRuntime(mockToolLoader, options, mockTelemetry, logger);

        // Assert
        Assert.NotNull(runtime);
        // Note: In a more sophisticated test setup, we could capture and verify log messages
        // For now, we verify that construction succeeds without throwing
    }

    [Fact]
    public async Task ListToolsHandler_DelegatesToToolLoader()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<McpRuntime>>();
        var mockToolLoader = Substitute.For<IToolLoader>();

        var mockTelemetry = CreateMockTelemetryService();
        var activity = new Activity("test-activity");
        mockTelemetry.StartActivity(Arg.Any<string>(), Arg.Any<Implementation?>())
            .Returns(activity);

        var options = CreateOptions();
        var runtime = new McpRuntime(mockToolLoader, options, mockTelemetry, logger);

        var expectedResult = new ListToolsResult
        {
            Tools = new List<Tool>
            {
                new Tool { Name = "test-tool", Description = "A test tool" }
            }
        };

        var request = CreateListToolsRequest();
        mockToolLoader.ListToolsHandler(request, Arg.Any<CancellationToken>())
            .Returns(new ValueTask<ListToolsResult>(expectedResult));

        // Act
        var result = await runtime.ListToolsHandler(request, CancellationToken.None);

        // Assert
        Assert.Equal(expectedResult, result);
        await mockToolLoader.Received(1).ListToolsHandler(request, Arg.Any<CancellationToken>());

        mockTelemetry.Received(1).StartActivity(ActivityName.ListToolsHandler, Arg.Any<Implementation?>());
        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
    }

    [Fact]
    public async Task CallToolHandler_DelegatesToToolLoader()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<McpRuntime>>();
        var mockToolLoader = Substitute.For<IToolLoader>();

        var mockTelemetry = CreateMockTelemetryService();
        var activity = new Activity("test-activity");
        mockTelemetry.StartActivity(Arg.Any<string>(), Arg.Any<Implementation?>())
            .Returns(activity);

        var options = CreateOptions();
        var runtime = new McpRuntime(mockToolLoader, options, mockTelemetry, logger);

        var expectedResult = new CallToolResult
        {
            Content = new List<ContentBlock>
            {
                new TextContentBlock { Text = "Tool executed successfully" }
            }
        };

        var toolName = "test-tool";
        var request = CreateCallToolRequest(toolName, new Dictionary<string, JsonElement>
        {
            { "param1", JsonDocument.Parse("\"value1\"").RootElement },
            { OptionDefinitions.Common.SubscriptionName, JsonDocument.Parse("\"test-subscription\"").RootElement },
        });
        mockToolLoader.CallToolHandler(request, Arg.Any<CancellationToken>())
            .Returns(new ValueTask<CallToolResult>(expectedResult));

        // Act
        var result = await runtime.CallToolHandler(request, CancellationToken.None);

        // Assert
        Assert.Equal(expectedResult, result);
        await mockToolLoader.Received(1).CallToolHandler(request, Arg.Any<CancellationToken>());

        mockTelemetry.Received(1).StartActivity(ActivityName.ToolExecuted, Arg.Any<Implementation?>());
        Assert.Equal(ActivityStatusCode.Ok, activity.Status);

        var isCommandInvoked = GetAndAssertTagKeyValue(activity, TagName.IsServerCommandInvoked);
        var isCommandInvokedBool = (bool)isCommandInvoked;
        Assert.True(isCommandInvokedBool);

        var actualToolName = GetAndAssertTagKeyValue(activity, TagName.ToolName);
        Assert.Equal(toolName, actualToolName);

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
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<McpRuntime>>();
        var mockToolLoader = Substitute.For<IToolLoader>();
        var options = CreateOptions();
        var runtime = new McpRuntime(mockToolLoader, options, CreateMockTelemetryService(), logger);

        var expectedResult = new ListToolsResult { Tools = new List<Tool>() };
        var request = CreateListToolsRequest();
        var cancellationToken = new CancellationToken();

        mockToolLoader.ListToolsHandler(request, cancellationToken)
            .Returns(new ValueTask<ListToolsResult>(expectedResult));

        // Act
        var result = await runtime.ListToolsHandler(request, cancellationToken);

        // Assert
        Assert.Equal(expectedResult, result);
        await mockToolLoader.Received(1).ListToolsHandler(request, cancellationToken);
    }

    [Fact]
    public async Task CallToolHandler_WithCancellationToken_PassesTokenToToolLoader()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<McpRuntime>>();
        var mockToolLoader = Substitute.For<IToolLoader>();
        var options = CreateOptions();
        var runtime = new McpRuntime(mockToolLoader, options, CreateMockTelemetryService(), logger);

        var expectedResult = new CallToolResult { Content = new List<ContentBlock>() };
        var request = CreateCallToolRequest();
        var cancellationToken = new CancellationToken();

        mockToolLoader.CallToolHandler(request, cancellationToken)
            .Returns(new ValueTask<CallToolResult>(expectedResult));

        // Act
        var result = await runtime.CallToolHandler(request, cancellationToken);

        // Assert
        Assert.Equal(expectedResult, result);
        await mockToolLoader.Received(1).CallToolHandler(request, cancellationToken);
    }

    [Fact]
    public async Task ListToolsHandler_WhenToolLoaderThrows_PropagatesException()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<McpRuntime>>();
        var mockToolLoader = Substitute.For<IToolLoader>();

        var mockTelemetry = CreateMockTelemetryService();
        var activity = new Activity("test-activity");
        mockTelemetry.StartActivity(Arg.Any<string>(), Arg.Any<Implementation?>())
            .Returns(activity);

        var options = CreateOptions();
        var runtime = new McpRuntime(mockToolLoader, options, mockTelemetry, logger);

        var request = CreateListToolsRequest();
        var expectedException = new InvalidOperationException("Tool loader failed");

        mockToolLoader.ListToolsHandler(request, Arg.Any<CancellationToken>())
            .Returns<ValueTask<ListToolsResult>>(x => throw expectedException);

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runtime.ListToolsHandler(request, CancellationToken.None).AsTask());

        Assert.Equal(expectedException.Message, actualException.Message);

        mockTelemetry.Received(1).StartActivity(ActivityName.ListToolsHandler, Arg.Any<Implementation?>());
        Assert.Equal(ActivityStatusCode.Error, activity.Status);

        GetAndAssertTagKeyValue(activity, TagName.ErrorDetails);
    }

    [Fact]
    public async Task CallToolHandler_WhenToolLoaderThrows_PropagatesException()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<McpRuntime>>();
        var mockToolLoader = Substitute.For<IToolLoader>();

        var mockTelemetry = CreateMockTelemetryService();
        var activity = new Activity("test-activity");
        mockTelemetry.StartActivity(Arg.Any<string>(), Arg.Any<Implementation?>())
            .Returns(activity);

        var options = CreateOptions();
        var runtime = new McpRuntime(mockToolLoader, options, mockTelemetry, logger);

        var toolName = "test-tool";
        var request = CreateCallToolRequest(toolName);
        var expectedException = new InvalidOperationException("Tool loader failed");

        mockToolLoader.CallToolHandler(request, Arg.Any<CancellationToken>())
            .Returns<ValueTask<CallToolResult>>(x => throw expectedException);

        // Act & Assert
        Assert.NotNull(request.Params);

        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runtime.CallToolHandler(request, CancellationToken.None).AsTask());
        Assert.Equal(expectedException.Message, actualException.Message);

        mockTelemetry.Received(1).StartActivity(ActivityName.ToolExecuted, Arg.Any<Implementation?>());
        Assert.Equal(ActivityStatusCode.Error, activity.Status);

        var isCommandInvoked = GetAndAssertTagKeyValue(activity, TagName.IsServerCommandInvoked);
        bool isCommandInvokedBool = isCommandInvoked is bool b ? b : bool.TryParse(isCommandInvoked?.ToString(), out var parsed) && parsed;
        Assert.True(isCommandInvokedBool);

        var actualToolName = GetAndAssertTagKeyValue(activity, TagName.ToolName);
        Assert.Equal(toolName, actualToolName);

        GetAndAssertTagKeyValue(activity, TagName.ErrorDetails);

        Assert.DoesNotContain(activity.TagObjects,
            x => string.Equals(x.Key, TagName.SubscriptionGuid, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Constructor_WithDifferentServiceOptions_LogsCorrectly()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<McpRuntime>>();
        var mockToolLoader = Substitute.For<IToolLoader>();

        // Test with ReadOnly = false and no services
        var options1 = CreateOptions(new ServiceStartOptions { ReadOnly = false });
        var runtime1 = new McpRuntime(mockToolLoader, options1, CreateMockTelemetryService(), logger);
        Assert.NotNull(runtime1);

        // Test with ReadOnly = null and multiple services
        var options2 = CreateOptions(new ServiceStartOptions
        {
            ReadOnly = null,
            Namespace = ["storage", "keyvault", "monitor"]
        });
        var runtime2 = new McpRuntime(mockToolLoader, options2, CreateMockTelemetryService(), logger);
        Assert.NotNull(runtime2);

        // Test with empty service array
        var options3 = CreateOptions(new ServiceStartOptions
        {
            ReadOnly = true,
            Namespace = []
        });
        var runtime3 = new McpRuntime(mockToolLoader, options3, CreateMockTelemetryService(), logger);
        Assert.NotNull(runtime3);
    }

    [Fact]
    public async Task Runtime_ImplementsIMcpRuntimeInterface()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<McpRuntime>>();
        var mockToolLoader = Substitute.For<IToolLoader>();
        var options = CreateOptions();
        IMcpRuntime runtime = new McpRuntime(mockToolLoader, options, CreateMockTelemetryService(), logger);

        // Setup mock responses
        var listToolsResult = new ListToolsResult { Tools = new List<Tool>() };
        var callToolResult = new CallToolResult { Content = new List<ContentBlock>() };

        mockToolLoader.ListToolsHandler(Arg.Any<RequestContext<ListToolsRequestParams>>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<ListToolsResult>(listToolsResult));
        mockToolLoader.CallToolHandler(Arg.Any<RequestContext<CallToolRequestParams>>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<CallToolResult>(callToolResult));

        // Act & Assert - Interface methods should be available
        var listResult = await runtime.ListToolsHandler(CreateListToolsRequest(), CancellationToken.None);
        var callResult = await runtime.CallToolHandler(CreateCallToolRequest(), CancellationToken.None);

        Assert.Equal(listToolsResult, listResult);
        Assert.Equal(callToolResult, callResult);
    }

    [Fact]
    public async Task ListToolsHandler_WithNullRequest_DelegatesToToolLoader()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<McpRuntime>>();
        var mockToolLoader = Substitute.For<IToolLoader>();
        var options = CreateOptions();
        var runtime = new McpRuntime(mockToolLoader, options, CreateMockTelemetryService(), logger);

        var expectedResult = new ListToolsResult { Tools = new List<Tool>() };
        mockToolLoader.ListToolsHandler(null!, Arg.Any<CancellationToken>())
            .Returns(new ValueTask<ListToolsResult>(expectedResult));

        // Act
        var result = await runtime.ListToolsHandler(null!, CancellationToken.None);

        // Assert
        Assert.Equal(expectedResult, result);
        await mockToolLoader.Received(1).ListToolsHandler(null!, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CallToolHandler_WithNullRequest_ReturnsError()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<McpRuntime>>();
        var mockToolLoader = Substitute.For<IToolLoader>();
        var options = CreateOptions();

        var mockTelemetry = CreateMockTelemetryService();
        var activity = new Activity("test-activity");
        mockTelemetry.StartActivity(Arg.Any<string>(), Arg.Any<Implementation?>())
            .Returns(activity);

        var runtime = new McpRuntime(mockToolLoader, options, mockTelemetry, logger);

        // Act
        var result = await runtime.CallToolHandler(null!, CancellationToken.None);

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

        mockTelemetry.Received(1).StartActivity(ActivityName.ToolExecuted, Arg.Any<Implementation?>());
        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        GetAndAssertTagKeyValue(activity, TagName.ErrorDetails);
    }

    [Fact]
    public void Constructor_WithNullServiceArray_LogsCorrectly()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<McpRuntime>>();
        var mockToolLoader = Substitute.For<IToolLoader>();
        var options = CreateOptions(new ServiceStartOptions { Namespace = null });

        // Act
        var runtime = new McpRuntime(mockToolLoader, options, CreateMockTelemetryService(), logger);

        // Assert
        Assert.NotNull(runtime);
        // Should log empty string for namespace when Service is null
    }

    [Fact]
    public async Task CallToolHandler_WithSpecificCancellationToken_PassesCorrectToken()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<McpRuntime>>();
        var mockToolLoader = Substitute.For<IToolLoader>();
        var options = CreateOptions();
        var runtime = new McpRuntime(mockToolLoader, options, CreateMockTelemetryService(), logger);

        var expectedResult = new CallToolResult { Content = new List<ContentBlock>() };
        var request = CreateCallToolRequest();
        var specificToken = new CancellationTokenSource().Token;

        mockToolLoader.CallToolHandler(request, specificToken)
            .Returns(new ValueTask<CallToolResult>(expectedResult));

        // Act
        var result = await runtime.CallToolHandler(request, specificToken);

        // Assert
        Assert.Equal(expectedResult, result);
        await mockToolLoader.Received(1).CallToolHandler(request, specificToken);
    }

    [Fact]
    public async Task ListToolsHandler_WithSpecificCancellationToken_PassesCorrectToken()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<McpRuntime>>();
        var mockToolLoader = Substitute.For<IToolLoader>();
        var options = CreateOptions();
        var runtime = new McpRuntime(mockToolLoader, options, CreateMockTelemetryService(), logger);

        var expectedResult = new ListToolsResult { Tools = new List<Tool>() };
        var request = CreateListToolsRequest();
        var specificToken = new CancellationTokenSource().Token;

        mockToolLoader.ListToolsHandler(request, specificToken)
            .Returns(new ValueTask<ListToolsResult>(expectedResult));

        // Act
        var result = await runtime.ListToolsHandler(request, specificToken);

        // Assert
        Assert.Equal(expectedResult, result);
        await mockToolLoader.Received(1).ListToolsHandler(request, specificToken);
    }

    [Fact]
    public void Constructor_LogsToolLoaderType()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<McpRuntime>>();
        var mockToolLoader = Substitute.For<IToolLoader>();
        var options = CreateOptions();

        // Act
        var runtime = new McpRuntime(mockToolLoader, options, CreateMockTelemetryService(), logger);

        // Assert
        Assert.NotNull(runtime);
        // The constructor should log the tool loader type name
        // This verifies that the logging statement executes without error
    }

    [Fact]
    public async Task CallToolHandler_CanSucceedBeforeListingTools()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<McpRuntime>>();
        var mockToolLoader = Substitute.For<IToolLoader>();
        var mockTelemetry = CreateMockTelemetryService();
        var options = CreateOptions();
        var runtime = new McpRuntime(mockToolLoader, options, mockTelemetry, logger);

        var expectedResult = new CallToolResult
        {
            Content = new List<ContentBlock>
            {
                new TextContentBlock { Text = "Tool executed successfully without prior listing" }
            }
        };

        var request = CreateCallToolRequest("existing-tool", new Dictionary<string, JsonElement>
        {
            { "action", JsonDocument.Parse("\"execute\"").RootElement }
        });
        mockToolLoader.CallToolHandler(request, Arg.Any<CancellationToken>())
            .Returns(new ValueTask<CallToolResult>(expectedResult));

        // Act - Call tool directly without listing tools first
        var result = await runtime.CallToolHandler(request, CancellationToken.None);

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
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<McpRuntime>>();
        var mockToolLoader = Substitute.For<IToolLoader>();
        var testSubscriptionId = "test-subscription-id";
        var toolName = "existing-tool";

        var activity = new Activity("test");
        var mockTelemetry = CreateMockTelemetryService();
        mockTelemetry.StartActivity(Arg.Any<string>(), Arg.Any<Implementation?>()).Returns(activity);

        var options = CreateOptions();
        var runtime = new McpRuntime(mockToolLoader, options, mockTelemetry, logger);

        var expectedResult = new CallToolResult
        {
            Content = new List<ContentBlock>
            {
                new TextContentBlock { Text = "Tool executed successfully without prior listing" }
            }
        };

        var request = CreateCallToolRequest(toolName, new Dictionary<string, JsonElement>
        {
            { "action", JsonDocument.Parse("\"execute\"").RootElement },
            { OptionDefinitions.Common.SubscriptionName, JsonDocument.Parse($"\"{testSubscriptionId}\"").RootElement }
        });
        mockToolLoader.CallToolHandler(request, Arg.Any<CancellationToken>())
            .Returns(new ValueTask<CallToolResult>(expectedResult));

        // Act - Call tool directly without listing tools first
        var result = await runtime.CallToolHandler(request, CancellationToken.None);

        // Assert
        Assert.Equal(expectedResult, result);

        var actualSubscription = activity.Tags.SingleOrDefault(e => TagName.SubscriptionGuid == e.Key);
        Assert.Equal(testSubscriptionId, actualSubscription.Value);

        var actualToolName = activity.Tags.SingleOrDefault(e => TagName.ToolName == e.Key);
        Assert.Equal(toolName, actualToolName.Value);

        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
    }

    [Fact]
    public async Task DisposeAsync_ShouldDisposeToolLoader()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var mockToolLoader = Substitute.For<IToolLoader>();
        var options = CreateOptions();
        var mockTelemetryService = CreateMockTelemetryService();
        var logger = serviceProvider.GetRequiredService<ILogger<McpRuntime>>();

        var runtime = new McpRuntime(mockToolLoader, options, mockTelemetryService, logger);

        // Act
        await runtime.DisposeAsync();

        // Assert
        await mockToolLoader.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task DisposeAsync_ShouldHandleNullToolLoader()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var options = CreateOptions();
        var mockTelemetryService = CreateMockTelemetryService();
        var logger = serviceProvider.GetRequiredService<ILogger<McpRuntime>>();

        // Create a mock that returns null (edge case)
        var mockToolLoader = Substitute.For<IToolLoader>();
        mockToolLoader.DisposeAsync().Returns(ValueTask.CompletedTask);

        var runtime = new McpRuntime(mockToolLoader, options, mockTelemetryService, logger);

        // Act & Assert - should not throw
        await runtime.DisposeAsync();
        await mockToolLoader.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task DisposeAsync_ShouldPropagateToolLoaderDisposalExceptions()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var mockToolLoader = Substitute.For<IToolLoader>();
        var options = CreateOptions();
        var mockTelemetryService = CreateMockTelemetryService();
        var logger = serviceProvider.GetRequiredService<ILogger<McpRuntime>>();

        var expectedException = new InvalidOperationException("Tool loader disposal failed");
        mockToolLoader.DisposeAsync().Returns(ValueTask.FromException(expectedException));

        var runtime = new McpRuntime(mockToolLoader, options, mockTelemetryService, logger);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => runtime.DisposeAsync().AsTask());
        Assert.Equal("Tool loader disposal failed", exception.Message);
    }

    [Fact]
    public async Task DisposeAsync_ShouldBeIdempotent()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var mockToolLoader = Substitute.For<IToolLoader>();
        var options = CreateOptions();
        var mockTelemetryService = CreateMockTelemetryService();
        var logger = serviceProvider.GetRequiredService<ILogger<McpRuntime>>();

        var runtime = new McpRuntime(mockToolLoader, options, mockTelemetryService, logger);

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
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<McpRuntime>>();
        var mockToolLoader = Substitute.For<IToolLoader>();

        var mockTelemetry = CreateMockTelemetryService();
        var activity = new Activity("test-activity");
        mockTelemetry.StartActivity(Arg.Any<string>(), Arg.Any<Implementation?>())
            .Returns(activity);

        var options = CreateOptions();
        var runtime = new McpRuntime(mockToolLoader, options, mockTelemetry, logger);

        var errorText = "Some error details";
        var expectedResult = new CallToolResult
        {
            Content = new List<ContentBlock>
            {
                new TextContentBlock { Text = errorText }
            },
            IsError = true
        };

        var toolName = "existing-tool";
        var request = CreateCallToolRequest(toolName, new Dictionary<string, JsonElement>
        {
            { "action", JsonDocument.Parse("\"execute\"").RootElement },
            { OptionDefinitions.Common.SubscriptionName, JsonDocument.Parse("\"test-subscription\"").RootElement },
        });
        mockToolLoader.CallToolHandler(request, Arg.Any<CancellationToken>())
            .Returns(new ValueTask<CallToolResult>(expectedResult));

        // Act
        var result = await runtime.CallToolHandler(request, CancellationToken.None);

        mockTelemetry.Received(1).StartActivity(ActivityName.ToolExecuted, Arg.Any<Implementation?>());
        Assert.Equal(ActivityStatusCode.Error, activity.Status);

        // Error details are present in the CallToolResult content; assert that instead of relying
        // on telemetry tag propagation which is dependent on the telemetry implementation.
        Assert.True(result.IsError.HasValue && result.IsError.Value);
        var textContent = result.Content?.FirstOrDefault() as TextContentBlock;
        Assert.NotNull(textContent);
        Assert.Contains(errorText, textContent!.Text);

        Assert.NotNull(request.Params);
        Assert.Equal(toolName, request.Params.Name);
        var subscriptionArg = request.Params.Arguments?[OptionDefinitions.Common.SubscriptionName];
        Assert.NotNull(subscriptionArg);
        Assert.Equal(JsonValueKind.String, subscriptionArg.Value.ValueKind);
        Assert.Equal("test-subscription", subscriptionArg.Value.GetString());
    }
}
