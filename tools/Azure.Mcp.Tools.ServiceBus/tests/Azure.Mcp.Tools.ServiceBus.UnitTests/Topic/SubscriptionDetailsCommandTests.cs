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

public class SubscriptionDetailsCommandTests : CommandUnitTestsBase<SubscriptionDetailsCommand, IServiceBusService>
{
    // Test constants
    private const string SubscriptionId = "sub123";
    private const string TopicName = "testTopic";
    private const string SubscriptionName = "testSubscription";
    private const string NamespaceName = "test.servicebus.windows.net";

    [Fact]
    public async Task ExecuteAsync_ReturnsSubscriptionDetails()
    {
        // Arrange
        var expectedDetails = new SubscriptionDetails
        {
            SubscriptionName = SubscriptionName,
            TopicName = TopicName,
            LockDuration = TimeSpan.FromMinutes(1),
            MaxDeliveryCount = 10,
            EnableBatchedOperations = true,
            ActiveMessageCount = 5,
            DeadLetterMessageCount = 0,
            TransferDeadLetterMessageCount = 0
        };

        Service.GetSubscriptionDetails(
            Arg.Is(NamespaceName),
            Arg.Is(TopicName),
            Arg.Is(SubscriptionName),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedDetails);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", SubscriptionId,
            "--namespace", NamespaceName,
            "--topic", TopicName,
            "--subscription-name", SubscriptionName);

        // Assert
        var result = ValidateAndDeserializeResponse(response, ServiceBusJsonContext.Default.SubscriptionDetailsCommandResult);

        Assert.Equal(SubscriptionName, result.SubscriptionDetails.SubscriptionName);
        Assert.Equal(TopicName, result.SubscriptionDetails.TopicName);
        Assert.Equal(expectedDetails.ActiveMessageCount, result.SubscriptionDetails.ActiveMessageCount);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesSubscriptionNotFound()
    {
        // Arrange
        var serviceBusException = new ServiceBusException("Subscription not found", ServiceBusFailureReason.MessagingEntityNotFound);

        Service.GetSubscriptionDetails(
            Arg.Is(NamespaceName),
            Arg.Is(TopicName),
            Arg.Is(SubscriptionName),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(serviceBusException);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", SubscriptionId,
            "--namespace", NamespaceName,
            "--topic", TopicName,
            "--subscription-name", SubscriptionName);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesGenericException()
    {
        // Arrange
        var expectedError = "Test error";

        Service.GetSubscriptionDetails(
            Arg.Is(NamespaceName),
            Arg.Is(TopicName),
            Arg.Is(SubscriptionName),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", SubscriptionId,
            "--namespace", NamespaceName,
            "--topic", TopicName,
            "--subscription-name", SubscriptionName);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Theory]
    [InlineData("--subscription sub123 --namespace test.servicebus.windows.net --topic testTopic --subscription-name testSubscription", true)]
    [InlineData("--namespace test.servicebus.windows.net --topic testTopic --subscription-name testSubscription", false)]  // Missing subscription
    [InlineData("--subscription sub123 --topic testTopic --subscription-name testSubscription", false)]   // Missing namespace
    [InlineData("--subscription sub123 --namespace test.servicebus.windows.net --subscription-name testSubscription", false)] // Missing topic
    [InlineData("--subscription sub123 --namespace test.servicebus.windows.net --topic testTopic", false)] // Missing subscription-name
    [InlineData("", false)]  // Missing all required options
    public async Task ExecuteAsync_ValidatesRequiredParameters(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var expectedDetails = new SubscriptionDetails
            {
                SubscriptionName = SubscriptionName,
                TopicName = TopicName,
                ActiveMessageCount = 5
            };

            Service.GetSubscriptionDetails(
                Arg.Any<string>(),
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
