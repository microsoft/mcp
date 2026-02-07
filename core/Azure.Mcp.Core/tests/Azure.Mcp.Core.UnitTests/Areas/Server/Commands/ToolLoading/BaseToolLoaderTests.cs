// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Commands.ToolLoading;

public class BaseToolLoaderTests
{
    [Fact]
    public void CreateClientOptions_WithNoCapabilities_ReturnsOptionsWithNoCapabilities()
    {
        // Arrange
        var loader = new TestableBaseToolLoader(NullLogger.Instance);
        var mockServer = Substitute.For<McpServer>();
        mockServer.ClientCapabilities.Returns((ClientCapabilities?)null);

        // Act
        var options = loader.CreateClientOptionsPublic(mockServer);

        // Assert
        Assert.NotNull(options);
        Assert.NotNull(options.Handlers);
        Assert.Null(options.Handlers.SamplingHandler);
        Assert.Null(options.Handlers.ElicitationHandler);
    }

    [Fact]
    public void CreateClientOptions_WithEmptyCapabilities_ReturnsOptionsWithNoCapabilities()
    {
        // Arrange
        var loader = new TestableBaseToolLoader(NullLogger.Instance);
        var mockServer = Substitute.For<McpServer>();
        mockServer.ClientCapabilities.Returns(new ClientCapabilities());

        // Act
        var options = loader.CreateClientOptionsPublic(mockServer);

        // Assert
        Assert.NotNull(options);
        Assert.NotNull(options.Handlers);
        Assert.Null(options.Handlers.SamplingHandler);
        Assert.Null(options.Handlers.ElicitationHandler);
    }

    [Fact]
    public void CreateClientOptions_WithSamplingCapability_ReturnsOptionsWithSamplingOnly()
    {
        // Arrange
        var loader = new TestableBaseToolLoader(NullLogger.Instance);
        var mockServer = Substitute.For<McpServer>();
        var capabilities = new ClientCapabilities
        {
            Sampling = new SamplingCapability()
        };
        mockServer.ClientCapabilities.Returns(capabilities);

        // Act
        var options = loader.CreateClientOptionsPublic(mockServer);

        // Assert
        Assert.NotNull(options);
        Assert.NotNull(options.Handlers);
        Assert.NotNull(options.Handlers.SamplingHandler);
        Assert.Null(options.Handlers.ElicitationHandler);
    }

    [Fact]
    public void CreateClientOptions_WithElicitationCapability_ReturnsOptionsWithElicitationOnly()
    {
        // Arrange
        var loader = new TestableBaseToolLoader(NullLogger.Instance);
        var mockServer = Substitute.For<McpServer>();
        var capabilities = new ClientCapabilities
        {
            Elicitation = new ElicitationCapability()
        };
        mockServer.ClientCapabilities.Returns(capabilities);

        // Act
        var options = loader.CreateClientOptionsPublic(mockServer);

        // Assert
        Assert.NotNull(options);
        Assert.NotNull(options.Handlers);
        Assert.Null(options.Handlers.SamplingHandler);
        Assert.NotNull(options.Handlers.ElicitationHandler);
    }

    [Fact]
    public void CreateClientOptions_WithBothCapabilities_ReturnsOptionsWithBothCapabilities()
    {
        // Arrange
        var loader = new TestableBaseToolLoader(NullLogger.Instance);
        var mockServer = Substitute.For<McpServer>();
        var capabilities = new ClientCapabilities
        {
            Sampling = new SamplingCapability(),
            Elicitation = new ElicitationCapability()
        };
        mockServer.ClientCapabilities.Returns(capabilities);

        // Act
        var options = loader.CreateClientOptionsPublic(mockServer);

        // Assert
        Assert.NotNull(options);
        Assert.NotNull(options.Handlers);
        Assert.NotNull(options.Handlers.SamplingHandler);
        Assert.NotNull(options.Handlers.ElicitationHandler);
    }

