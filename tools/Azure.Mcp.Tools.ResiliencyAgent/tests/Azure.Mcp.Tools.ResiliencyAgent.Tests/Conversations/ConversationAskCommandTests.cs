// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResiliencyAgent.Commands;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.Conversations;
using Azure.Mcp.Tools.ResiliencyAgent.Models;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResiliencyAgent.Tests.Conversations;

public sealed class ConversationAskCommandTests
    : CommandUnitTestsBase<ConversationAskCommand, IResiliencyAgentService>
{
    private readonly IArtifactWriter _artifactWriter = Substitute.For<IArtifactWriter>();

    public ConversationAskCommandTests()
    {
        Services.AddSingleton(_artifactWriter);
    }
    private static AgentTurn Completed(string reply = "All resources are zone redundant.", params AgentArtifact[] artifacts) =>
        new("conv-1", "task-1", "completed", true, false, reply, artifacts);

    private static AgentTurn CompletedWithNextSteps(params string[] steps) =>
        new("conv-1", "task-1", "completed", true, false, "Done.", [], null, steps);

    private static AgentTurn Working(string? reasoningStep = null) =>
        new("conv-1", "task-1", "working", false, false, null, [], reasoningStep);

    private static AgentTurn AwaitingUser(string question = "Which subscription?") =>
        new("conv-1", "task-1", "input-required", false, true, question, []);

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("ask", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("--request \"is my app zone redundant\"", true)]
    [InlineData("--request \"is my app zone redundant\" --conversation-id conv-1", true)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.SendAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Completed());
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        var artifact = new AgentArtifact("main.bicep", "template", "text/plain", "param a string", "bicep");
        Service.SendAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Completed("Done.", artifact));
        _artifactWriter.Write(Arg.Any<IEnumerable<AgentArtifact>>(), Arg.Any<string>())
            .Returns([new WrittenArtifact("main.bicep", @"C:\ws\azure-resiliency\main.bicep", "bicep", "template")]);

        var response = await ExecuteCommandAsync("--request", "generate a template");

        var result = ValidateAndDeserializeResponse(
            response, ResiliencyAgentJsonContext.Default.ConversationAskResult);

        Assert.Equal("conv-1", result.ConversationId);
        Assert.Equal("completed", result.State);
        Assert.Equal("Done.", result.Reply);
        Assert.Single(result.Files);
        Assert.Equal("main.bicep", result.Files[0].Name);
        Assert.EndsWith("main.bicep", result.Files[0].Path);
    }

    [Fact]
    public async Task ExecuteAsync_WritesArtifactsToDiskRatherThanReturningContent()
    {
        var artifact = new AgentArtifact("registry.bicep", null, "text/plain", "resource r ...", "bicep");
        Service.SendAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Completed("Generated.", artifact));
        _artifactWriter.Write(Arg.Any<IEnumerable<AgentArtifact>>(), Arg.Any<string>())
            .Returns([new WrittenArtifact("registry.bicep", @"C:\ws\azure-resiliency\registry.bicep", "bicep", null)]);

        var response = await ExecuteCommandAsync("--request", "generate a template");

        _artifactWriter.Received(1).Write(Arg.Any<IEnumerable<AgentArtifact>>(), "conv-1");

        // The result carries the path, not the generated content - the point of writing to disk.
        var result = ValidateAndDeserializeResponse(
            response, ResiliencyAgentJsonContext.Default.ConversationAskResult);
        Assert.Single(result.Files);
        Assert.Equal(@"C:\ws\azure-resiliency\registry.bicep", result.Files[0].Path);
        Assert.Equal("bicep", result.Files[0].Format);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotWriteWhenThereAreNoArtifacts()
    {
        Service.SendAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Completed());

        var response = await ExecuteCommandAsync("--request", "assess my app");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        _artifactWriter.DidNotReceive().Write(Arg.Any<IEnumerable<AgentArtifact>>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ExecuteAsync_ReusesSuppliedConversationId()
    {
        Service.SendAsync("conv-existing", Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Completed());

        var response = await ExecuteCommandAsync("--request", "follow up", "--conversation-id", "conv-existing");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).SendAsync(
            "conv-existing", null, "follow up", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_PollsUntilTerminal()
    {
        Service.SendAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Working());
        Service.PollAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(Working(), Completed());

        var response = await ExecuteCommandAsync("--request", "assess my app");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(2).PollAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_SurvivesWhenNoProgressTokenIsAvailable()
    {
        // The specification forbids reporting progress without a token from the client, so the
        // conversation must still complete when there is nothing to report to.
        Service.SendAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Working("Analyzing Request"));
        Service.PollAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(Completed());

        var response = await ExecuteCommandAsync("--request", "assess my app");

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenClientCannotElicit_ReportsAnError()
    {
        // The agent needs the user, but a unit-test context has no MCP server to ask through.
        Service.SendAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(AwaitingUser());

        var response = await ExecuteCommandAsync("--request", "make my app resilient");

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.Contains("elicitation", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsTheAgentsOwnSuggestedNextSteps()
    {
        Service.SendAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(CompletedWithNextSteps("Generate Bicep templates", "Add more resource types"));

        var response = await ExecuteCommandAsync("--request", "assess my app");

        var result = ValidateAndDeserializeResponse(
            response, ResiliencyAgentJsonContext.Default.ConversationAskResult);

        Assert.Equal(["Generate Bicep templates", "Add more resource types"], result.SuggestedNextSteps);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNoNextStepsWhenTheAgentOfferedNone()
    {
        Service.SendAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Completed());

        var response = await ExecuteCommandAsync("--request", "assess my app");

        var result = ValidateAndDeserializeResponse(
            response, ResiliencyAgentJsonContext.Default.ConversationAskResult);

        Assert.Empty(result.SuggestedNextSteps);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.SendAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--request", "assess my app");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }
}
