// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.ResiliencyAgent.Commands;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.Discovery;
using Azure.Mcp.Tools.ResiliencyAgent.Models;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Tests.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ResiliencyAgent.Tests.Discovery;

public sealed class DiscoveryGetCommandTests
    : CommandUnitTestsBase<DiscoveryGetCommand, IResiliencyAgentService>
{
    private readonly IAttachmentCache _attachmentCache = Substitute.For<IAttachmentCache>();

    public DiscoveryGetCommandTests()
    {
        Services.AddSingleton(Substitute.For<IArtifactWriter>());
        Services.AddSingleton(_attachmentCache);
        _attachmentCache.Resolve(Arg.Any<IReadOnlyList<string>?>()).Returns([]);
    }

    [Fact]
    public void Constructor_ProvidesInspectionOnlyMetadata()
    {
        var command = Command.GetCommand();

        Assert.Equal("get", command.Name);
        Assert.False(Command.Metadata.Destructive);
        Assert.Contains("List the available Azure application Discovery Agents", command.Description, StringComparison.Ordinal);
        Assert.Contains("has Reader access", command.Description, StringComparison.Ordinal);
        Assert.Contains("previous saved", command.Description, StringComparison.Ordinal);
        Assert.Contains("Do not create an agent", command.Description, StringComparison.Ordinal);
        Assert.Contains("completed response is terminal", command.Description, StringComparison.Ordinal);
        Assert.Contains("suggestedNextSteps", command.Description, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsSavedDiscoveryRequest()
    {
        Service.SendAsync(
                "conv-discovery",
                Arg.Is<string?>(taskId => taskId == null),
                Arg.Is<string>(request => request.Contains("saved discoveries", StringComparison.Ordinal)),
                Arg.Is<IReadOnlyList<AgentAttachment>>(attachments => attachments.Count == 0),
                Arg.Any<CancellationToken>())
            .Returns(new AgentTurn(
                "conv-discovery",
                "task-discovery",
                "completed",
                true,
                false,
                "No saved discoveries were found.",
                []));

        var response = await ExecuteCommandAsync(
            "--request",
            "List my saved discoveries",
            "--conversation-id",
            "conv-discovery");

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Theory]
    [InlineData("List my Discovery Agents")]
    [InlineData("Check whether Discovery Agent assigning-discovery has Reader access")]
    public async Task ExecuteAsync_ForwardsReadOnlyAgentInventoryRequests(string request)
    {
        Service.SendAsync(
                "conv-discovery",
                Arg.Is<string?>(taskId => taskId == null),
                request,
                Arg.Any<IReadOnlyList<AgentAttachment>>(),
                Arg.Any<CancellationToken>())
            .Returns(new AgentTurn(
                "conv-discovery",
                "task-discovery",
                "completed",
                true,
                false,
                "Inspection complete.",
                []));

        var response = await ExecuteCommandAsync(
            "--request",
            request,
            "--conversation-id",
            "conv-discovery");

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }
}

public sealed class DiscoveryManageCommandTests
    : CommandUnitTestsBase<DiscoveryManageCommand, IResiliencyAgentService>
{
    private readonly IAttachmentCache _attachmentCache = Substitute.For<IAttachmentCache>();

    public DiscoveryManageCommandTests()
    {
        Services.AddSingleton(Substitute.For<IArtifactWriter>());
        Services.AddSingleton(_attachmentCache);
        _attachmentCache.Resolve(Arg.Any<IReadOnlyList<string>?>()).Returns([]);
    }

    [Fact]
    public void Constructor_ProvidesFreshDiscoveryAndPlanningMetadata()
    {
        var command = Command.GetCommand();

        Assert.Equal("manage", command.Name);
        Assert.True(Command.Metadata.Destructive);
        Assert.Contains("Run every fresh", command.Description, StringComparison.Ordinal);
        Assert.Contains("rename", command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("regroup", command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("merge", command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("split", command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Discovery get owns agent listings and Reader checks", command.Description, StringComparison.Ordinal);
        Assert.Contains("This command does not execute", command.Description, StringComparison.Ordinal);
        Assert.Contains("completed backend reply is authoritative and terminal", command.Description, StringComparison.Ordinal);
        Assert.Contains("choose a discovery reference", command.Description, StringComparison.Ordinal);
        Assert.Contains("If saved discoveries are returned", command.Description, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsFreshDiscoveryRequest()
    {
        Service.SendAsync(
                "conv-discovery",
                Arg.Is<string?>(taskId => taskId == null),
                Arg.Is<string>(request => request.Contains("fresh discovery", StringComparison.Ordinal)),
                Arg.Any<IReadOnlyList<AgentAttachment>>(),
                Arg.Any<CancellationToken>())
            .Returns(new AgentTurn(
                "conv-discovery",
                "task-discovery",
                "input-required",
                false,
                true,
                "Run a fresh discovery now?",
                []));

        var response = await ExecuteCommandAsync(
            "--request",
            "Run a fresh discovery",
            "--conversation-id",
            "conv-discovery");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.Status);
    }
}

public sealed class DiscoveryExecuteCommandTests
    : CommandUnitTestsBase<DiscoveryExecuteCommand, IResiliencyAgentService>
{
    private readonly IAttachmentCache _attachmentCache = Substitute.For<IAttachmentCache>();

    public DiscoveryExecuteCommandTests()
    {
        Services.AddSingleton(Substitute.For<IArtifactWriter>());
        Services.AddSingleton(_attachmentCache);
        _attachmentCache.Resolve(Arg.Any<IReadOnlyList<string>?>()).Returns([]);
    }

    [Fact]
    public void Constructor_ProvidesExecutionOnlyMetadata()
    {
        var command = Command.GetCommand();

        Assert.Equal("execute", command.Name);
        Assert.True(Command.Metadata.Destructive);
        Assert.Contains("Execute now or apply now", command.Description, StringComparison.Ordinal);
        Assert.Contains("routing intent", command.Description, StringComparison.Ordinal);
        Assert.Contains("final Azure writes", command.Description, StringComparison.Ordinal);
        Assert.Contains("completed, declined, or cancelled response is", command.Description, StringComparison.Ordinal);
        Assert.Contains("Do not repeat execution", command.Description, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsExecutionRequestWithoutLocalMutation()
    {
        Service.SendAsync(
                "conv-discovery",
                Arg.Is<string?>(taskId => taskId == null),
                Arg.Is<string>(request => request.Contains("latest arrangement", StringComparison.Ordinal)),
                Arg.Any<IReadOnlyList<AgentAttachment>>(),
                Arg.Any<CancellationToken>())
            .Returns(new AgentTurn(
                "conv-discovery",
                "task-discovery",
                "input-required",
                false,
                true,
                "Execute the latest arrangement?",
                []));

        var response = await ExecuteCommandAsync(
            "--request",
            "Execute the latest arrangement",
            "--conversation-id",
            "conv-discovery");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.Status);
        await Service.DidNotReceive().SendAsync(
            Arg.Any<string>(),
            "task-discovery",
            Arg.Any<string>(),
            Arg.Any<IReadOnlyList<AgentAttachment>>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("decline")]
    [InlineData("cancel")]
    public async Task ExecuteAsync_WhenConfirmationIsNotAccepted_DoesNotContinueBackendTask(
        string action)
    {
        Service.SendAsync(
                "conv-discovery",
                Arg.Is<string?>(taskId => taskId == null),
                Arg.Is<string>(request => request.Contains("latest arrangement", StringComparison.Ordinal)),
                Arg.Any<IReadOnlyList<AgentAttachment>>(),
                Arg.Any<CancellationToken>())
            .Returns(new AgentTurn(
                "conv-discovery",
                "task-discovery",
                "input-required",
                false,
                true,
                "Execute the latest arrangement?",
                []));
        McpServer server = CreateElicitationServer(action);
        var context = new CommandContext { McpServer = server };

        CommandResponse response = await ((IBaseCommand)Command).ExecuteAsync(
            context,
            CommandDefinition.Parse(
                [
                    "--request",
                    "Execute the latest arrangement",
                    "--conversation-id",
                    "conv-discovery",
                ]),
            TestContext.Current.CancellationToken);
        ResiliencyAgentCommandResult result = ValidateAndDeserializeResponse(
            response,
            ResiliencyAgentJsonContext.Default.ResiliencyAgentCommandResult,
            HttpStatusCode.BadRequest);

        Assert.Equal("canceled", result.State);
        await Service.DidNotReceive().SendAsync(
            Arg.Any<string>(),
            "task-discovery",
            Arg.Any<string>(),
            Arg.Any<IReadOnlyList<AgentAttachment>>(),
            Arg.Any<CancellationToken>());
    }

    private static McpServer CreateElicitationServer(string action)
    {
        McpServer server = Substitute.For<McpServer>();
        server.ClientCapabilities.Returns(new ClientCapabilities
        {
            Elicitation = new ElicitationCapability { Form = new() },
        });
        server.SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>())
            .Returns(new JsonRpcResponse
            {
                Id = new RequestId(1),
                Result = JsonSerializer.SerializeToNode(new ElicitResult { Action = action }),
            });
        return server;
    }
}
