// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ServiceBus.Commands;
using Azure.Mcp.Tools.ServiceBus.Commands.Queue;
using Azure.Mcp.Tools.ServiceBus.Models;
using Azure.Mcp.Tools.ServiceBus.Services;
using Azure.Messaging.ServiceBus;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ServiceBus.UnitTests.Queue;

public class QueueDetailsCommandTests : CommandUnitTestsBase<QueueDetailsCommand, IServiceBusService>
{
    // Test constants
    private const string SubscriptionId = "sub123";
    private const string QueueName = "testQueue";
    private const string NamespaceName = "test.servicebus.windows.net";

    [Fact]
    public async Task ExecuteAsync_ReturnsQueueDetails()
    {
        // Arrange
        var expectedDetails = new QueueDetails
        {
            Name = QueueName,
            Status = "Active",
            LockDuration = TimeSpan.FromMinutes(1),
            MaxDeliveryCount = 10,
            MaxMessageSizeInKilobytes = 1024,
            SizeInBytes = 1024,
            ActiveMessageCount = 5,
            DeadLetterMessageCount = 0,
            ScheduledMessageCount = 0
        };

        Service.GetQueueDetails(
            Arg.Is(NamespaceName),
            Arg.Is(QueueName),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedDetails);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", SubscriptionId,
            "--namespace", NamespaceName,
            "--queue", QueueName);

        // Assert
        var result = ValidateAndDeserializeResponse(response, ServiceBusJsonContext.Default.QueueDetailsCommandResult);

        Assert.Equal(QueueName, result.QueueDetails.Name);
        Assert.Equal(expectedDetails.Status, result.QueueDetails.Status);
        Assert.Equal(expectedDetails.ActiveMessageCount, result.QueueDetails.ActiveMessageCount);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesQueueNotFound()
    {
        // Arrange
        var serviceBusException = new ServiceBusException("Queue not found", ServiceBusFailureReason.MessagingEntityNotFound);

        Service.GetQueueDetails(
            Arg.Is(NamespaceName),
            Arg.Is(QueueName),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(serviceBusException);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", SubscriptionId,
            "--namespace", NamespaceName,
            "--queue", QueueName);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("Queue not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesGenericException()
    {
        // Arrange
        var expectedError = "Test error";

        Service.GetQueueDetails(
            Arg.Is(NamespaceName),
            Arg.Is(QueueName),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", SubscriptionId,
            "--namespace", NamespaceName,
            "--queue", QueueName);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Theory]
    [InlineData("--subscription sub123 --namespace test.servicebus.windows.net --queue testQueue", true)]
    [InlineData("--namespace test.servicebus.windows.net --queue testQueue", false)]  // Missing subscription
    [InlineData("--subscription sub123 --queue testQueue", false)]   // Missing namespace
    [InlineData("--subscription sub123 --namespace test.servicebus.windows.net", false)] // Missing queue
    [InlineData("", false)]  // Missing all required options
    public async Task ExecuteAsync_ValidatesRequiredParameters(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var expectedDetails = new QueueDetails
            {
                Name = QueueName,
                Status = "Active",
                ActiveMessageCount = 5
            };

            Service.GetQueueDetails(
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
