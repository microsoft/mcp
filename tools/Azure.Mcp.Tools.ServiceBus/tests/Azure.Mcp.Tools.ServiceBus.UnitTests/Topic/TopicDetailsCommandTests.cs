// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ServiceBus.Commands;
using Azure.Mcp.Tools.ServiceBus.Commands.Topic;
using Azure.Mcp.Tools.ServiceBus.Models;
using Azure.Mcp.Tools.ServiceBus.Services;
using Azure.Messaging.ServiceBus;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ServiceBus.UnitTests.Topic;

public class TopicDetailsCommandTests : CommandUnitTestsBase<TopicDetailsCommand, IServiceBusService>
{
    // Test constants
    private const string SubscriptionId = "sub123";
    private const string TopicName = "testTopic";
    private const string NamespaceName = "test.servicebus.windows.net";

    [Fact]
    public async Task ExecuteAsync_ReturnsTopicDetails()
    {
        // Arrange
        var expectedDetails = new TopicDetails
        {
            Name = TopicName,
            Status = "Active",
            DefaultMessageTimeToLive = TimeSpan.FromDays(14),
            MaxMessageSizeInKilobytes = 1024,
            SizeInBytes = 2048,
            SubscriptionCount = 3,
            EnablePartitioning = true,
            MaxSizeInMegabytes = 1024,
            ScheduledMessageCount = 0
        };

        Service.GetTopicDetails(
            Arg.Is(NamespaceName),
            Arg.Is(TopicName),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedDetails);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", SubscriptionId,
            "--namespace", NamespaceName,
            "--topic", TopicName);

        // Assert
        var result = ValidateAndDeserializeResponse(response, ServiceBusJsonContext.Default.TopicDetailsCommandResult);

        Assert.Equal(TopicName, result.TopicDetails.Name);
        Assert.Equal(expectedDetails.Status, result.TopicDetails.Status);
        Assert.Equal(expectedDetails.SubscriptionCount, result.TopicDetails.SubscriptionCount);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesTopicNotFound()
    {
        // Arrange
        var serviceBusException = new ServiceBusException("Topic not found", ServiceBusFailureReason.MessagingEntityNotFound);

        Service.GetTopicDetails(
            Arg.Is(NamespaceName),
            Arg.Is(TopicName),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(serviceBusException);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", SubscriptionId,
            "--namespace", NamespaceName,
            "--topic", TopicName);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesGenericException()
    {
        // Arrange
        var expectedError = "Test error";

        Service.GetTopicDetails(
            Arg.Is(NamespaceName),
            Arg.Is(TopicName),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", SubscriptionId,
            "--namespace", NamespaceName,
            "--topic", TopicName);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Theory]
    [InlineData("--subscription sub123 --namespace test.servicebus.windows.net --topic testTopic", true)]
    [InlineData("--namespace test.servicebus.windows.net --topic testTopic", false)]  // Missing subscription
    [InlineData("--subscription sub123 --topic testTopic", false)]   // Missing namespace
    [InlineData("--subscription sub123 --namespace test.servicebus.windows.net", false)] // Missing topic
    [InlineData("", false)]  // Missing all required options
    public async Task ExecuteAsync_ValidatesRequiredParameters(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var expectedDetails = new TopicDetails
            {
                Name = TopicName,
                Status = "Active",
                SubscriptionCount = 2
            };

            Service.GetTopicDetails(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(expectedDetails);
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
            Assert.Contains("required", response.Message.ToLower());
        }
    }
}
