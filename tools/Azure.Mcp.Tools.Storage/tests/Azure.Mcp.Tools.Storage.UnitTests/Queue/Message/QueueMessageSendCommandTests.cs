// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Storage.Commands.Queue.Message;
using Azure.Mcp.Tools.Storage.Models;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Storage.UnitTests.Queue.Message;

public class QueueMessageSendCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IStorageService _service;
    private readonly ILogger<QueueMessageSendCommand> _logger;
    private readonly QueueMessageSendCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public QueueMessageSendCommandTests()
    {
        _service = Substitute.For<IStorageService>();
        _logger = Substitute.For<ILogger<QueueMessageSendCommand>>();

        var collection = new ServiceCollection().AddSingleton(_service);
        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("send", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--account testaccount --queue testqueue --message \"test message\" --subscription sub123", true)]
    [InlineData("--account testaccount --queue testqueue --message \"test message\" --time-to-live-in-seconds 3600 --subscription sub123", true)]
    [InlineData("--account testaccount --queue testqueue --message \"test message\" --visibility-timeout-in-seconds 30 --subscription sub123", true)]
    [InlineData("--account testaccount --queue testqueue --message \"test message\" --time-to-live-in-seconds 3600 --visibility-timeout-in-seconds 30 --subscription sub123", true)]
    [InlineData("--account testaccount --queue testqueue --subscription sub123", false)] // Missing message content
    [InlineData("--account testaccount --message \"test message\" --subscription sub123", false)] // Missing queue name
    [InlineData("--queue testqueue --message \"test message\" --subscription sub123", false)] // Missing account name
    [InlineData("--account testaccount --queue testqueue --message \"test message\"", false)] // Missing subscription
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var mockResult = new QueueMessageSendResult(
                MessageId: "msg-123",
                InsertionTime: DateTimeOffset.UtcNow,
                ExpirationTime: DateTimeOffset.UtcNow.AddHours(1),
                PopReceipt: "receipt-456",
                NextVisibleTime: DateTimeOffset.UtcNow,
                Message: "test message");

            _service.SendQueueMessage(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int?>(),
                Arg.Any<int?>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>())
                .Returns(mockResult);
        }

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(shouldSucceed ? 200 : 400, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _service.SendQueueMessage(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(Task.FromException<QueueMessageSendResult>(new Exception("Test error")));

        var parseResult = _commandDefinition.Parse(["--account", "testaccount", "--queue", "testqueue", "--message", "test message", "--subscription", "sub123"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesQueueNotFoundError()
    {
        // Arrange
        var requestFailedException = new RequestFailedException(404, "Not found");

        _service.SendQueueMessage(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(requestFailedException);

        var parseResult = _commandDefinition.Parse(["--account", "testaccount", "--queue", "testqueue", "--message", "test message", "--subscription", "sub123"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(404, response.Status);
        Assert.Contains("Not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationError()
    {
        // Arrange
        var requestFailedException = new RequestFailedException(403, "Access denied");

        _service.SendQueueMessage(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(requestFailedException);

        var parseResult = _commandDefinition.Parse(["--account", "testaccount", "--queue", "testqueue", "--message", "test message", "--subscription", "sub123"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(403, response.Status);
        Assert.Contains("Access denied", response.Message);
    }
}
