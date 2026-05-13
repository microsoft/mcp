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

public class ThreadsListCommandTests : CommandUnitTestsBase<ThreadsListCommand, ISreAgentService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("list", command.Name);
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
    }

    [Theory]
    [InlineData("--subscription sub --agent test-agent", true)]
    [InlineData("", false)]
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

            Service.ListThreadsAsync(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
                .Returns(new List<SreAgentThread>());
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

        Service.ListThreadsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new List<SreAgentThread>
            {
                new() { Id = "thread1", Title = "Test Thread" }
            });

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "test-agent");

        var result = ValidateAndDeserializeResponse(
            response,
            SreAgentJsonContext.Default.ThreadsListCommandResult);
        Assert.NotNull(result.Threads);
        Assert.Single(result.Threads);
        Assert.Equal("thread1", result.Threads[0].Id);
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

        Service.ListThreadsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "test-agent");

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

        Service.ListThreadsAsync("https://test.azuresre.ai", null, Arg.Any<CancellationToken>())
            .Returns(new List<SreAgentThread>());

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "test-agent");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).ListThreadsAsync(
            "https://test.azuresre.ai",
            null,
            Arg.Any<CancellationToken>());
    }
}
