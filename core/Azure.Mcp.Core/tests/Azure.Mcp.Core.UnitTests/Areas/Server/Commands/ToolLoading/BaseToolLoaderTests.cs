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
        var mockServer = Substitute.For<IMcpServer>();
        mockServer.ClientCapabilities.Returns((ClientCapabilities?)null);

        // Act
        var options = loader.CreateClientOptionsPublic(mockServer);

        // Assert
        Assert.NotNull(options);
        Assert.NotNull(options.Capabilities);
        Assert.Null(options.Capabilities.Sampling);
        Assert.Null(options.Capabilities.Elicitation);
    }

    [Fact]
    public void CreateClientOptions_WithEmptyCapabilities_ReturnsOptionsWithNoCapabilities()
    {
        // Arrange
        var loader = new TestableBaseToolLoader(NullLogger.Instance);
        var mockServer = Substitute.For<IMcpServer>();
        mockServer.ClientCapabilities.Returns(new ClientCapabilities());

        // Act
        var options = loader.CreateClientOptionsPublic(mockServer);

        // Assert
        Assert.NotNull(options);
        Assert.NotNull(options.Capabilities);
        Assert.Null(options.Capabilities.Sampling);
        Assert.Null(options.Capabilities.Elicitation);
    }

    [Fact]
    public void CreateClientOptions_WithSamplingCapability_ReturnsOptionsWithSamplingOnly()
    {
        // Arrange
        var loader = new TestableBaseToolLoader(NullLogger.Instance);
        var mockServer = Substitute.For<IMcpServer>();
        var capabilities = new ClientCapabilities
        {
            Sampling = new SamplingCapability()
        };
        mockServer.ClientCapabilities.Returns(capabilities);

        // Act
        var options = loader.CreateClientOptionsPublic(mockServer);

        // Assert
        Assert.NotNull(options);
        Assert.NotNull(options.Capabilities);
        Assert.NotNull(options.Capabilities.Sampling);
        Assert.Null(options.Capabilities.Elicitation);
    }

    [Fact]
    public void CreateClientOptions_WithElicitationCapability_ReturnsOptionsWithElicitationOnly()
    {
        // Arrange
        var loader = new TestableBaseToolLoader(NullLogger.Instance);
        var mockServer = Substitute.For<IMcpServer>();
        var capabilities = new ClientCapabilities
        {
            Elicitation = new ElicitationCapability()
        };
        mockServer.ClientCapabilities.Returns(capabilities);

        // Act
        var options = loader.CreateClientOptionsPublic(mockServer);

        // Assert
        Assert.NotNull(options);
        Assert.NotNull(options.Capabilities);
        Assert.Null(options.Capabilities.Sampling);
        Assert.NotNull(options.Capabilities.Elicitation);
    }

    [Fact]
    public void CreateClientOptions_WithBothCapabilities_ReturnsOptionsWithBothCapabilities()
    {
        // Arrange
        var loader = new TestableBaseToolLoader(NullLogger.Instance);
        var mockServer = Substitute.For<IMcpServer>();
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
        Assert.NotNull(options.Capabilities);
        Assert.NotNull(options.Capabilities.Sampling);
        Assert.NotNull(options.Capabilities.Elicitation);
    }

    [Fact]
    public void CreateClientOptions_WithServerClientInfo_CopiesClientInfoToOptions()
    {
        // Arrange
        var loader = new TestableBaseToolLoader(NullLogger.Instance);
        var mockServer = Substitute.For<IMcpServer>();
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
        var mockServer = Substitute.For<IMcpServer>();
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
        var mockServer = Substitute.For<IMcpServer>();
        var capabilities = new ClientCapabilities
        {
            Sampling = new SamplingCapability()
        };
        mockServer.ClientCapabilities.Returns(capabilities);

        // Act
        var options = loader.CreateClientOptionsPublic(mockServer);
        Assert.NotNull(options.Capabilities?.Sampling?.SamplingHandler);

        // Assert - verify handler validates null request
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await options.Capabilities.Sampling.SamplingHandler(null!, default!, CancellationToken.None));
    }

    [Fact]
    public async Task CreateClientOptions_SamplingHandler_DelegatesToServerSendRequestAsync()
    {
        // Arrange
        var loader = new TestableBaseToolLoader(NullLogger.Instance);
        var mockServer = Substitute.For<IMcpServer>();
        var capabilities = new ClientCapabilities
        {
            Sampling = new SamplingCapability()
        };
        mockServer.ClientCapabilities.Returns(capabilities);

        var samplingRequest = new CreateMessageRequestParams
        {
            Messages =
            [
                new SamplingMessage
                {
                    Role = Role.User,
                    Content = new TextContentBlock { Text = "Test message" }
                }
            ]
        };

        var mockResponse = new JsonRpcResponse
        {
            Id = new RequestId(1),
            Result = JsonSerializer.SerializeToNode(new CreateMessageResult
            {
                Role = Role.Assistant,
                Content = new TextContentBlock { Text = "Mock response" },
                Model = "test-model"
            })
        };

        mockServer.SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>())
                  .Returns(Task.FromResult(mockResponse));

        // Act
        var options = loader.CreateClientOptionsPublic(mockServer);
        Assert.NotNull(options.Capabilities?.Sampling?.SamplingHandler);

        await options.Capabilities.Sampling.SamplingHandler(samplingRequest, default!, CancellationToken.None);

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
        var mockServer = Substitute.For<IMcpServer>();
        var capabilities = new ClientCapabilities
        {
            Elicitation = new ElicitationCapability()
        };
        mockServer.ClientCapabilities.Returns(capabilities);

        var elicitationRequest = new ElicitRequestParams
        {
            Message = "Please enter your password:"
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
        Assert.NotNull(options.Capabilities?.Elicitation?.ElicitationHandler);

        await options.Capabilities.Elicitation.ElicitationHandler(elicitationRequest, CancellationToken.None);

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
        var mockServer = Substitute.For<IMcpServer>();
        var capabilities = new ClientCapabilities
        {
            Elicitation = new ElicitationCapability()
        };
        mockServer.ClientCapabilities.Returns(capabilities);

        // Act
        var options = loader.CreateClientOptionsPublic(mockServer);
        Assert.NotNull(options.Capabilities?.Elicitation?.ElicitationHandler);

        // Assert - verify handler validates null request
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await options.Capabilities.Elicitation.ElicitationHandler(null!, CancellationToken.None));
    }

    internal sealed class TestableBaseToolLoader : BaseToolLoader
    {
        public TestableBaseToolLoader(ILogger logger)
            : base(logger)
        {
        }

        public McpClientOptions CreateClientOptionsPublic(IMcpServer server)
        {
            return CreateClientOptions(server);
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
