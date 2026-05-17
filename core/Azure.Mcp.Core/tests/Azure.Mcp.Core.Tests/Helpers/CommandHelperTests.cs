// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Areas.Group.Commands;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Tests.Helpers;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.Tests.Helpers;

public class CommandHelperTests
{
    [Fact]
    public void GetSubscription_EmptySubscriptionParameter_ReturnsEnvironmentValue()
    {
        // Arrange
        TestEnvironment.SetAzureSubscriptionId("env-subs");
        var subscription = CommandHelper.GetDefaultSubscription();
        var parseResult = GetParseResult(["--subscription", ""]);

        // Act
        var actual = CommandHelper.GetSubscription(parseResult);

        // Assert
        Assert.Equal(subscription, actual);
    }

    [Fact]
    public void GetSubscription_MissingSubscriptionParameter_ReturnsEnvironmentValue()
    {
        // Arrange
        TestEnvironment.SetAzureSubscriptionId("env-subs");
        var subscription = CommandHelper.GetDefaultSubscription();
        var parseResult = GetParseResult([]);

        // Act
        var actual = CommandHelper.GetSubscription(parseResult);

        // Assert
        Assert.Equal(subscription, actual);
    }

    [Fact]
    public void GetSubscription_ValidSubscriptionParameter_ReturnsParameterValue()
    {
        // Arrange
        TestEnvironment.SetAzureSubscriptionId("env-subs");
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
        TestEnvironment.SetAzureSubscriptionId("env-subs");
        var subscription = CommandHelper.GetDefaultSubscription();
        var parseResult = GetParseResult(["--subscription", "Azure subscription 1"]);

        // Act
        var actual = CommandHelper.GetSubscription(parseResult);

        // Assert
        Assert.Equal(subscription, actual);
    }

    [Fact]
    public void GetSubscription_ParameterValueContainingDefault_ReturnsEnvironmentValue()
    {
        // Arrange
        TestEnvironment.SetAzureSubscriptionId("env-subs");
        var subscription = CommandHelper.GetDefaultSubscription();
        var parseResult = GetParseResult(["--subscription", "Some default name"]);

        // Act
        var actual = CommandHelper.GetSubscription(parseResult);

        // Assert
        Assert.Equal(subscription, actual);
    }

    [Fact]
    public void GetSubscription_NoEnvironmentVariableParameterValueContainingDefault_ReturnsParameterValue()
    {
        // Arrange
        TestEnvironment.ClearAzureSubscriptionId();
        var subscription = CommandHelper.GetProfileSubscription();
        var parseResult = GetParseResult(["--subscription", "Some default name"]);

        // Act
        var actual = CommandHelper.GetSubscription(parseResult);

        // Assert
        // If-else this test as being logged in with Azure CLI cannot be mocked out at this time.
        // So, if the running environment is logged in the subscription will be defaulted to the Azure CLI subscription.
        if (subscription != null)
        {
            Assert.Equal(subscription, actual);
        }
        else
        {
            Assert.Equal("Some default name", actual);
        }
    }

    [Fact]
    public void GetSubscription_NoEnvironmentVariableParameterValueContainingSubscription_ReturnsParameterValue()
    {
        // Arrange
        TestEnvironment.ClearAzureSubscriptionId();
        var subscription = CommandHelper.GetProfileSubscription();
        var parseResult = GetParseResult(["--subscription", "Azure subscription 1"]);

        // Act
        var actual = CommandHelper.GetSubscription(parseResult);

        // Assert
        // If-else this test as being logged in with Azure CLI cannot be mocked out at this time.
        // So, if the running environment is logged in the subscription will be defaulted to the Azure CLI subscription.
        if (subscription != null)
        {
            Assert.Equal(subscription, actual);
        }
        else
        {
            Assert.Equal("Azure subscription 1", actual);
        }
    }

    private static ParseResult GetParseResult(params string[] args)
    {
        var command = new GroupListCommand(NullLogger<GroupListCommand>.Instance, Substitute.For<IResourceGroupService>());
        var commandDefinition = command.GetCommand();
        return commandDefinition.Parse(args);
    }
}
