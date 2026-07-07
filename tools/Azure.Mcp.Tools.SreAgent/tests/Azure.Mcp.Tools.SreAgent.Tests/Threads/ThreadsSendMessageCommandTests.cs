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

public class ThreadsSendMessageCommandTests : SubscriptionCommandUnitTestsBase<ThreadsSendMessageCommand, ISreAgentService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("send_message", command.Name);
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
        Assert.Contains(command.Options, o => o.Name == "--thread-id");
        Assert.Contains(command.Options, o => o.Name == "--message");
    }

    [Theory]
    [InlineData("--subscription sub --agent test-agent --thread-id thread1 --message \"test message\"", true)]
    [InlineData("--subscription sub --agent test-agent --thread-id thread1", false)]
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

            Service.SendThreadMessageAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<SreAgentThreadMessageRequest>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
                .Returns(new SreAgentThreadMessage { Id = "msg1" });

            Service.PollThreadForCompletionAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
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

        Service.SendThreadMessageAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<SreAgentThreadMessageRequest>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new SreAgentThreadMessage { Id = "msg1" });

        Service.PollThreadForCompletionAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(
            [
                new() { Id = "msg1", Text = "Agent response" }
            ]);

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "test-agent", "--thread-id", "thread1", "--message", "test message");

        var result = ValidateAndDeserializeResponse(
            response,
            SreAgentJsonContext.Default.SreAgentThreadOperationResult);
        Assert.NotNull(result.Messages);
        Assert.Equal("thread1", result.ThreadId);
        Assert.Equal("sent", result.Status);
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

        Service.SendThreadMessageAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<SreAgentThreadMessageRequest>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "test-agent", "--thread-id", "thread1", "--message", "test message");

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

        Service.SendThreadMessageAsync(
            "https://test.azuresre.ai",
            "thread1",
            Arg.Any<SreAgentThreadMessageRequest>(),
            null,
            Arg.Any<CancellationToken>())
            .Returns(new SreAgentThreadMessage { Id = "msg1" });

        Service.PollThreadForCompletionAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "test-agent", "--thread-id", "thread1", "--message", "test message");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).SendThreadMessageAsync(
            "https://test.azuresre.ai",
            "thread1",
            Arg.Any<SreAgentThreadMessageRequest>(),
            null,
            Arg.Any<CancellationToken>());
    }
}
