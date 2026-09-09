#pragma warning disable MCP9005 // Deprecated Sampling/Logging APIs - backward compat during Phase 1
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Tests.Client.Helpers;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.Tests.Areas.Server.Commands.ToolLoading;

public class BaseToolLoaderTests
{
    [Fact]
    public void CreateClientOptions_WithNoCapabilities_ReturnsOptionsWithNoCapabilities()
    {
        // Arrange
        var mockServer = Substitute.For<McpServer>();
        mockServer.ClientCapabilities.Returns((ClientCapabilities?)null);

        // Act
        var options = BaseToolLoader.CreateClientOptions(mockServer);

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
        var mockServer = Substitute.For<McpServer>();
        mockServer.ClientCapabilities.Returns(new ClientCapabilities());

        // Act
        var options = BaseToolLoader.CreateClientOptions(mockServer);

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
        var mockServer = Substitute.For<McpServer>();
        var capabilities = new ClientCapabilities
        {
            Sampling = new SamplingCapability()
        };
        mockServer.ClientCapabilities.Returns(capabilities);

        // Act
        var options = BaseToolLoader.CreateClientOptions(mockServer);

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
        var mockServer = Substitute.For<McpServer>();
        var capabilities = new ClientCapabilities
        {
            Elicitation = new ElicitationCapability()
        };
        mockServer.ClientCapabilities.Returns(capabilities);

        // Act
        var options = BaseToolLoader.CreateClientOptions(mockServer);

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
        var mockServer = Substitute.For<McpServer>();
        var capabilities = new ClientCapabilities
        {
            Sampling = new SamplingCapability(),
            Elicitation = new ElicitationCapability()
        };
        mockServer.ClientCapabilities.Returns(capabilities);

        // Act
        var options = BaseToolLoader.CreateClientOptions(mockServer);

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
        var mockServer = Substitute.For<McpServer>();
        var clientInfo = new Implementation
        {
            Name = "test-client",
            Version = "1.0.0"
        };
        mockServer.ClientInfo.Returns(clientInfo);
        mockServer.ClientCapabilities.Returns(new ClientCapabilities());

        // Act
        var options = BaseToolLoader.CreateClientOptions(mockServer);

        // Assert
        Assert.NotNull(options);
        Assert.Equal(clientInfo, options.ClientInfo);
    }

    [Fact]
    public void CreateClientOptions_WithNullServerClientInfo_HandlesGracefully()
    {
        // Arrange
        var mockServer = Substitute.For<McpServer>();
        mockServer.ClientInfo.Returns((Implementation?)null);
        mockServer.ClientCapabilities.Returns(new ClientCapabilities());

        // Act
        var options = BaseToolLoader.CreateClientOptions(mockServer);

        // Assert
        Assert.NotNull(options);
        Assert.Null(options.ClientInfo);
    }