    [Fact]
    public void CreateClientOptions_WithServerClientInfo_CopiesClientInfoToOptions()
    {
        // Arrange
        var loader = new TestableBaseToolLoader(NullLogger.Instance);
        var mockServer = Substitute.For<McpServer>();
        var clientInfo = new Implementation
        {
            Name = "test-client",
            Version = "1.0.0"
        };
        mockServer.ClientInfo.Returns(clientInfo);
        mockServer.ClientCapabilities.Returns(new ClientCapabilities());

        // Act
        var options = loader.CreateClientOptionsPublic(mockServer);

        // Assert
        Assert.NotNull(options);
        Assert.Equal(clientInfo, options.ClientInfo);
    }

    [Fact]
    public void CreateClientOptions_WithNullServerClientInfo_HandlesGracefully()
    {
        // Arrange
        var loader = new TestableBaseToolLoader(NullLogger.Instance);
        var mockServer = Substitute.For<McpServer>();
        mockServer.ClientInfo.Returns((Implementation?)null);
        mockServer.ClientCapabilities.Returns(new ClientCapabilities());

        // Act
        var options = loader.CreateClientOptionsPublic(mockServer);

        // Assert
        Assert.NotNull(options);
        Assert.Null(options.ClientInfo);
    }

    [Fact]
    public async Task CreateClientOptions_SamplingHandler_ValidatesRequestAndThrowsOnNull()
    {
        // Arrange
        var loader = new TestableBaseToolLoader(NullLogger.Instance);
        var mockServer = Substitute.For<McpServer>();
        var capabilities = new ClientCapabilities
        {
            Sampling = new SamplingCapability()
        };
        mockServer.ClientCapabilities.Returns(capabilities);

        // Act
        var options = loader.CreateClientOptionsPublic(mockServer);
        Assert.NotNull(options.Handlers.SamplingHandler);

        // Assert - verify handler validates null request
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await options.Handlers.SamplingHandler(null!, default!, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CreateClientOptions_SamplingHandler_DelegatesToServerSendRequestAsync()
    {
        // Arrange
        var loader = new TestableBaseToolLoader(NullLogger.Instance);
        var mockServer = Substitute.For<McpServer>();
        var capabilities = new ClientCapabilities
        {
            Sampling = new SamplingCapability()
        };
        mockServer.ClientCapabilities.Returns(capabilities);

        var samplingRequest = new CreateMessageRequestParams
        {
            MaxTokens = 1000,
            Messages =
            [
                new SamplingMessage
                {
                    Role = Role.User,
                    Content = [new TextContentBlock { Text = "Test message" }]
                }
            ]
        };

        var mockResponse = new JsonRpcResponse
        {
            Id = new RequestId(1),
            Result = JsonSerializer.SerializeToNode(new CreateMessageResult
            {
                Role = Role.Assistant,
                Content = [new TextContentBlock { Text = "Mock response" }],
                Model = "test-model"
            })
        };

        mockServer.SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>())
                  .Returns(Task.FromResult(mockResponse));

        // Act
        var options = loader.CreateClientOptionsPublic(mockServer);
        Assert.NotNull(options.Handlers.SamplingHandler);

        await options.Handlers.SamplingHandler(samplingRequest, default!, TestContext.Current.CancellationToken);

        // Assert - verify SendRequestAsync was called with sampling method
        await mockServer.Received(1).SendRequestAsync(
            Arg.Is<JsonRpcRequest>(req => req.Method == "sampling/createMessage"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateClientOptions_ElicitationHandler_DelegatesToServerSendRequestAsync()
    {
        // Arrange
        var loader = new TestableBaseToolLoader(NullLogger.Instance);
        var mockServer = Substitute.For<McpServer>();
        var capabilities = new ClientCapabilities
        {
            Elicitation = new ElicitationCapability()
            {
                Form = new(),
            }
        };
        mockServer.ClientCapabilities.Returns(capabilities);

        var elicitationRequest = new ElicitRequestParams
        {
            Message = "Please enter your password:",
            RequestedSchema = new()
            {
                Properties = new Dictionary<string, ElicitRequestParams.PrimitiveSchemaDefinition>()
                {
                    ["password"] = new ElicitRequestParams.StringSchema
                    {
                        Title = "password",
                        Description = "The user's password.",
                    }
                },
                Required = ["password"],
            }
        };

        var mockResponse = new JsonRpcResponse
        {
            Id = new RequestId(1),
            Result = JsonSerializer.SerializeToNode(new ElicitResult { Action = "accept" })
        };

        mockServer.SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>())
                  .Returns(Task.FromResult(mockResponse));

        // Act
        var options = loader.CreateClientOptionsPublic(mockServer);
        Assert.NotNull(options.Handlers.ElicitationHandler);

        await options.Handlers.ElicitationHandler(elicitationRequest, TestContext.Current.CancellationToken);

        // Assert - verify SendRequestAsync was called with elicitation method
        await mockServer.Received(1).SendRequestAsync(
            Arg.Is<JsonRpcRequest>(req => req.Method == "elicitation/create"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateClientOptions_ElicitationHandler_ValidatesRequestAndThrowsOnNull()
    {
        // Arrange
        var loader = new TestableBaseToolLoader(NullLogger.Instance);
        var mockServer = Substitute.For<McpServer>();
        var capabilities = new ClientCapabilities
        {
            Elicitation = new ElicitationCapability()
        };
        mockServer.ClientCapabilities.Returns(capabilities);

        // Act
        var options = loader.CreateClientOptionsPublic(mockServer);
        Assert.NotNull(options.Handlers.ElicitationHandler);

        // Assert - verify handler validates null request
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await options.Handlers.ElicitationHandler.Invoke(null!, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task HandleSecretElicitation_WhenElicitationDisabled_ProceedsWithoutConsent()
    {
        // Arrange
        var mockServer = Substitute.For<McpServer>();
        var request = new RequestContext<CallToolRequestParams>(mockServer, new CallToolRequestParams { Name = "test-tool" });
        var logger = Substitute.For<ILogger>();

        // Act
        var result = await TestableBaseToolLoader.HandleSecretElicitationAsyncPublic(
            request, "test-tool", dangerouslyDisableElicitation: true, logger, CancellationToken.None);

        // Assert
        Assert.Null(result); // Should proceed
        logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("elicitation is disabled")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task HandleSecretElicitation_WhenClientDoesNotSupportElicitation_RejectsOperation()
    {
        // Arrange
        var mockServer = Substitute.For<McpServer>();
        mockServer.ClientCapabilities.Returns((ClientCapabilities?)null); // No elicitation support
        var request = new RequestContext<CallToolRequestParams>(mockServer, new CallToolRequestParams { Name = "test-tool" });
        var logger = Substitute.For<ILogger>();

        // Act
        var result = await TestableBaseToolLoader.HandleSecretElicitationAsyncPublic(
            request, "test-tool", dangerouslyDisableElicitation: false, logger, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsError);
        Assert.Contains("does not support elicitation", result.Content[0].Text);
    }

    [Fact]
    public async Task HandleSecretElicitation_WhenUserAccepts_ProceedsWithOperation()
    {
        // Arrange
        var mockServer = Substitute.For<McpServer>();
        mockServer.ClientCapabilities.Returns(new ClientCapabilities { Elicitation = new ElicitationCapability() });
        var mockResponse = new JsonRpcResponse
        {
            Id = new RequestId(1),
            Result = JsonSerializer.SerializeToNode(new ElicitResult { Action = "accept" })
        };
        mockServer.SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>())
                  .Returns(Task.FromResult(mockResponse));

        var request = new RequestContext<CallToolRequestParams>(mockServer, new CallToolRequestParams { Name = "test-tool" });
        var logger = Substitute.For<ILogger>();

        // Act
        var result = await TestableBaseToolLoader.HandleSecretElicitationAsyncPublic(
            request, "test-tool", dangerouslyDisableElicitation: false, logger, CancellationToken.None);

        // Assert
        Assert.Null(result); // Should proceed
        await mockServer.Received(1).SendRequestAsync(
            Arg.Is<JsonRpcRequest>(req => req.Method == "elicitation/create"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleSecretElicitation_WhenUserDeclines_RejectsOperation()
    {
        // Arrange
        var mockServer = Substitute.For<McpServer>();
        mockServer.ClientCapabilities.Returns(new ClientCapabilities { Elicitation = new ElicitationCapability() });
        var mockResponse = new JsonRpcResponse
        {
            Id = new RequestId(1),
            Result = JsonSerializer.SerializeToNode(new ElicitResult { Action = "decline" })
        };
        mockServer.SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>())
                  .Returns(Task.FromResult(mockResponse));

        var request = new RequestContext<CallToolRequestParams>(mockServer, new CallToolRequestParams { Name = "test-tool" });
        var logger = Substitute.For<ILogger>();

        // Act
        var result = await TestableBaseToolLoader.HandleSecretElicitationAsyncPublic(
            request, "test-tool", dangerouslyDisableElicitation: false, logger, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsError);
        Assert.Contains("cancelled by user", result.Content[0].Text);
    }

    [Fact]
    public async Task HandleSecretElicitation_UsesEmptySchemaWithNoProperties()
    {
        // Arrange
        var mockServer = Substitute.For<McpServer>();
        mockServer.ClientCapabilities.Returns(new ClientCapabilities { Elicitation = new ElicitationCapability() });
        
        JsonRpcRequest? capturedRequest = null;
        var mockResponse = new JsonRpcResponse
        {
            Id = new RequestId(1),
            Result = JsonSerializer.SerializeToNode(new ElicitResult { Action = "accept" })
        };
        
        mockServer.SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>())
                  .Returns(callInfo =>
                  {
                      capturedRequest = callInfo.Arg<JsonRpcRequest>();
                      return Task.FromResult(mockResponse);
                  });

        var request = new RequestContext<CallToolRequestParams>(mockServer, new CallToolRequestParams { Name = "test-tool" });
        var logger = Substitute.For<ILogger>();

        // Act
        await TestableBaseToolLoader.HandleSecretElicitationAsyncPublic(
            request, "test-tool", dangerouslyDisableElicitation: false, logger, CancellationToken.None);

        // Assert - verify the schema has no properties (empty dictionary)
        Assert.NotNull(capturedRequest);
        var elicitParams = JsonSerializer.Deserialize<ElicitRequestParams>(capturedRequest.Params!.ToString()!);
        Assert.NotNull(elicitParams);
        Assert.NotNull(elicitParams.RequestedSchema);
        Assert.NotNull(elicitParams.RequestedSchema.Properties);
        Assert.Empty(elicitParams.RequestedSchema.Properties); // Key assertion: no form fields
        Assert.NotNull(elicitParams.RequestedSchema.Required);
        Assert.Empty(elicitParams.RequestedSchema.Required);
    }

    [Fact]
    public async Task HandleSecretElicitation_WhenExceptionOccurs_ReturnsErrorResult()
    {
        // Arrange
        var mockServer = Substitute.For<McpServer>();
        mockServer.ClientCapabilities.Returns(new ClientCapabilities { Elicitation = new ElicitationCapability() });
        mockServer.SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>())
                  .Returns<JsonRpcResponse>(_ => throw new InvalidOperationException("Elicitation failed"));

        var request = new RequestContext<CallToolRequestParams>(mockServer, new CallToolRequestParams { Name = "test-tool" });
        var logger = Substitute.For<ILogger>();

        // Act
        var result = await TestableBaseToolLoader.HandleSecretElicitationAsyncPublic(
            request, "test-tool", dangerouslyDisableElicitation: false, logger, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsError);
        Assert.Contains("Elicitation failed", result.Content[0].Text);
    }

    internal sealed class TestableBaseToolLoader : BaseToolLoader
    {
        public TestableBaseToolLoader(ILogger logger)
            : base(logger)
        {
        }

        public McpClientOptions CreateClientOptionsPublic(McpServer server)
        {
            return CreateClientOptions(server);
        }

        public static Task<CallToolResult?> HandleSecretElicitationAsyncPublic(
            RequestContext<CallToolRequestParams> request,
            string toolName,
            bool dangerouslyDisableElicitation,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            return HandleSecretElicitationAsync(request, toolName, dangerouslyDisableElicitation, logger, cancellationToken);
        }

        public override ValueTask<ListToolsResult> ListToolsHandler(RequestContext<ListToolsRequestParams> request, CancellationToken cancellationToken)
        {
            var result = new ListToolsResult
            {
                Tools = []
            };
            return ValueTask.FromResult(result);
        }

        public override ValueTask<CallToolResult> CallToolHandler(RequestContext<CallToolRequestParams> request, CancellationToken cancellationToken)
        {
            var result = new CallToolResult
            {
                Content = [],
                IsError = false
            };
            return ValueTask.FromResult(result);
        }
    }
}
