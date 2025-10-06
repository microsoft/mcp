// Copyright (c) Microsoft Corporation
using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tests;
using Azure.Mcp.Tools.Communication.Commands.Email;
using Azure.Mcp.Tools.Communication.Models;
using Azure.Mcp.Tools.Communication.Options;
using Azure.Mcp.Tools.Communication.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Communication.UnitTests.Email;

public class EmailSendCommandTests
{
    private readonly ILogger<EmailSendCommand> _mockLogger;
    private readonly EmailSendCommand _command;

    public EmailSendCommandTests()
    {
        _mockLogger = Substitute.For<ILogger<EmailSendCommand>>();
        _command = new EmailSendCommand(_mockLogger);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Assert
        Assert.NotNull(_command);
        Assert.Equal("email send", _command.Name);
        Assert.Contains("Send an email message using Azure Communication Services", _command.Description);
    }

    [Fact]
    public void RegisterOptions_RegistersAllRequiredOptions()
    {
        // Arrange
        var rootCommand = new Command("test");

        // Act
        var command = _command.GetCommand();
        rootCommand.AddCommand(command);

        // Assert
        Assert.Contains(command.Options, o => o.Name == CommunicationOptionDefinitions.Endpoint.Name);
        Assert.Contains(command.Options, o => o.Name == CommunicationOptionDefinitions.Sender.Name);
        Assert.Contains(command.Options, o => o.Name == CommunicationOptionDefinitions.To.Name);
        Assert.Contains(command.Options, o => o.Name == CommunicationOptionDefinitions.Subject.Name);
        Assert.Contains(command.Options, o => o.Name == CommunicationOptionDefinitions.Message.Name);
        Assert.Contains(command.Options, o => o.Name == OptionDefinitions.Common.Subscription.Name);

        // Verify optional options
        Assert.Contains(command.Options, o => o.Name == CommunicationOptionDefinitions.SenderName.Name);
        Assert.Contains(command.Options, o => o.Name == CommunicationOptionDefinitions.Cc.Name);
        Assert.Contains(command.Options, o => o.Name == CommunicationOptionDefinitions.Bcc.Name);
        Assert.Contains(command.Options, o => o.Name == CommunicationOptionDefinitions.IsHtml.Name);
        Assert.Contains(command.Options, o => o.Name == CommunicationOptionDefinitions.ReplyTo.Name);
        Assert.Contains(command.Options, o => o.Name == OptionDefinitions.Common.ResourceGroup.Name);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange
        var parseResult = new Parser(_command.GetCommand()).Parse(
            "--endpoint", "https://example.communication.azure.com",
            "--sender", "sender@example.com",
            "--sender-name", "Test Sender",
            "--to", "recipient1@example.com", "recipient2@example.com",
            "--cc", "cc@example.com",
            "--bcc", "bcc@example.com",
            "--subject", "Test Subject",
            "--message", "Test Message",
            "--is-html",
            "--reply-to", "reply@example.com",
            "--subscription", "test-subscription",
            "--resource-group", "test-rg"
        );

        // Act
        var options = TestHelpers.InvokeNonPublicMethod<EmailSendCommand, Options.Email.EmailSendOptions>(
            _command,
            "BindOptions",
            parseResult);

        // Assert
        Assert.Equal("https://example.communication.azure.com", options.Endpoint);
        Assert.Equal("sender@example.com", options.Sender);
        Assert.Equal("Test Sender", options.SenderName);
        Assert.Collection(options.To,
            item => Assert.Equal("recipient1@example.com", item),
            item => Assert.Equal("recipient2@example.com", item)
        );
        Assert.Collection(options.Cc, item => Assert.Equal("cc@example.com", item));
        Assert.Collection(options.Bcc, item => Assert.Equal("bcc@example.com", item));
        Assert.Equal("Test Subject", options.Subject);
        Assert.Equal("Test Message", options.Message);
        Assert.True(options.IsHtml);
        Assert.Collection(options.ReplyTo, item => Assert.Equal("reply@example.com", item));
        Assert.Equal("test-subscription", options.Subscription);
        Assert.Equal("test-rg", options.ResourceGroup);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ExecuteAsync_WithMissingEndpoint_ThrowsValidationException(string endpoint)
    {
        // Arrange
        var context = new CommandContext();
        var parseResult = new Parser(_command.GetCommand()).Parse(
            "--sender", "sender@example.com",
            "--to", "recipient@example.com",
            "--subject", "Test Subject",
            "--message", "Test Message"
        );

        if (endpoint != null)
        {
            parseResult = new Parser(_command.GetCommand()).Parse(
                "--endpoint", endpoint,
                "--sender", "sender@example.com",
                "--to", "recipient@example.com",
                "--subject", "Test Subject",
                "--message", "Test Message"
            );
        }

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _command.ExecuteAsync(context, parseResult));

        Assert.Contains("endpoint", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidInput_CallsServiceAndReturnsSuccess()
    {
        // Arrange
        var context = new CommandContext();
        var parseResult = new Parser(_command.GetCommand()).Parse(
            "--endpoint", "https://example.communication.azure.com",
            "--sender", "sender@example.com",
            "--to", "recipient@example.com",
            "--subject", "Test Subject",
            "--message", "Test Message"
        );

        var expectedResult = new EmailSendResult
        {
            MessageId = "test-message-id",
            Status = "Queued"
        };

        _mockCommunicationService
            .Setup(s => s.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string[]>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<string[]>(),
                It.IsAny<string[]>(),
                It.IsAny<string[]>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<RetryPolicyOptions>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(0, result);
        _mockCommunicationService.Verify(s => s.SendEmailAsync(
            "https://example.communication.azure.com",
            "sender@example.com",
            null,
            It.Is<string[]>(arr => arr.Length == 1 && arr[0] == "recipient@example.com"),
            "Test Subject",
            "Test Message",
            false,
            null,
            null,
            null,
            null,
            null,
            null
        ), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var context = new CommandContext();
        var parseResult = new Parser(_command.GetCommand()).Parse(
            "--endpoint", "https://example.communication.azure.com",
            "--sender", "sender@example.com",
            "--to", "recipient@example.com",
            "--subject", "Test Subject",
            "--message", "Test Message"
        );

        var expectedException = new RequestFailedException("Test error message");
        _mockCommunicationService
            .Setup(s => s.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string[]>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<string[]>(),
                It.IsAny<string[]>(),
                It.IsAny<string[]>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<RetryPolicyOptions>()))
            .ThrowsAsync(expectedException);

        // Act
        await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.NotNull(context.Response.Error);
        Assert.Equal(expectedException.Message, context.Response.Error.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithOptionalParameters_PassesThemToService()
    {
        // Arrange
        var context = new CommandContext();
        var parseResult = new Parser(_command.GetCommand()).Parse(
            "--endpoint", "https://example.communication.azure.com",
            "--sender", "sender@example.com",
            "--sender-name", "Test Sender",
            "--to", "recipient1@example.com", "recipient2@example.com",
            "--cc", "cc@example.com",
            "--bcc", "bcc@example.com",
            "--subject", "Test Subject",
            "--message", "Test Message",
            "--is-html",
            "--reply-to", "reply@example.com",
            "--subscription", "test-subscription",
            "--resource-group", "test-rg"
        );

        var expectedResult = new EmailSendResult
        {
            MessageId = "test-message-id",
            Status = "Queued"
        };

        _mockCommunicationService
            .Setup(s => s.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string[]>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<string[]>(),
                It.IsAny<string[]>(),
                It.IsAny<string[]>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<RetryPolicyOptions>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(0, result);
        _mockCommunicationService.Verify(s => s.SendEmailAsync(
            "https://example.communication.azure.com",
            "sender@example.com",
            "Test Sender",
            It.Is<string[]>(arr => arr.Length == 2 && arr[0] == "recipient1@example.com" && arr[1] == "recipient2@example.com"),
            "Test Subject",
            "Test Message",
            true,
            It.Is<string[]>(arr => arr.Length == 1 && arr[0] == "cc@example.com"),
            It.Is<string[]>(arr => arr.Length == 1 && arr[0] == "bcc@example.com"),
            It.Is<string[]>(arr => arr.Length == 1 && arr[0] == "reply@example.com"),
            "test-subscription",
            "test-rg",
            null
        ), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var context = new CommandContext();
        var parseResult = new Parser(_command.GetCommand()).Parse(
            "--endpoint", "https://example.communication.azure.com",
            "--sender", "sender@example.com",
            "--to", "recipient@example.com",
            "--subject", "Test Subject",
            "--message", "Test Message"
        );

        var expectedResult = new EmailSendResult
        {
            MessageId = "test-message-id",
            Status = "Queued"
        };

        _mockCommunicationService
            .Setup(s => s.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string[]>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<string[]>(),
                It.IsAny<string[]>(),
                It.IsAny<string[]>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<RetryPolicyOptions>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(0, result);
        Assert.NotNull(context.Response.Results);

        // Verify the JSON can be properly deserialized
        var json = context.Response.Results.Value.ToString();
        Assert.Contains("test-message-id", json);
        Assert.Contains("Queued", json);
    }
}