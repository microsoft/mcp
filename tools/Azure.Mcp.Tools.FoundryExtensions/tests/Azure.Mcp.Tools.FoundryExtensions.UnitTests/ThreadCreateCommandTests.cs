// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.FoundryExtensions.Commands;
using Azure.Mcp.Tools.FoundryExtensions.Options;
using Azure.Mcp.Tools.FoundryExtensions.Options.Thread;
using Azure.Mcp.Tools.FoundryExtensions.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.FoundryExtensions.UnitTests;

public class ThreadCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFoundryExtensionsService _foundryService;

    public ThreadCreateCommandTests()
    {
        _foundryService = Substitute.For<IFoundryExtensionsService>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_foundryService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Theory]
    [InlineData("", FoundryExtensionsOptionDefinitions.Endpoint)]
    [InlineData("--endpoint https://test-endpoint.com", FoundryExtensionsOptionDefinitions.UserMessage)]
    public async Task ExecuteAsync_Fails_WhenMissingRequiredParameter(string argsString, string missingArgName)
    {
        var command = new ThreadCreateCommand();
        var args = command.GetCommand().Parse(argsString);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

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
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var command = new ThreadCreateCommand();
        var args = command.GetCommand().Parse(["--endpoint", endpoint, "--user-message", userMessage]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }
}
