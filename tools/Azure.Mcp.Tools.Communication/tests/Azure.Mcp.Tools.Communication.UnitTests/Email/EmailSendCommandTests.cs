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
    [InlineData(null, "sender@example.com", "recipient@example.com", "Subject", "Message", false, "Missing endpoint")]
    [InlineData("", "sender@example.com", "recipient@example.com", "Subject", "Message", false, "Empty endpoint")]
    [InlineData("https://example.communication.azure.com", null, "recipient@example.com", "Subject", "Message", false, "Missing sender")]
    [InlineData("https://example.communication.azure.com", "", "recipient@example.com", "Subject", "Message", false, "Empty sender")]
    [InlineData("https://example.communication.azure.com", "sender@example.com", null, "Subject", "Message", false, "Missing to email")]
    [InlineData("https://example.communication.azure.com", "sender@example.com", "", "Subject", "Message", false, "Empty to email")]
    [InlineData("https://example.communication.azure.com", "sender@example.com", "recipient@example.com", null, "Message", false, "Missing subject")]
    [InlineData("https://example.communication.azure.com", "sender@example.com", "recipient@example.com", "", "Message", false, "Empty subject")]
    [InlineData("https://example.communication.azure.com", "sender@example.com", "recipient@example.com", "Subject", null, false, "Missing message")]
    [InlineData("https://example.communication.azure.com", "sender@example.com", "recipient@example.com", "Subject", "", false, "Empty message")]
    [InlineData("https://example.communication.azure.com", "sender@example.com", "recipient@example.com", "Subject", "Message", true, "Valid parameters")]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string? endpoint, string? from, string? to, string? subject, string? message, bool shouldSucceed, string scenario)
    {
        // Arrange
        var args = new List<string>();

        if (endpoint != null)
        { args.AddRange(["--endpoint", endpoint]); }
        if (from != null)
        { args.AddRange(["--from", from]); }
        if (to != null)
        { args.AddRange(["--to", to]); }
        if (subject != null)
        { args.AddRange(["--subject", subject]); }
        if (message != null)
        { args.AddRange(["--message", message]); }

        var parseResult = _commandDefinition.Parse(args.ToArray());

        if (shouldSucceed)
        {
            // Setup mock for success case
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

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.Status);
        }
        else
        {
            // Act & Assert
            if (parseResult.Errors.Count > 0)
            {
                // Parse-time validation errors (missing required options)
                Assert.True(parseResult.Errors.Count > 0, $"Expected parse errors for scenario: {scenario}");
            }
            else
            {
                // Runtime validation errors (empty values or command validation)
                try
                {
                    var response = await _command.ExecuteAsync(_context, parseResult);

                    // If we reach here without exception, check if it's a validation error response
                    if (response.Status == HttpStatusCode.BadRequest)
                    {
                        // This is expected for validation failures
                        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
                    }
                    else
                    {
                        Assert.Fail($"Expected validation failure for scenario: {scenario}, but got status: {response.Status}");
                    }
                }
                catch (ArgumentException ex)
                {
                    // This is expected for service-level validation failures
                    Assert.NotNull(ex.Message);
                    Assert.NotEmpty(ex.Message);
                }
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithValidInput_CallsServiceAndReturnsSuccess()
    {
        // Arrange
        string[] args = [
            "--endpoint", "https://example.communication.azure.com",
            "--from", "sender@example.com",
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
        Assert.Equal(HttpStatusCode.OK, _context.Response.Status);

        // Verify service was called with correct parameters
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

        // Verify the response contains the expected result
        Assert.NotNull(_context.Response.Results);
        var responseJson = JsonSerializer.Serialize(_context.Response.Results);
        Assert.Contains(expectedResult.MessageId, responseJson);
        Assert.Contains(expectedResult.Status, responseJson);

        // Verify the JSON can be properly deserialized (contains expected values)
        Assert.Contains("test-message-id", responseJson);
        Assert.Contains("Queued", responseJson);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        string[] args = [
            "--endpoint", "https://example.communication.azure.com",
            "--from", "sender@example.com",
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
}
