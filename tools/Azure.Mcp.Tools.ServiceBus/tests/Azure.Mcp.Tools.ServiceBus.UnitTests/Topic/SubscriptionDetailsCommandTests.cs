// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ServiceBus.Commands.Topic;
using Azure.Mcp.Tools.ServiceBus.Models;
using Azure.Mcp.Tools.ServiceBus.Services;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using static Azure.Mcp.Tools.ServiceBus.Commands.Topic.SubscriptionDetailsCommand;

namespace Azure.Mcp.Tools.ServiceBus.UnitTests.Topic;

[Trait("Area", "ServiceBus")]
public class SubscriptionDetailsCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceBusService _serviceBusService;
    private readonly ILogger<SubscriptionDetailsCommand> _logger;
    private readonly SubscriptionDetailsCommand _command;
    private readonly CommandContext _context;
    private readonly Parser _parser;

    // Test constants
    private const string SubscriptionId = "sub123";
    private const string TopicName = "testTopic";
    private const string SubscriptionName = "testSubscription";
    private const string NamespaceName = "test.servicebus.windows.net";

    public SubscriptionDetailsCommandTests()
    {
        _serviceBusService = Substitute.For<IServiceBusService>();
        _logger = Substitute.For<ILogger<SubscriptionDetailsCommand>>();

        var collection = new ServiceCollection().AddSingleton(_serviceBusService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _parser = new(_command.GetCommand());
    }

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

        _serviceBusService.GetSubscriptionDetails(
            Arg.Is(NamespaceName),
            Arg.Is(TopicName),
            Arg.Is(SubscriptionName),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>()
        ).Returns(expectedDetails);

        var args = _parser.Parse([
            "--subscription", SubscriptionId,
            "--namespace", NamespaceName,
            "--topic", TopicName,
            "--subscription-name", SubscriptionName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var json = JsonSerializer.Serialize(response.Results, options);
        var result = JsonSerializer.Deserialize<SubscriptionDetailsCommandResult>(json, options);
        Assert.NotNull(result);
        Assert.Equal(SubscriptionName, result.SubscriptionDetails.SubscriptionName);
        Assert.Equal(TopicName, result.SubscriptionDetails.TopicName);
        Assert.Equal(expectedDetails.ActiveMessageCount, result.SubscriptionDetails.ActiveMessageCount);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesSubscriptionNotFound()
    {
        // Arrange
        var serviceBusException = new ServiceBusException("Subscription not found", ServiceBusFailureReason.MessagingEntityNotFound);

        _serviceBusService.GetSubscriptionDetails(
            Arg.Is(NamespaceName),
            Arg.Is(TopicName),
            Arg.Is(SubscriptionName),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>()
        ).ThrowsAsync(serviceBusException);

        var args = _parser.Parse([
            "--subscription", SubscriptionId,
            "--namespace", NamespaceName,
            "--topic", TopicName,
            "--subscription-name", SubscriptionName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(404, response.Status);
        Assert.Contains("not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesGenericException()
    {
        // Arrange
        var expectedError = "Test error";

        _serviceBusService.GetSubscriptionDetails(
            Arg.Is(NamespaceName),
            Arg.Is(TopicName),
            Arg.Is(SubscriptionName),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>()
        ).ThrowsAsync(new Exception(expectedError));

        var args = _parser.Parse([
            "--subscription", SubscriptionId,
            "--namespace", NamespaceName,
            "--topic", TopicName,
            "--subscription-name", SubscriptionName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
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

            _serviceBusService.GetSubscriptionDetails(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>())
                .Returns(expectedDetails);
        }

        var parseResult = _parser.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

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
