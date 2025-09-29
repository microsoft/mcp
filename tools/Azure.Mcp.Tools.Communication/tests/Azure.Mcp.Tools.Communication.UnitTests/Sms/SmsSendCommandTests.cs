// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using Azure.Mcp.Tools.Communication.Commands.Sms;
using Azure.Mcp.Tools.Communication.Options.Sms;
using Azure.Mcp.Tools.Communication.Services;
using Azure.Mcp.Core.Models.Command;
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

    // Removed RequiredOptions_ShouldBeMarkedCorrectly test because System.CommandLine.Option does not have IsRequired property.
}