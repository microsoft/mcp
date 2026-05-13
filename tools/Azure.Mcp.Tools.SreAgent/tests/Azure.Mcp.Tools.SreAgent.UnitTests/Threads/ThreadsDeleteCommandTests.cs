// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.SreAgent.Commands;
using Azure.Mcp.Tools.SreAgent.Commands.Threads;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Helpers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.SreAgent.UnitTests.Threads;

public class ThreadsDeleteCommandTests : CommandUnitTestsBase<ThreadsDeleteCommand, ISreAgentService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("delete", command.Name);
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
        Assert.Contains(command.Options, o => o.Name == "--confirm");
    }

    [Theory]
    [InlineData("--subscription sub --agent test-agent --thread-id thread1 --confirm true", true)]
    [InlineData("--subscription sub --agent test-agent --thread-id thread1 --confirm false", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        TestEnvironment.ClearAzureSubscriptionId();
        if (shouldSucceed)
        {
            Service.ListAgentsAsync(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(new List<SreAgentResource>
                {
                    new() { Name = "test-agent", Endpoint = "https://test.azuresre.ai" }
                });

            Service.DeleteThreadAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.UnprocessableEntity, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
        }
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        Service.ListAgentsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new List<SreAgentResource>
            {
                new() { Name = "test-agent", Endpoint = "https://test.azuresre.ai" }
            });

        Service.DeleteThreadAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "test-agent", "--thread-id", "thread1", "--confirm", "true");

        var result = ValidateAndDeserializeResponse(
            response,
            SreAgentJsonContext.Default.SreAgentThreadOperationResult);
        Assert.Equal("thread1", result.ThreadId);
        Assert.Equal("deleted", result.Status);
        Assert.NotNull(result.Messages);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.ListAgentsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new List<SreAgentResource>
            {
                new() { Name = "test-agent", Endpoint = "https://test.azuresre.ai" }
            });

        Service.DeleteThreadAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "test-agent", "--thread-id", "thread1", "--confirm", "true");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Fact]
    public async Task BindOptions_BindsOptionsCorrectly()
    {
        Service.ListAgentsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new List<SreAgentResource>
            {
                new() { Name = "test-agent", Endpoint = "https://test.azuresre.ai" }
            });

        Service.DeleteThreadAsync(
            "https://test.azuresre.ai",
            "thread1",
            null,
            Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "test-agent", "--thread-id", "thread1", "--confirm", "true");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).DeleteThreadAsync(
            "https://test.azuresre.ai",
            "thread1",
            null,
            Arg.Any<CancellationToken>());
    }
}
