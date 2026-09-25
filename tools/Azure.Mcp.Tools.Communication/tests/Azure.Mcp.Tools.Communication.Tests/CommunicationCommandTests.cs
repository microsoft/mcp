// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Microsoft.Mcp.Tests.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Communication.Tests;

[Trait("Command", "EmailSendCommand")]
[Trait("Command", "SmsSendCommand")]
public class CommunicationCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    private const string EmptyGuid = "00000000-0000-0000-0000-000000000000";
    private string? _endpointRecorded;
    private string? _fromSms;
    private string? _toSms;
    private string? _fromEmail;
    private string? _toEmail;
    public override bool EnableDefaultSanitizerAdditions => false;

    public override async ValueTask InitializeAsync()
    {
        await LoadSettingsAsync();
        if (TestMode == TestMode.Playback)
        {
            _endpointRecorded = "https://sanitized.communication.azure.com";
            _fromSms = "12345678900";
            _toSms = "12345678901";
            _fromEmail = "DoNotReply@domain.com";
            _toEmail = "placeholder@microsoft.com";
        }
        else
        {
            Settings.DeploymentOutputs.TryGetValue("COMMUNICATION_SERVICES_ENDPOINT", out _endpointRecorded);
            Settings.DeploymentOutputs.TryGetValue("COMMUNICATION_SERVICES_FROM_PHONE", out var tempFromSms);
            _fromSms = tempFromSms?.Substring(1); // Remove '+' for regex matching
            Settings.DeploymentOutputs.TryGetValue("COMMUNICATION_SERVICES_TO_PHONE", out var tempToSms);
            _toSms = tempToSms?.Substring(1); // Remove '+' for regex matching
            Settings.DeploymentOutputs.TryGetValue("COMMUNICATION_SERVICES_SENDER_EMAIL", out _fromEmail);
            Settings.DeploymentOutputs.TryGetValue("COMMUNICATION_SERVICES_TEST_EMAIL", out _toEmail);
        }

        await base.InitializeAsync();
    }

    public override List<GeneralRegexSanitizer> GeneralRegexSanitizers =>
    [
        ..base.GeneralRegexSanitizers,
        new(new()
        {
            Regex = Settings.ResourceBaseName,
            Value = "Sanitized",
        }),
        new(new()
        {
            Regex = Settings.SubscriptionId,
            Value = EmptyGuid,
        }),
        new(new()
        {
            Regex = _endpointRecorded,
            Value = "https://sanitized.communication.azure.com",
        }),
        new(new()
        {
            Regex = _fromEmail,
            Value = "DoNotReply@domain.com",
        }),
        new(new()
        {
            Regex = _toEmail,
            Value = "placeholder@microsoft.com",
        })
    ];

    public override List<BodyKeySanitizer> BodyKeySanitizers =>
    [
        ..base.BodyKeySanitizers,
        new(new("$..to")
        {
            Value = "12345678901"
        }),
        new(new("$.from")
        {
            Value = "12345678900"
        }),
        new(new("$..repeatabilityRequestId")
        {
            Value = EmptyGuid
        }),
        new(new("$..repeatabilityFirstSent")
        {
            Value = "Fri, 30 Jan 2026 01:02:04 GMT"
        })
    ];

    public override List<HeaderRegexSanitizer> HeaderRegexSanitizers =>
    [
        ..base.HeaderRegexSanitizers,
        new(new("Operation-Id")
        {
            Value = EmptyGuid
        })
    ];

    [Fact]
    public async Task Should_SendSms_WithValidParameters()
    {

        if (TestMode != TestMode.Playback)
        {
            Assert.SkipWhen(string.IsNullOrEmpty(_endpointRecorded), "Communication Services endpoint not configured for live testing");
            Assert.SkipWhen(string.IsNullOrEmpty(_fromSms), "From phone number not configured for live testing");
            Assert.SkipWhen(string.IsNullOrEmpty(_toSms), "To phone number not configured for live testing");
        }

        var result = await CallToolAsync(
            "communication_sms_send",
            new()
            {
                { "endpoint", _endpointRecorded },
                { "from", _fromSms },
                { "to", new[] { _toSms } },
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
        Assert.Equal(_toSms, to);
        Assert.True(successful, "SMS was not sent successfully");
        Assert.True(Guid.TryParse(messageId, out _), "MessageId should be a valid GUID");

        Output.WriteLine($"SMS successfully sent to {to} with message ID {messageId}");
    }

    [Fact]
    public async Task Should_SendEmail_WithValidParameters()
    {
        // Output the values for debugging
        Output.WriteLine($"Endpoint: {_endpointRecorded ?? "null"}");
        Output.WriteLine($"Sender Email: {_fromEmail ?? "null"}");
        Output.WriteLine($"Test Email: {_toEmail ?? "null"}");

        if (TestMode != TestMode.Playback)
        {
            Assert.SkipWhen(string.IsNullOrEmpty(_endpointRecorded), "Communication Services endpoint not configured for live testing");
            Assert.SkipWhen(string.IsNullOrEmpty(_fromEmail), "Sender email not configured for live testing");
            Assert.SkipWhen(string.IsNullOrEmpty(_toEmail), "Test recipient email not configured for live testing");
        }
        var result = await CallToolAsync(
            "communication_email_send",
            new()
            {
                { "endpoint", _endpointRecorded },
                { "from", _fromEmail },
                { "to", new[] { _toEmail } },
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
}
