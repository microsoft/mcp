// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.EventGrid.Commands;
using Azure.Mcp.Tools.EventGrid.Commands.Topic;
using Azure.Mcp.Tools.EventGrid.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.EventGrid.Tests.Topic;

public class TopicCreateCommandTests : CommandUnitTestsBase<TopicCreateCommand, IEventGridService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("create", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ValidParameters_ReturnsCreatedTopic()
    {
        var expectedTopic = new Models.EventGridTopicInfo(
            "test-topic", "eastus",
            "https://test-topic.eastus.eventgrid.azure.net/api/events",
            "Succeeded", "Enabled", "EventGridSchema");

        Service.CreateTopicAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Models.EventGridTopicInfo?>(expectedTopic));

        var response = await ExecuteCommandAsync(
            "--topic", "test-topic",
            "--resource-group", "test-rg",
            "--location", "eastus",
            "--subscription", "sub123");

        var result = ValidateAndDeserializeResponse(response, EventGridJsonContext.Default.TopicCreateCommandResult);
        Assert.NotNull(result.Topic);
        Assert.Equal("test-topic", result.Topic.Name);
        Assert.Equal("eastus", result.Topic.Location);
    }

    [Theory]
    [InlineData("--topic test --resource-group rg --location eastus --subscription sub", true)]
    [InlineData("--topic test --resource-group rg --subscription sub", false)] // missing location
    [InlineData("--topic test --location eastus --subscription sub", false)] // missing resource-group
    [InlineData("--resource-group rg --location eastus --subscription sub", false)] // missing topic
    [InlineData("", false)] // missing args
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.CreateTopicAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Models.EventGridTopicInfo?>(
                    new Models.EventGridTopicInfo("test", "eastus", null, "Succeeded", "Enabled", "EventGridSchema")));
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.CreateTopicAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync(
            "--topic", "test-topic",
            "--resource-group", "test-rg",
            "--location", "eastus",
            "--subscription", "sub123");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }
}
