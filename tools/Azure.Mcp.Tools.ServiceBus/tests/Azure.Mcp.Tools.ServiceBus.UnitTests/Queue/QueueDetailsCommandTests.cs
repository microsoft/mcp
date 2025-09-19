// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ServiceBus.Commands;
using Azure.Mcp.Tools.ServiceBus.Commands.Queue;
using Azure.Mcp.Tools.ServiceBus.Models;
using Azure.Mcp.Tools.ServiceBus.Services;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ServiceBus.UnitTests.Queue;

public class QueueDetailsCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceBusService _serviceBusService;
    private readonly ILogger<QueueDetailsCommand> _logger;
    private readonly QueueDetailsCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    // Test constants
    private const string SubscriptionId = "sub123";
    private const string QueueName = "testQueue";
    private const string NamespaceName = "test.servicebus.windows.net";

    public QueueDetailsCommandTests()
    {
        _serviceBusService = Substitute.For<IServiceBusService>();
        _logger = Substitute.For<ILogger<QueueDetailsCommand>>();

        var collection = new ServiceCollection().AddSingleton(_serviceBusService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

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

        _serviceBusService.GetQueueDetails(
            Arg.Is(NamespaceName),
            Arg.Is(QueueName),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>()
        ).Returns(expectedDetails);

        var args = _commandDefinition.Parse(["--subscription", SubscriptionId, "--namespace", NamespaceName, "--queue", QueueName]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ServiceBusJsonContext.Default.QueueDetailsCommandResult);

        Assert.NotNull(result);
        Assert.Equal(QueueName, result.QueueDetails.Name);
        Assert.Equal(expectedDetails.Status, result.QueueDetails.Status);
        Assert.Equal(expectedDetails.ActiveMessageCount, result.QueueDetails.ActiveMessageCount);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesQueueNotFound()
    {
        // Arrange
        var serviceBusException = new ServiceBusException("Queue not found", ServiceBusFailureReason.MessagingEntityNotFound);

        _serviceBusService.GetQueueDetails(
            Arg.Is(NamespaceName),
            Arg.Is(QueueName),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>()
        ).ThrowsAsync(serviceBusException);

        var args = _commandDefinition.Parse(["--subscription", SubscriptionId, "--namespace", NamespaceName, "--queue", QueueName]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(404, response.Status);
        Assert.Contains("Queue not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesGenericException()
    {
        // Arrange
        var expectedError = "Test error";

        _serviceBusService.GetQueueDetails(
            Arg.Is(NamespaceName),
            Arg.Is(QueueName),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>()
        ).ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse(["--subscription", SubscriptionId, "--namespace", NamespaceName, "--queue", QueueName]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
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

            _serviceBusService.GetQueueDetails(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>())
                .Returns(expectedDetails);
        }

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(200, response.Status);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Equal(400, response.Status);
            Assert.Contains("required", response.Message.ToLower());
        }
    }
}
