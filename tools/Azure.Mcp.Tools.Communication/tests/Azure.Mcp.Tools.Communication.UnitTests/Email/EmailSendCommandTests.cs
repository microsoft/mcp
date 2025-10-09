// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Azure;
using Azure.Mcp.Core.Exceptions;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tools.Communication.Commands.Email;
using Azure.Mcp.Tools.Communication.Models;
using Azure.Mcp.Tools.Communication.Options;
using Azure.Mcp.Tools.Communication.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Communication.UnitTests.Email;

public class EmailSendCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ICommunicationService _mockCommunicationService;
    private readonly ILogger<EmailSendCommand> _logger;
    private readonly EmailSendCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public EmailSendCommandTests()
    {
        _mockCommunicationService = Substitute.For<ICommunicationService>();
        _logger = Substitute.For<ILogger<EmailSendCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_mockCommunicationService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
        _context = new CommandContext(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ExecuteAsync_WithMissingEndpoint_ThrowsValidationException(string? endpoint)
    {
        // Arrange
        string[] args = ["--sender", "sender@example.com", "--to", "recipient@example.com", "--subject", "Test Subject", "--message", "Test Message"];

        if (!string.IsNullOrEmpty(endpoint))
        {
            args = ["--endpoint", endpoint, "--sender", "sender@example.com", "--to", "recipient@example.com", "--subject", "Test Subject", "--message", "Test Message"];
        }

        var parseResult = _commandDefinition.Parse(args);

        // Act & Assert
        if (string.IsNullOrEmpty(endpoint))
        {
            // When endpoint is missing, parse should have errors
            Assert.True(parseResult.Errors.Count > 0);
            var endpointError = parseResult.Errors.FirstOrDefault(e => e.Message.Contains("endpoint", StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(endpointError);
        }
        else
        {
            // When endpoint is empty string, execution should throw validation exception
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _command.ExecuteAsync(_context, parseResult));

            Assert.Contains("endpoint", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithValidInput_CallsServiceAndReturnsSuccess()
    {
        // Arrange
        string[] args = [
            "--endpoint", "https://example.communication.azure.com",
            "--sender", "sender@example.com",
            "--to", "recipient@example.com",
            "--subject", "Test Subject",
            "--message", "Test Message"
        ];

        var parseResult = _commandDefinition.Parse(args);

        var expectedResult = new EmailSendResult
        {
            MessageId = "test-message-id",
            Status = "Queued"
        };

        _mockCommunicationService
            .SendEmailAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string[]>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<bool>(),
                Arg.Any<string[]>(),
                Arg.Any<string[]>(),
                Arg.Any<string[]>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>())
            .Returns(expectedResult);

        // Act
        await _command.ExecuteAsync(_context, parseResult);
        Console.WriteLine($"Response: {JsonSerializer.Serialize(_context.Response)}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, _context.Response.Status);
        await _mockCommunicationService.Received(1).SendEmailAsync(
            "https://example.communication.azure.com",
            "sender@example.com",
            null,
            Arg.Is<string[]>(arr => arr.Length == 1 && arr[0] == "recipient@example.com"),
            "Test Subject",
            "Test Message",
            false,
            null,
            null,
            null,
            null,
            null
        );

        // Verify the result matches the expected result
        Assert.NotNull(_context.Response.Results);
        var responseJson = JsonSerializer.Serialize(_context.Response.Results);
        Assert.Contains(expectedResult.MessageId, responseJson);
        Assert.Contains(expectedResult.Status, responseJson);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        string[] args = [
            "--endpoint", "https://example.communication.azure.com",
            "--sender", "sender@example.com",
            "--to", "recipient@example.com",
            "--subject", "Test Subject",
            "--message", "Test Message"
        ];

        var parseResult = _commandDefinition.Parse(args);

        var expectedException = new RequestFailedException("Test error message");
        _mockCommunicationService
            .When(x => x.SendEmailAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string[]>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<bool>(),
                Arg.Any<string[]>(),
                Arg.Any<string[]>(),
                Arg.Any<string[]>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>()))
            .Do(x => throw expectedException);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);
        Console.WriteLine($"Response: {JsonSerializer.Serialize(response)}");

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.NotNull(_context.Response.Results);
        var responseJson = JsonSerializer.Serialize(_context.Response.Results);
        Assert.Contains("Test error message", responseJson);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        string[] args = [
            "--endpoint", "https://example.communication.azure.com",
            "--sender", "sender@example.com",
            "--to", "recipient@example.com",
            "--subject", "Test Subject",
            "--message", "Test Message"
        ];

        var parseResult = _commandDefinition.Parse(args);

        var expectedResult = new EmailSendResult
        {
            MessageId = "test-message-id",
            Status = "Queued"
        };

        _mockCommunicationService
            .SendEmailAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string[]>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<bool>(),
                Arg.Any<string[]>(),
                Arg.Any<string[]>(),
                Arg.Any<string[]>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>())
            .Returns(expectedResult);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);
        Console.WriteLine($"Response: {JsonSerializer.Serialize(response)}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(_context.Response.Results);

        // Verify the JSON can be properly deserialized
        var json = JsonSerializer.Serialize(_context.Response.Results);
        Assert.Contains("test-message-id", json);
        Assert.Contains("Queued", json);
    }
}
