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

namespace Azure.Mcp.Tools.EventGrid.UnitTests.Topic;

public class TopicListCommandTests : CommandUnitTestsBase<TopicListCommand, IEventGridService>
{
    [Fact]
    public async Task ExecuteAsync_NoParameters_ReturnsTopics()
    {
        // Arrange
        var subscriptionId = "sub123";
        var expectedTopics = new List<Models.EventGridTopicInfo>
        {
            new("topic1", "eastus", "https://topic1.eastus.eventgrid.azure.net/api/events", "Succeeded", "Enabled", "EventGridSchema"),
            new("topic2", "westus", "https://topic2.westus.eventgrid.azure.net/api/events", "Succeeded", "Enabled", "EventGridSchema")
        };

        Service.GetTopicsAsync(Arg.Is(subscriptionId), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedTopics);

        // Act
        var response = await ExecuteCommandAsync("--subscription", subscriptionId);

        // Assert
        var result = ValidateAndDeserializeResponse(response, EventGridJsonContext.Default.TopicListCommandResult);

        Assert.NotNull(result.Topics);
        Assert.Equal(expectedTopics.Count, result.Topics.Count);
        Assert.Equal(expectedTopics.Select(t => t.Name), result.Topics.Select(t => t.Name));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoTopics()
    {
        // Arrange
        var subscriptionId = "sub123";

        Service.GetTopicsAsync(Arg.Is(subscriptionId), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync("--subscription", subscriptionId);

        // Assert
        var result = ValidateAndDeserializeResponse(response, EventGridJsonContext.Default.TopicListCommandResult);

        Assert.Empty(result.Topics);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";
        var subscriptionId = "sub123";

        Service.GetTopicsAsync(Arg.Is(subscriptionId), null, Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync("--subscription", subscriptionId);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Theory]
    [InlineData("--subscription test-sub", true)]
    [InlineData("--subscription test-sub --tenant test-tenant", true)]
    [InlineData("--subscription test-sub --resource-group test-rg", true)]
    [InlineData("--subscription test-sub --resource-group test-rg --tenant test-tenant", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var expectedTopics = new List<Models.EventGridTopicInfo>
            {
                new("topic1", "eastus", "https://topic1.eastus.eventgrid.azure.net/api/events", "Succeeded", "Enabled", "EventGridSchema"),
                new("topic2", "westus", "https://topic2.westus.eventgrid.azure.net/api/events", "Succeeded", "Enabled", "EventGridSchema")
            };
            Service.GetTopicsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns(expectedTopics);
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
            Assert.Contains("required", response.Message?.ToLower() ?? "");
        }
    }
}
