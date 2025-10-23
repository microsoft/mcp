// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Foundry.Commands;
using Azure.Mcp.Tools.Foundry.Models;
using Azure.Mcp.Tools.Foundry.Options;
using Azure.Mcp.Tools.Foundry.Options.Thread;
using Azure.Mcp.Tools.Foundry.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Foundry.UnitTests;

public class ThreadCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFoundryService _foundryService;

    public ThreadCreateCommandTests()
    {
        _foundryService = Substitute.For<IFoundryService>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_foundryService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Theory]
    [InlineData("", FoundryOptionDefinitions.Endpoint)]
    [InlineData("--endpoint https://test-endpoint.com", FoundryOptionDefinitions.UserMessage)]
    public async Task ExecuteAsync_Fails_WhenMissingRequiredParameter(string argsString, string missingArgName)
    {
        var command = new ThreadCreateCommand();
        var args = command.GetCommand().Parse(argsString);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(missingArgName, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCreatedThread()
    {
        var endpoint = "https://test-endpoint.com";
        var userMessage = "usermessage";

        var expectedResult = new ThreadCreateResult()
        {
            ThreadId = "threadId",
        };

        _foundryService.CreateThread(
            Arg.Is(endpoint),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(expectedResult);

        var command = new ThreadCreateCommand();
        var args = command.GetCommand().Parse(["--endpoint", endpoint, "--user-message", userMessage]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }
}