    [Fact]
    public async Task CreateClientOptions_SamplingHandler_ValidatesRequestAndThrowsOnNull()
    {
        // Arrange
        var mockServer = Substitute.For<McpServer>();
        var capabilities = new ClientCapabilities
        {
            Sampling = new SamplingCapability()
        };
        mockServer.ClientCapabilities.Returns(capabilities);

        // Act
        var options = BaseToolLoader.CreateClientOptions(mockServer);
        Assert.NotNull(options.Handlers.SamplingHandler);

        // Assert - verify handler validates null request
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await options.Handlers.SamplingHandler(null!, default!, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CreateClientOptions_SamplingHandler_DelegatesToServerSendRequestAsync()
    {
        // Arrange
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
            .Returns(mockResponse);

        // Act
        var options = BaseToolLoader.CreateClientOptions(mockServer);
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
            .Returns(mockResponse);

        // Act
        var options = BaseToolLoader.CreateClientOptions(mockServer);
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
        var mockServer = Substitute.For<McpServer>();
        var capabilities = new ClientCapabilities
        {
            Elicitation = new ElicitationCapability()
        };
        mockServer.ClientCapabilities.Returns(capabilities);

        // Act
        var options = BaseToolLoader.CreateClientOptions(mockServer);
        Assert.NotNull(options.Handlers.ElicitationHandler);

        // Assert - verify handler validates null request
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await options.Handlers.ElicitationHandler.Invoke(null!, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task HandleSecretElicitation_WhenElicitationDisabled_ProceedsWithoutConsent()
    {
        // Arrange
        var request = McpTestUtilities.CreateToolCallRequest("test-tool");
        var logger = Substitute.For<ILogger>();
        var baseCommand = Substitute.For<IBaseCommand>();
        baseCommand.Metadata.Returns(new ToolMetadata { Secret = true });

        // Act
        var result = await BaseToolLoader.HandleElicitationAsync(
            request, "test-tool", baseCommand, true, logger, TestContext.Current.CancellationToken);

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
        var request = McpTestUtilities.CreateToolCallRequest("test-tool", mockServer);
        var logger = Substitute.For<ILogger>();
        var baseCommand = Substitute.For<IBaseCommand>();
        baseCommand.Metadata.Returns(new ToolMetadata { Secret = true });

        // Act
        var result = await BaseToolLoader.HandleElicitationAsync(
            request, "test-tool", baseCommand, false, logger, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsError);
        Assert.Contains("does not support elicitation", ((TextContentBlock)result.Content[0]).Text);
    }

    [Fact]
    public async Task HandleSecretElicitation_WhenUserAccepts_ProceedsWithOperation()
    {
        // Arrange
        var mockServer = Substitute.For<McpServer>();
        mockServer.ClientCapabilities.Returns(new ClientCapabilities { Elicitation = new ElicitationCapability() { Form = new() } });
        var mockResponse = new JsonRpcResponse
        {
            Id = new RequestId(1),
            Result = JsonSerializer.SerializeToNode(new ElicitResult
            {
                Action = "accept",
                Content = new Dictionary<string, JsonElement>
                {
                    ["decision"] = JsonSerializer.SerializeToElement("accept")
                }
            })
        };
        mockServer.SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>())
            .Returns(mockResponse);

        var request = McpTestUtilities.CreateToolCallRequest("test-tool", mockServer);
        var logger = Substitute.For<ILogger>();

        var baseCommand = Substitute.For<IBaseCommand>();
        baseCommand.Metadata.Returns(new ToolMetadata { Secret = true });

        // Act
        var result = await BaseToolLoader.HandleElicitationAsync(
            request, "test-tool", baseCommand, false, logger, TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result); // Should proceed
        await mockServer.Received(1).SendRequestAsync(
            Arg.Is<JsonRpcRequest>(req => req.Method == "elicitation/create"),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("decline")]
    [InlineData("cancel")]
    public async Task HandleSecretElicitation_WhenEnvelopeNotAcceptedWithAcceptDecision_RejectsOperation(string action)
    {
        // Arrange
        var mockServer = Substitute.For<McpServer>();
        mockServer.ClientCapabilities.Returns(new ClientCapabilities { Elicitation = new ElicitationCapability() { Form = new() } });
        var mockResponse = new JsonRpcResponse
        {
            Id = new RequestId(1),
            Result = JsonSerializer.SerializeToNode(new ElicitResult
            {
                Action = action,
                Content = new Dictionary<string, JsonElement>
                {
                    ["decision"] = JsonSerializer.SerializeToElement("accept")
                }
            })
        };
        mockServer.SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>())
            .Returns(mockResponse);

        var request = McpTestUtilities.CreateToolCallRequest("test-tool", mockServer);
        var logger = Substitute.For<ILogger>();

        var baseCommand = Substitute.For<IBaseCommand>();
        baseCommand.Metadata.Returns(new ToolMetadata { Secret = true });

        // Act
        var result = await BaseToolLoader.HandleElicitationAsync(
            request, "test-tool", baseCommand, false, logger, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsError);
        Assert.Contains("cancelled by user", ((TextContentBlock)result.Content[0]).Text);
    }

    [Fact]
    public async Task HandleSecretElicitation_WhenUserSubmitsRejectDecision_RejectsOperation()
    {
        // Arrange - client submits the form (envelope action "accept") but the user selected
        // "Reject", so the selection is carried in Content["decision"]. The operation must NOT
        // execute in this scenario.
        var mockServer = Substitute.For<McpServer>();
        mockServer.ClientCapabilities.Returns(new ClientCapabilities { Elicitation = new ElicitationCapability() { Form = new() } });
        var mockResponse = new JsonRpcResponse
        {
            Id = new RequestId(1),
            Result = JsonSerializer.SerializeToNode(new ElicitResult
            {
                Action = "accept",
                Content = new Dictionary<string, JsonElement>
                {
                    ["decision"] = JsonSerializer.SerializeToElement("reject")
                }
            })
        };
        mockServer.SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>())
            .Returns(mockResponse);

        var request = McpTestUtilities.CreateToolCallRequest("test-tool", mockServer);
        var logger = Substitute.For<ILogger>();

        var baseCommand = Substitute.For<IBaseCommand>();
        baseCommand.Metadata.Returns(new ToolMetadata { Secret = true });

        // Act
        var result = await BaseToolLoader.HandleElicitationAsync(
            request, "test-tool", baseCommand, false, logger, TestContext.Current.CancellationToken);

        // Assert - a rejected decision must block the operation
        Assert.NotNull(result);
        Assert.True(result.IsError);
        Assert.Contains("cancelled by user", ((TextContentBlock)result.Content[0]).Text);
    }

    [Fact]
    public async Task HandleSecretElicitation_WhenAcceptEnvelopeButNoDecision_RejectsOperation()
    {
        // Arrange - envelope action is "accept" but no decision value is present. The handler
        // must treat this as not approved rather than assume approval.
        var mockServer = Substitute.For<McpServer>();
        mockServer.ClientCapabilities.Returns(new ClientCapabilities { Elicitation = new ElicitationCapability() { Form = new() } });
        var mockResponse = new JsonRpcResponse
        {
            Id = new RequestId(1),
            Result = JsonSerializer.SerializeToNode(new ElicitResult { Action = "accept" })
        };
        mockServer.SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>())
            .Returns(mockResponse);

        var request = McpTestUtilities.CreateToolCallRequest("test-tool", mockServer);
        var logger = Substitute.For<ILogger>();

        var baseCommand = Substitute.For<IBaseCommand>();
        baseCommand.Metadata.Returns(new ToolMetadata { Secret = true });

        // Act
        var result = await BaseToolLoader.HandleElicitationAsync(
            request, "test-tool", baseCommand, false, logger, TestContext.Current.CancellationToken);

        // Assert - a missing decision must block the operation
        Assert.NotNull(result);
        Assert.True(result.IsError);
        Assert.Contains("cancelled by user", ((TextContentBlock)result.Content[0]).Text);
    }

    [Fact]
    public async Task HandleSecretElicitation_UsesDecisionEnumSchema()
    {
        // Arrange
        var mockServer = Substitute.For<McpServer>();
        mockServer.ClientCapabilities.Returns(new ClientCapabilities { Elicitation = new ElicitationCapability() { Form = new() } });

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
                return mockResponse;
            });

