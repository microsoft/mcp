// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.SreAgent.Commands;
using Azure.Mcp.Tools.SreAgent.Commands.Docs;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.SreAgent.Tests.Docs;

public class MemoriesSearchCommandTests : SubscriptionCommandUnitTestsBase<MemoriesSearchCommand, ISreAgentService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("memories_search", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public void RegisterOptions_AddsExpectedOptions()
    {
        var command = Command.GetCommand();
        Assert.NotNull(command.Options);
        var optionNames = command.Options.Select(o => o.Name).ToList();
        Assert.Contains("--query", optionNames);
    }

    [Theory]
    [InlineData("--subscription sub --agent myagent --query test", true)]
    [InlineData("--subscription sub --agent myagent", false)]
    [InlineData("", false)]
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
                .Returns(new SreAgentResource { Name = "myagent", Endpoint = "https://myagent.azuresre.ai" });
            Service.SearchMemoriesAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
                .Returns(new List<MemorySearchResult>());
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
            .Returns(new SreAgentResource { Name = "myagent", Endpoint = "https://myagent.azuresre.ai" });
        Service.SearchMemoriesAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "myagent", "--query", "test");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_PassesQueryAndReturnsResults()
    {
        Service.GetAgentAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new SreAgentResource { Name = "myagent", Endpoint = "https://myagent.azuresre.ai" });
        Service.SearchMemoriesAsync(
            Arg.Any<string>(),
            "test-query",
            10,
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new List<MemorySearchResult>
            {
                new() { Title = "Result 1", Contents = "Test content 1" }
            });

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "myagent", "--query", "test-query");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        var result = ValidateAndDeserializeResponse(response, SreAgentJsonContext.Default.SreAgentTextResult);
        Assert.Contains("Result 1", result.Message);
        await Service.Received(1).SearchMemoriesAsync(
            "https://myagent.azuresre.ai",
            "test-query",
            10,
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }
}
