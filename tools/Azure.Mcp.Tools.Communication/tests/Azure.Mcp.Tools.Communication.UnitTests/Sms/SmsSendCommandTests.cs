// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.Communication.Commands.Sms;
using Azure.Mcp.Tools.Communication.Options.Sms;
using Azure.Mcp.Tools.Communication.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Communication.UnitTests.Sms;

[Trait("Area", "Communication")]
[Trait("Category", "Unit")]
public class SmsSendCommandTests
{
    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        // Arrange
        var logger = Substitute.For<ILogger<SmsSendCommand>>();

        // Act
        var command = new SmsSendCommand(logger);

        // Assert
        Assert.NotNull(command);
        Assert.Equal("send", command.Name);
        Assert.NotEmpty(command.Description);
        Assert.NotEmpty(command.Title);
    }

    [Fact]
    public void Command_ShouldHaveRequiredOptions()
    {
        // Arrange
        var logger = Substitute.For<ILogger<SmsSendCommand>>();
        var command = new SmsSendCommand(logger);

        // Act
        var cmd = command.GetCommand();

        // Assert
        Assert.NotNull(cmd);
        Assert.Contains(cmd.Options, o => o.Name == "--endpoint");
        Assert.Contains(cmd.Options, o => o.Name == "--from");
        Assert.Contains(cmd.Options, o => o.Name == "--to");
        Assert.Contains(cmd.Options, o => o.Name == "--message");
    }

    public static IEnumerable<object[]> ValidParameters => new List<object[]>
    {
        new object[] { "https://mycomm.communication.azure.com", "+1234567890", new string[] { "+1234567891" }, "Hello", true, "test" },
        new object[] { "https://mycomm.communication.azure.com", "+1234567899", new string[] { "+1234567892", "+1234567893" }, "Hi", false, "" }
    };

    [Theory]
    [MemberData(nameof(ValidParameters))]
    public async Task ExecuteAsync_WithValidParameters_CallsServiceAndReturnsResults(string endpoint, string from, string[] to, string message, bool enableDeliveryReport, string? tag)
    {
        var logger = Substitute.For<ILogger<SmsSendCommand>>();
        var service = Substitute.For<ICommunicationService>();
        var results = new List<Models.SmsResult> {
            new Models.SmsResult { MessageId = "msg1", To = to.First(), Successful = true, HttpStatusCode = 202 }
        };
        service.SendSmsAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>())
            .Returns(Task.FromResult(results));

        var command = new SmsSendCommand(logger);
        var services = new ServiceCollection();
        services.AddSingleton(service);
        var provider = services.BuildServiceProvider();
        var context = new CommandContext(provider);
        var cmd = command.GetCommand();
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
        var parseResult = cmd.Parse(args.ToArray());

        // Act
        var response = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(context.Response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsException_HandlesError()
    {
        var logger = Substitute.For<ILogger<SmsSendCommand>>();
        var service = Substitute.For<ICommunicationService>();
        service.SendSmsAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>())
            .Returns(Task.FromException<List<Models.SmsResult>>(new InvalidOperationException("fail")));

        var command = new SmsSendCommand(logger);
        var services = new ServiceCollection();
        services.AddSingleton(service);
        var provider = services.BuildServiceProvider();
        var context = new CommandContext(provider);
        var cmd = command.GetCommand();
        var args = new[] { "--endpoint", "https://mycomm.communication.azure.com", "--from", "+1", "--to", "+2", "--message", "fail" };
        var parseResult = cmd.Parse(args);

        // Act
        var response = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.NotNull(response);
        Assert.NotEqual(System.Net.HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Message);
    }

    public static IEnumerable<object?[]> InvalidParameters => new List<object?[]>
    {
        new object?[] { null, "+1234567890", new string[] { "+1234567891" }, "Hello" },
        new object?[] { "https://mycomm.communication.azure.com", null, new string[] { "+1234567891" }, "Hello" },
        new object?[] { "https://mycomm.communication.azure.com", "+1234567890", null, "Hello" },
        new object?[] { "https://mycomm.communication.azure.com", "+1234567890", new string[] { "+1234567891" }, null }
    };

    [Theory]
    [MemberData(nameof(InvalidParameters))]
    public async Task ExecuteAsync_MissingRequiredParameters_ReturnsError(string? endpoint, string? from, string[]? to, string? message)
    {
        var logger = Substitute.For<ILogger<SmsSendCommand>>();
        var service = Substitute.For<ICommunicationService>();
        var command = new SmsSendCommand(logger);
        var services = new ServiceCollection();
        services.AddSingleton(service);
        var provider = services.BuildServiceProvider();
        var context = new CommandContext(provider);
        var cmd = command.GetCommand();
        var args = new List<string>();
        if (endpoint != null)
        { args.Add("--endpoint"); args.Add(endpoint); }
        if (from != null)
        { args.Add("--from"); args.Add(from); }
        if (to != null)
        { args.Add("--to"); args.Add(string.Join(",", to)); }
        if (message != null)
        { args.Add("--message"); args.Add(message); }
        var parseResult = cmd.Parse(args.ToArray());

        // Act
        var response = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.NotNull(response);
        Assert.NotEqual(System.Net.HttpStatusCode.OK, response.Status);
    }
}
