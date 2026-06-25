// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.SreAgent.Commands;
using Azure.Mcp.Tools.SreAgent.Commands.Threads;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.SreAgent.Tests.Threads;

public class ThreadsInvestigateYoloCommandTests : SubscriptionCommandUnitTestsBase<ThreadsInvestigateYoloCommand, ISreAgentService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("investigate_yolo", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public void RegisterOptions_AddsExpectedOptions()
    {
        var command = Command.GetCommand();
        Assert.NotNull(command.Options);
        Assert.NotEmpty(command.Options);
        Assert.Contains(command.Options, o => o.Name == "--agent");
        Assert.Contains(command.Options, o => o.Name == "--message");
        Assert.Contains(command.Options, o => o.Name == "--max-iterations");
        Assert.Contains(command.Options, o => o.Name == "--timeout-seconds");
    }

    [Theory]
    [InlineData("--subscription sub --agent test-agent --message \"test message\"", true)]
    [InlineData("--subscription sub --agent test-agent", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.GetAgentAsync(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(new SreAgentResource { Name = "test-agent", Endpoint = "https://test.azuresre.ai" });

            Service.CreateThreadAsync(
                Arg.Any<string>(),
                Arg.Any<SreAgentThreadCreateRequest>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
                .Returns(new SreAgentThread { Id = "thread1" });

            Service.GetThreadAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
                .Returns(new SreAgentThread { Id = "thread1", LastMessage = new SreAgentThreadLastMessage { IsComplete = true } });

            Service.PollThreadForCompletionAsync(
                Arg.Any<string>(),
                "thread1",
                Arg.Any<string?>(),
                Arg.Any<TimeSpan>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
                .Returns([]);
        }

        var response = await ExecuteCommandAsync(args);

        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
        }
        else
        {
            Assert.NotEqual(HttpStatusCode.OK, response.Status);
        }
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
        }
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        Service.GetAgentAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new SreAgentResource { Name = "test-agent", Endpoint = "https://test.azuresre.ai" });

        Service.CreateThreadAsync(
            Arg.Any<string>(),
            Arg.Any<SreAgentThreadCreateRequest>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new SreAgentThread { Id = "thread1" });

        Service.GetThreadAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new SreAgentThread { Id = "thread1", LastMessage = new SreAgentThreadLastMessage { IsComplete = true } });

        Service.PollThreadForCompletionAsync(
            Arg.Any<string>(),
            "thread1",
            Arg.Any<string?>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(
            [
                new() { Id = "msg1", Text = "YOLO investigation result" }
            ]);

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "test-agent", "--message", "investigate this yolo");

        var result = ValidateAndDeserializeResponse(
            response,
            SreAgentJsonContext.Default.SreAgentInvestigationResult);
        Assert.NotNull(result.Messages);
        Assert.Equal("thread1", result.ThreadId);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.GetAgentAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new SreAgentResource { Name = "test-agent", Endpoint = "https://test.azuresre.ai" });

        Service.CreateThreadAsync(
            Arg.Any<string>(),
            Arg.Any<SreAgentThreadCreateRequest>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "test-agent", "--message", "investigate this yolo");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Fact]
    public async Task BindOptions_BindsOptionsCorrectly()
    {
        Service.GetAgentAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new SreAgentResource { Name = "test-agent", Endpoint = "https://test.azuresre.ai" });

        Service.CreateThreadAsync(
            "https://test.azuresre.ai",
            Arg.Any<SreAgentThreadCreateRequest>(),
            null,
            Arg.Any<CancellationToken>())
            .Returns(new SreAgentThread { Id = "thread1" });

        Service.GetThreadAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new SreAgentThread { Id = "thread1", LastMessage = new SreAgentThreadLastMessage { IsComplete = true } });

        Service.PollThreadForCompletionAsync(
            Arg.Any<string>(),
            "thread1",
            Arg.Any<string?>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "test-agent", "--message", "investigate this yolo", "--max-iterations", "3");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateThreadAsync(
            "https://test.azuresre.ai",
            Arg.Any<SreAgentThreadCreateRequest>(),
            null,
            Arg.Any<CancellationToken>());
    }
}
