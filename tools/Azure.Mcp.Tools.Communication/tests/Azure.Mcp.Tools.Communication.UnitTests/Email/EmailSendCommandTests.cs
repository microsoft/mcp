// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.Communication.Commands.Email;
using Azure.Mcp.Tools.Communication.Models;
using Azure.Mcp.Tools.Communication.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Communication.UnitTests.Email;

public class EmailSendCommandTests : CommandUnitTestsBase<EmailSendCommand, ICommunicationService>
{
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

        if (shouldSucceed)
        {
            // Setup mock for success case
            var expectedResult = new EmailSendResult
            {
                MessageId = "test-message-id",
                Status = "Queued"
            };

            Service.SendEmailAsync(
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
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(expectedResult);
        }

        // Act
        var response = await ExecuteCommandAsync(args.ToArray());

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
        }
        else
        {
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

        var expectedResult = new EmailSendResult
        {
            MessageId = "test-message-id",
            Status = "Queued"
        };

        Service.SendEmailAsync(
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
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);

        // Verify service was called with correct parameters
        await Service.Received(1).SendEmailAsync(
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
            null,
            Arg.Any<CancellationToken>());

        // Verify the response contains the expected result
        Assert.NotNull(response.Results);
        var result = ConvertResponse(response, CommunicationJsonContext.Default.EmailSendCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Result);
        
        // Verify the JSON can be properly deserialized (contains expected values)
        Assert.Contains("test-message-id", result.Result.MessageId);
        Assert.Contains("Queued", result.Result.Status);
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

        var expectedException = new RequestFailedException("Test error message");
        Service.SendEmailAsync(
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
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(expectedException);

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        var responseJson = JsonSerializer.Serialize(response.Results);
        Assert.Contains("Test error message", responseJson);
    }
}
