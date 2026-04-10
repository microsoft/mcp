// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Tools.Communication.Commands.Sms;
using Azure.Mcp.Tools.Communication.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Communication.UnitTests.Sms;

public class SmsSendCommandTests
{
    private readonly ICommunicationService _communicationService;
    private readonly ILogger<SmsSendCommand> _logger;
    private readonly SmsSendCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public SmsSendCommandTests()
    {
        _communicationService = Substitute.For<ICommunicationService>();
        _logger = Substitute.For<ILogger<SmsSendCommand>>();

        _command = new(_logger, _communicationService);
        _context = new CommandContext(new ServiceCollection().BuildServiceProvider());
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        // Assert
        Assert.NotNull(_command);
        Assert.Equal("send", _command.Name);
        Assert.NotEmpty(_command.Description);
        Assert.NotEmpty(_command.Title);
    }

    [Fact]
    public void Command_ShouldHaveRequiredOptions()
    {
        // Assert
        Assert.NotNull(_commandDefinition);
        Assert.Contains(_commandDefinition.Options, o => o.Name == "--endpoint");
        Assert.Contains(_commandDefinition.Options, o => o.Name == "--from");
        Assert.Contains(_commandDefinition.Options, o => o.Name == "--to");
        Assert.Contains(_commandDefinition.Options, o => o.Name == "--message");
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
        var results = new List<Models.SmsResult> {
            new Models.SmsResult { MessageId = "msg1", To = to.First(), Successful = true, HttpStatusCode = 202 }
        };
        _communicationService.SendSmsAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(results));

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
        var parseResult = _commandDefinition.Parse(args.ToArray());

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(_context.Response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsException_HandlesError()
    {
        _communicationService.SendSmsAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<Models.SmsResult>>(new InvalidOperationException("fail")));

        var args = new[] { "--endpoint", "https://mycomm.communication.azure.com", "--from", "+1", "--to", "+2", "--message", "fail" };
        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

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
        var args = new List<string>();
        if (endpoint != null)
        { args.Add("--endpoint"); args.Add(endpoint); }
        if (from != null)
        { args.Add("--from"); args.Add(from); }
        if (to != null)
        { args.Add("--to"); args.Add(string.Join(",", to)); }
        if (message != null)
        { args.Add("--message"); args.Add(message); }
        var parseResult = _commandDefinition.Parse(args.ToArray());

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotEqual(System.Net.HttpStatusCode.OK, response.Status);
    }
}
