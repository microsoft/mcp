// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tools.Communication.Models;
using Xunit;

namespace Azure.Mcp.Tools.Communication.LiveTests.Email;

[Trait("Area", "Communication")]
[Trait("Command", "EmailSendCommand")]
public class EmailSendCommandLiveTests : CommandTestsBase
{
    public EmailSendCommandLiveTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task Should_SendEmail_WithValidParameters()
    {
        // Get configuration from DeploymentOutputs in Settings
        Settings.DeploymentOutputs.TryGetValue("COMMUNICATION_SERVICES_ENDPOINT", out var endpoint);
        Settings.DeploymentOutputs.TryGetValue("COMMUNICATION_SERVICES_SENDER_EMAIL", out var senderEmail);
        Settings.DeploymentOutputs.TryGetValue("COMMUNICATION_SERVICES_TEST_EMAIL", out var testEmail);

        // Output the values for debugging
        Output.WriteLine($"Endpoint: {endpoint ?? "null"}");
        Output.WriteLine($"Sender Email: {senderEmail ?? "null"}");
        Output.WriteLine($"Test Email: {testEmail ?? "null"}");

        Assert.SkipWhen(string.IsNullOrEmpty(endpoint), "Communication Services endpoint not configured for live testing");
        Assert.SkipWhen(string.IsNullOrEmpty(senderEmail), "Sender email not configured for live testing");
        Assert.SkipWhen(string.IsNullOrEmpty(testEmail), "Test recipient email not configured for live testing");

        var result = await CallToolAsync(
            "azmcp_communication_email_send",
            new()
            {
                { "endpoint", endpoint },
                { "from", senderEmail },
                { "to", new[] { testEmail } },
                { "subject", "Test Email from Azure MCP Live Test" },
                { "message", "This is a test email sent from Azure MCP Live Test." },
                { "is-html", false }
                // Using default Azure authentication (Managed Identity or az login)
            });

        // Assert that we have a result
        Assert.NotNull(result);

        // Check if we got a success response (has 'result' property) or error response
        if (result.Value.TryGetProperty("result", out var resultProperty))
        {
            // Success response - get the result property
            var emailResult = resultProperty;
            Assert.Equal(JsonValueKind.Object, emailResult.ValueKind);

            // Verify expected properties
            var messageIdElement = emailResult.AssertProperty("messageId");
            var messageId = messageIdElement.GetString();

            Assert.True(emailResult.TryGetProperty("status", out var messageStatusElement));
            var messageStatus = messageStatusElement.GetString();

            // Verify values
            Assert.NotNull(messageId);
            Assert.NotEmpty(messageId);
            Assert.NotNull(messageStatus);
            Assert.NotEmpty(messageStatus);

            Output.WriteLine($"Email successfully sent with message ID {messageId} and status {messageStatus}");
        }
        else if (result.Value.TryGetProperty("status", out var statusElement))
        {
            // This is an error response
            var status = statusElement.GetInt32();
            Output.WriteLine($"Error status code: {status}");

            if (result.Value.TryGetProperty("message", out var messageElement))
            {
                var message = messageElement.GetString();
                Output.WriteLine($"Error message: {message}");
            }

            // Skip the test due to auth error
            if (status == 401)
            {
                Output.WriteLine("Skipping test due to authentication error. Make sure Azure Managed Identity is configured properly.");
                Output.WriteLine("To run this test, ensure your Azure environment has the proper RBAC permissions set up for Communication Services.");
            }

            Assert.Fail($"Email sending failed with status code {status}");
        }
        else
        {
            Assert.Fail("Unexpected response format - no 'result' or 'status' property found");
        }
    }

    [Theory]
    [InlineData("--endpoint https://example.communication.azure.com")]
    [InlineData("--endpoint https://example.communication.azure.com --from sender@example.com")]
    [InlineData("--endpoint https://example.communication.azure.com --from sender@example.com --to")]
    [InlineData("--endpoint https://example.communication.azure.com --from sender@example.com --to recipient@example.com")]
    [InlineData("--endpoint https://example.communication.azure.com --from sender@example.com --to recipient@example.com --subject 'Test'")]
    public async Task Should_Return400_WithInvalidInput(string args)
    {
        var result = await CallToolAsync(
            "azmcp_communication_email_send",
            new()
            {
                { "args", args }
            });

        Output.WriteLine($"Error result: {result}");

        // Check if result is not null
        if (result == null)
        {
            Output.WriteLine($"Error result: {result}");
            return;
        }

        // Check if status property exists
        var statusElement = result.Value.AssertProperty("status");
        var status = statusElement.GetInt32();
        Output.WriteLine($"Status code: {status}");

        // We expect error 400 for validation failures
        Assert.Equal((int)HttpStatusCode.BadRequest, status);

        // Verify the error message exists
        var messageElement = result.Value.AssertProperty("message");
        var message = messageElement.GetString();

        // Make sure message is not null
        Assert.NotNull(message);
        Output.WriteLine($"Error message: {message}");

        // Verify the message contains expected text
        Assert.True(
            message!.Contains("Missing", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("Required", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("validation", StringComparison.OrdinalIgnoreCase),
            $"Error message did not contain expected text: {message}");
    }
}