        var request = McpTestUtilities.CreateToolCallRequest("test-tool", mockServer);
        var logger = Substitute.For<ILogger>();

        var baseCommand = Substitute.For<IBaseCommand>();
        baseCommand.Metadata.Returns(new ToolMetadata { Secret = true });

        // Act
        await BaseToolLoader.HandleElicitationAsync(
            request, "test-tool", baseCommand, false, logger, TestContext.Current.CancellationToken);

        // Assert - verify the schema has a decision single-select enum property with approve/reject
        Assert.NotNull(capturedRequest);
        Assert.NotNull(capturedRequest.Params);
        var elicitParams = JsonSerializer.Deserialize<ElicitRequestParams>(capturedRequest.Params.ToJsonString());
        Assert.NotNull(elicitParams);
        Assert.NotNull(elicitParams.RequestedSchema);
        Assert.NotNull(elicitParams.RequestedSchema.Properties);
        Assert.Single(elicitParams.RequestedSchema.Properties);
        Assert.True(elicitParams.RequestedSchema.Properties.ContainsKey("decision"));
        var decisionSchema = Assert.IsType<ElicitRequestParams.TitledSingleSelectEnumSchema>(elicitParams.RequestedSchema.Properties["decision"]);
        Assert.Equal("Decision", decisionSchema.Title);
        Assert.Equal("Approve or reject this sensitive operation.", decisionSchema.Description);
        Assert.NotNull(decisionSchema.OneOf);
        Assert.Equal(2, decisionSchema.OneOf.Count);
        Assert.Equal("Approve", decisionSchema.OneOf[0].Title);
        Assert.Equal("accept", decisionSchema.OneOf[0].Const);
        Assert.Equal("Reject", decisionSchema.OneOf[1].Title);
        Assert.Equal("reject", decisionSchema.OneOf[1].Const);
        Assert.NotNull(elicitParams.RequestedSchema.Required);
        Assert.Single(elicitParams.RequestedSchema.Required);
        Assert.Contains("decision", elicitParams.RequestedSchema.Required);
    }

    [Fact]
    public async Task HandleSecretElicitation_WhenExceptionOccurs_ReturnsErrorResult()
    {
        // Arrange
        var mockServer = Substitute.For<McpServer>();
        mockServer.ClientCapabilities.Returns(new ClientCapabilities { Elicitation = new ElicitationCapability() { Form = new() } });
        mockServer.SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>())
                  .Returns<JsonRpcResponse>(_ => throw new InvalidOperationException("Elicitation failed"));

        var request = McpTestUtilities.CreateToolCallRequest("test-tool", mockServer);
        var logger = Substitute.For<ILogger>();

        var baseCommand = Substitute.For<IBaseCommand>();
        baseCommand.Metadata.Returns(new ToolMetadata { Secret = true });

        // Act
        var result = await BaseToolLoader.HandleElicitationAsync(
            request, "test-tool", baseCommand, false, logger, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsError);
        Assert.Contains("Elicitation failed", ((TextContentBlock)result.Content[0]).Text);
    }
}
