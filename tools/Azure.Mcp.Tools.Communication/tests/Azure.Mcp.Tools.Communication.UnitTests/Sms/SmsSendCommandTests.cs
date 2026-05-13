// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Communication.Commands.Sms;
using Azure.Mcp.Tools.Communication.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Communication.UnitTests.Sms;

public class SmsSendCommandTests : CommandUnitTestsBase<SmsSendCommand, ICommunicationService>
{
    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        // Assert
        Assert.NotNull(Command);
        Assert.Equal("send", Command.Name);
        Assert.NotEmpty(Command.Description);
        Assert.NotEmpty(Command.Title);
    }

    [Fact]
    public void Command_ShouldHaveRequiredOptions()
    {
        // Assert
        Assert.NotNull(CommandDefinition);
        Assert.Contains(CommandDefinition.Options, o => o.Name == "--endpoint");
        Assert.Contains(CommandDefinition.Options, o => o.Name == "--from");
        Assert.Contains(CommandDefinition.Options, o => o.Name == "--to");
        Assert.Contains(CommandDefinition.Options, o => o.Name == "--message");
    }

    [Theory]
    [InlineData("https://mycomm.communication.azure.com", "+1234567890", new[] { "+1234567891" }, "Hello", true, "test")]
    [InlineData("https://mycomm.communication.azure.com", "+1234567899", new[] { "+1234567892", "+1234567893" }, "Hi", false, "")]
    public async Task ExecuteAsync_WithValidParameters_CallsServiceAndReturnsResults(string endpoint, string from, string[] to, string message, bool enableDeliveryReport, string? tag)
    {
        var results = new List<Models.SmsResult> {
            new() { MessageId = "msg1", To = to.First(), Successful = true, HttpStatusCode = 202 }
        };
        Service.SendSmsAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(results);

        var args = new List<string>
        {
            "--endpoint", endpoint,
            "--from", from,
            "--to", string.Join(",", to),
            "--message", message
        };
        if (enableDeliveryReport)
            args.Add("--enable-delivery-report");
        if (!string.IsNullOrEmpty(tag))
        { args.Add("--tag"); args.Add(tag!); }

        // Act
        var response = await ExecuteCommandAsync(args.ToArray());

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsException_HandlesError()
    {
        Service.SendSmsAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("fail"));

        // Act
        var response = await ExecuteCommandAsync("--endpoint", "https://mycomm.communication.azure.com", "--from", "+1", "--to", "+2", "--message", "fail");

        // Assert
        Assert.NotNull(response);
        Assert.NotEqual(System.Net.HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Message);
    }

    [Theory]
    [InlineData(null, "+1234567890", new[] { "+1234567891" }, "Hello")]
    [InlineData("https://mycomm.communication.azure.com", null, new[] { "+1234567891" }, "Hello")]
    [InlineData("https://mycomm.communication.azure.com", "+1234567890", null, "Hello")]
    [InlineData("https://mycomm.communication.azure.com", "+1234567890", new[] { "+1234567891" }, null)]
    public async Task ExecuteAsync_MissingRequiredParameters_ReturnsError(string? endpoint, string? from, string[]? to, string? message)
    {
        var args = new List<string>();
        if (endpoint != null)
        { args.Add("--endpoint"); args.Add(endpoint); }
        if (from != null)
        { args.Add("--from"); args.Add(from); }
        if (to != null)
        { args.Add("--to"); args.Add(string.Join(",", to)); }
        if (message != null)
        { args.Add("--message"); args.Add(message); }

        // Act
        var response = await ExecuteCommandAsync(args.ToArray());

        // Assert
        Assert.NotNull(response);
        Assert.NotEqual(System.Net.HttpStatusCode.OK, response.Status);
    }
}
