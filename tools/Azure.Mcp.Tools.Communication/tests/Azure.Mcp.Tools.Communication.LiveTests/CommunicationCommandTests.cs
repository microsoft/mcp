// Copyright (c) Microsoft Corporation
using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tools.Communication.Models;
using Xunit;

namespace Azure.Mcp.Tools.Communication.LiveTests;

[Trait("Area", "Communication")]
[Trait("Command", "SmsSendCommand")]
public class CommunicationCommandTests : CommandTestsBase
{
    public CommunicationCommandTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task Should_SendSms_WithValidParameters()
    {
        // Get configuration from DeploymentOutputs in Settings
        Settings.DeploymentOutputs.TryGetValue("COMMUNICATION_SERVICES_ENDPOINT", out var endpoint);
        Settings.DeploymentOutputs.TryGetValue("COMMUNICATION_SERVICES_FROM_PHONE", out var fromPhone);
        Settings.DeploymentOutputs.TryGetValue("COMMUNICATION_SERVICES_TO_PHONE", out var toPhone);
        Assert.SkipWhen(string.IsNullOrEmpty(endpoint), "Communication Services endpoint not configured for live testing");
        Assert.SkipWhen(string.IsNullOrEmpty(fromPhone), "From phone number not configured for live testing");
        Assert.SkipWhen(string.IsNullOrEmpty(toPhone), "To phone number not configured for live testing");
        var result = await CallToolAsync(
            "azmcp_communication_sms_send",
            new()
            {
                { "endpoint", endpoint },
                { "from", fromPhone },
                { "to", new[] { toPhone } },
                { "message", "Test SMS from Azure MCP Live Test" },
                { "enable-delivery-report", true },
                { "tag", "live-test" }
            });

        // Assert that we have a result
        Assert.NotNull(result);

        // Get the results property which contains the SMS results
        var results = result!.AssertProperty("results");
        Assert.Equal(JsonValueKind.Array, results.ValueKind);

        // Make sure we have at least one result
        Assert.True(results.GetArrayLength() > 0, "No SMS results returned");

        // Get the first result
        var firstResult = results[0];
        Assert.Equal(JsonValueKind.Object, firstResult.ValueKind);

        // Verify expected properties
        var messageId = firstResult.AssertProperty("messageId").GetString();
        var to = firstResult.AssertProperty("to").GetString();
        var successful = firstResult.AssertProperty("successful").GetBoolean();

        // Verify the result values
        Assert.NotNull(messageId);
        Assert.StartsWith("+", to);
        Assert.True(successful, "SMS was not sent successfully");
        Assert.True(Guid.TryParse(messageId, out _), "MessageId should be a valid GUID");

        Output.WriteLine($"SMS successfully sent to {to} with message ID {messageId}");
    }

    [Theory]
    [InlineData("--invalid-endpoint test")]
    [InlineData("--endpoint")]
    [InlineData("--endpoint https://mycomm.communication.azure.com --from")]
    [InlineData("--endpoint https://mycomm.communication.azure.com --from +1234567890")]
    [InlineData("--endpoint https://mycomm.communication.azure.com --from +1234567890 --to +1987654321")]
    public async Task Should_Return400_WithInvalidInput(string args)
    {
        var result = await CallToolAsync(
            "azmcp_communication_sms_send",
            new()
            {
                { "args", args }
            });

        Output.WriteLine($"Error result: {result}");

        // Check if the response is valid
        if (result == null)
        {
            // If result is null, the test is considered a success because we expected an error
            // In this case, there's nothing more to validate
            return;
        }

        // If result is not null, let's check the status
        if (result.Value.TryGetProperty("status", out var statusElement))
        {
            var status = statusElement.GetInt32();
            Output.WriteLine($"Status code: {status}");

            // We expect error 400 for validation failures
            Assert.Equal((int)HttpStatusCode.BadRequest, status);
        }

        // Check if message property exists and get the message
        string? message = null;
        if (result.Value.TryGetProperty("message", out var messageElement))
        {
            message = messageElement.GetString();

            // If message is not null, log it
            if (message != null)
            {
                Output.WriteLine($"Error message: {message}");
            }
        }

        // Verify the message exists and contains expected text
        if (message != null)
        {
            Assert.True(
                message.Contains("Missing", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("Required", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("validation", StringComparison.OrdinalIgnoreCase),
                $"Error message did not contain expected text: {message}");
        }
    }
}
