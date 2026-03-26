using System.CommandLine;
using Azure.Mcp.Core.Areas.Group.Commands;
using Azure.Mcp.Core.Helpers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Helpers;

public class CommandHelperTests : IDisposable
{
    private readonly string? _originalSubscriptionId;

    public CommandHelperTests()
    {
        _originalSubscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
        EnvironmentHelpers.SetAzureSubscriptionId(null);
        CommandHelper.ResetProfileCacheForTesting();
    }

    public void Dispose()
    {
        EnvironmentHelpers.SetAzureSubscriptionId(_originalSubscriptionId);
        CommandHelper.ResetProfileCacheForTesting();
    }

    [Fact]
    public void GetSubscription_EmptySubscriptionParameter_ReturnsEnvironmentValue()
    {
        // Arrange
        EnvironmentHelpers.SetAzureSubscriptionId("env-subs");
        var parseResult = GetParseResult(["--subscription", ""]);

        // Act
        var actual = CommandHelper.GetSubscription(parseResult);

        // Assert
        Assert.Equal("env-subs", actual);
    }

    [Fact]
    public void GetSubscription_MissingSubscriptionParameter_ReturnsEnvironmentValue()
    {
        // Arrange
        EnvironmentHelpers.SetAzureSubscriptionId("env-subs");
        var parseResult = GetParseResult([]);

        // Act
        var actual = CommandHelper.GetSubscription(parseResult);

        // Assert
        Assert.Equal("env-subs", actual);
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
        var parseResult = GetParseResult(["--subscription", "Azure subscription 1"]);

        // Act
        var actual = CommandHelper.GetSubscription(parseResult);

        // Assert
        Assert.Equal("env-subs", actual);
    }

    [Fact]
    public void GetSubscription_ParameterValueContainingDefault_ReturnsEnvironmentValue()
    {
        // Arrange
        EnvironmentHelpers.SetAzureSubscriptionId("env-subs");
        var parseResult = GetParseResult(["--subscription", "Some default name"]);

        // Act
        var actual = CommandHelper.GetSubscription(parseResult);

        // Assert
        Assert.Equal("env-subs", actual);
    }

    [Fact]
    public void GetSubscription_NoEnvironmentVariableParameterValueContainingDefault_ReturnsParameterValue()
    {
        // Arrange
        EnvironmentHelpers.SetAzureSubscriptionId(null);
        CommandHelper.ResetProfileCacheForTesting(() => null);
        var parseResult = GetParseResult(["--subscription", "Some default name"]);

        // Act
        var actual = CommandHelper.GetSubscription(parseResult);

        // Assert
        Assert.Equal("Some default name", actual);
    }

    [Fact]
    public void GetSubscription_NoEnvironmentVariableParameterValueContainingSubscription_ReturnsParameterValue()
    {
        // Arrange
        EnvironmentHelpers.SetAzureSubscriptionId(null);
        CommandHelper.ResetProfileCacheForTesting(() => null);
        var parseResult = GetParseResult(["--subscription", "Azure subscription 1"]);

        // Act
        var actual = CommandHelper.GetSubscription(parseResult);

        // Assert
        Assert.Equal("Azure subscription 1", actual);
    }

    private static ParseResult GetParseResult(params string[] args)
    {
        var command = new GroupListCommand(Substitute.For<ILogger<GroupListCommand>>());
        var commandDefinition = command.GetCommand();
        return commandDefinition.Parse(args);
    }
}
