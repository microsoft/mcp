using System.CommandLine;
using Azure.Mcp.Core.Areas.Group.Commands;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Helpers;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Helpers;

public class CommandHelperTests : IDisposable
{
    public void Dispose()
    {
        EnvironmentHelpers.ClearAzureSubscriptionId();
    }

    [Fact]
    public void GetSubscription_EmptySubscriptionParameter_ReturnsEnvironmentValue()
    {
        // Arrange
        EnvironmentHelpers.SetAzureSubscriptionId("env-subs");
        var expected = CommandHelper.GetDefaultSubscription();
        var parseResult = GetParseResult(["--subscription", ""]);

        // Act
        var actual = CommandHelper.GetSubscription(parseResult);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetSubscription_MissingSubscriptionParameter_ReturnsEnvironmentValue()
    {
        // Arrange
        EnvironmentHelpers.SetAzureSubscriptionId("env-subs");
        var expected = CommandHelper.GetDefaultSubscription();
        var parseResult = GetParseResult([]);

        // Act
        var actual = CommandHelper.GetSubscription(parseResult);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetSubscription_ValidSubscriptionParameter_ReturnsParameterValue()
    {
        // Arrange
        EnvironmentHelpers.SetAzureSubscriptionId("env-subs");
        var parseResult = GetParseResult(["--subscription", "param-subs"]);

        // Act
        var actual = CommandHelper.GetSubscription(parseResult);

        // Assert
        Assert.Equal("param-subs", actual);
    }

    [Fact]
    public void GetSubscription_ParameterValueContainingSubscription_ReturnsEnvironmentValue()
    {
        // Arrange
        EnvironmentHelpers.SetAzureSubscriptionId("env-subs");
        var expected = CommandHelper.GetDefaultSubscription();
        var parseResult = GetParseResult(["--subscription", "Azure subscription 1"]);

        // Act
        var actual = CommandHelper.GetSubscription(parseResult);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetSubscription_ParameterValueContainingDefault_ReturnsEnvironmentValue()
    {
        // Arrange
        EnvironmentHelpers.SetAzureSubscriptionId("env-subs");
        var expected = CommandHelper.GetDefaultSubscription();
        var parseResult = GetParseResult(["--subscription", "Some default name"]);

        // Act
        var actual = CommandHelper.GetSubscription(parseResult);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetSubscription_NoEnvironmentVariableParameterValueContainingDefault_ReturnsParameterValue()
    {
        // Arrange
        var subscription = CommandHelper.GetProfileSubscription();
        var parseResult = GetParseResult(["--subscription", "Some default name"]);

        // Act
        var actual = CommandHelper.GetSubscription(parseResult);

        // Assert
        Assert.Equal(subscription ?? "Some default name", actual);
    }

    [Fact]
    public void GetSubscription_NoEnvironmentVariableParameterValueContainingSubscription_ReturnsParameterValue()
    {
        // Arrange
        var subscription = CommandHelper.GetProfileSubscription();
        var parseResult = GetParseResult(["--subscription", "Azure subscription 1"]);

        // Act
        var actual = CommandHelper.GetSubscription(parseResult);

        // Assert
        Assert.Equal(subscription ?? "Azure subscription 1", actual);
    }

    private static ParseResult GetParseResult(params string[] args)
    {
        var command = new GroupListCommand(Substitute.For<ILogger<GroupListCommand>>());
        var commandDefinition = command.GetCommand();
        return commandDefinition.Parse(args);
    }
}
