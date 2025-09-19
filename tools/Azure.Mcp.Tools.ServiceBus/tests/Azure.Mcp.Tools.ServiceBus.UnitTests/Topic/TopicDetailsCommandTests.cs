// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ServiceBus.Commands;
using Azure.Mcp.Tools.ServiceBus.Commands.Topic;
using Azure.Mcp.Tools.ServiceBus.Models;
using Azure.Mcp.Tools.ServiceBus.Services;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ServiceBus.UnitTests.Topic;

public class TopicDetailsCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceBusService _serviceBusService;
    private readonly ILogger<TopicDetailsCommand> _logger;
    private readonly TopicDetailsCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    // Test constants
    private const string SubscriptionId = "sub123";
    private const string TopicName = "testTopic";
    private const string NamespaceName = "test.servicebus.windows.net";

    public TopicDetailsCommandTests()
    {
        _serviceBusService = Substitute.For<IServiceBusService>();
        _logger = Substitute.For<ILogger<TopicDetailsCommand>>();

        var collection = new ServiceCollection().AddSingleton(_serviceBusService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

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

        _serviceBusService.GetTopicDetails(
            Arg.Is(NamespaceName),
            Arg.Is(TopicName),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>()
        ).Returns(expectedDetails);

        var args = _commandDefinition.Parse(["--subscription", SubscriptionId, "--namespace", NamespaceName, "--topic", TopicName]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        // write a json converter that extends from EntityStatus
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ServiceBusJsonContext.Default.TopicDetailsCommandResult);

        Assert.NotNull(result);
        Assert.Equal(TopicName, result.TopicDetails.Name);
        Assert.Equal(expectedDetails.Status, result.TopicDetails.Status);
        Assert.Equal(expectedDetails.SubscriptionCount, result.TopicDetails.SubscriptionCount);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesTopicNotFound()
    {
        // Arrange
        var serviceBusException = new ServiceBusException("Topic not found", ServiceBusFailureReason.MessagingEntityNotFound);

        _serviceBusService.GetTopicDetails(
            Arg.Is(NamespaceName),
            Arg.Is(TopicName),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>()
        ).ThrowsAsync(serviceBusException);

        var args = _commandDefinition.Parse(["--subscription", SubscriptionId, "--namespace", NamespaceName, "--topic", TopicName]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(404, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesGenericException()
    {
        // Arrange
        var expectedError = "Test error";

        _serviceBusService.GetTopicDetails(
            Arg.Is(NamespaceName),
            Arg.Is(TopicName),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>()
        ).ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse(["--subscription", SubscriptionId, "--namespace", NamespaceName, "--topic", TopicName]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
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

            _serviceBusService.GetTopicDetails(
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
