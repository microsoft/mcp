// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using AzureMcp.Communication.Commands.Sms;
using AzureMcp.Communication.Options.Sms;
using AzureMcp.Communication.Services;
using AzureMcp.Core.Models.Command;
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
        Assert.Contains(cmd.Options, o => o.Name == "connection-string");
        Assert.Contains(cmd.Options, o => o.Name == "from");
        Assert.Contains(cmd.Options, o => o.Name == "to");
        Assert.Contains(cmd.Options, o => o.Name == "message");
    }

    [Theory]
    [InlineData("connection-string", true)]
    [InlineData("from", true)]
    [InlineData("to", true)]
    [InlineData("message", true)]
    [InlineData("enable-delivery-report", false)]
    [InlineData("tag", false)]
    public void RequiredOptions_ShouldBeMarkedCorrectly(string optionName, bool expectedRequired)
    {
        // Arrange
        var logger = Substitute.For<ILogger<SmsSendCommand>>();
        var command = new SmsSendCommand(logger);

        // Act
        var cmd = command.GetCommand();
        var option = cmd.Options.FirstOrDefault(o => o.Name == optionName);

        // Assert
        Assert.NotNull(option);
        Assert.Equal(expectedRequired, option.IsRequired);
    }
}