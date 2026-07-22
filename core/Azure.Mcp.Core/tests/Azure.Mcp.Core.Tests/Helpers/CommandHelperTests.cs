// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Group.Commands;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Helpers;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.Tests.Helpers;

public class CommandHelperTests
{
    [Fact]
    public void GetSubscription_EmptySubscriptionParameter_ReturnsEnvironmentValue()
    {
        // Arrange
        var subscription = "env-subs";
        Environment.SetEnvironmentVariable(EnvironmentHelpers.AzureSubscriptionIdEnvironmentVariable, subscription);
        var options = BindSubscriptionOption("--subscription", "");

        // Act
        var actual = CommandHelper.GetSubscription(options.Subscription);

        // Assert
        Assert.Equal(subscription, actual);
    }

    [Fact]
    public void GetSubscription_MissingSubscriptionParameter_ReturnsEnvironmentValue()
    {
        // Arrange
        var subscription = "env-subs";
        Environment.SetEnvironmentVariable(EnvironmentHelpers.AzureSubscriptionIdEnvironmentVariable, subscription);
        var options = BindSubscriptionOption();

        // Act
        var actual = CommandHelper.GetSubscription(options.Subscription);

        // Assert
        Assert.Equal(subscription, actual);
    }

    [Fact]
    public void GetSubscription_ValidSubscriptionParameter_ReturnsParameterValue()
    {
        // Arrange
        var subscription = "param-subs";
        Environment.SetEnvironmentVariable(EnvironmentHelpers.AzureSubscriptionIdEnvironmentVariable, "env-subs");
        var options = BindSubscriptionOption("--subscription", subscription);

        // Act
        var actual = CommandHelper.GetSubscription(options.Subscription);

        // Assert
        Assert.Equal(subscription, actual);
    }

    [Fact]
    public void GetSubscription_ParameterValueContainingSubscription_ReturnsParameterValue()
    {
        // Arrange
        var subscription = "Azure subscription 1";
        Environment.SetEnvironmentVariable(EnvironmentHelpers.AzureSubscriptionIdEnvironmentVariable, "env-subs");
        var options = BindSubscriptionOption("--subscription", subscription);

        // Act
        var actual = CommandHelper.GetSubscription(options.Subscription);

        // Assert
        Assert.Equal(subscription, actual);
    }

    [Fact]
    public void GetSubscription_ParameterValueMatchingKnownPlaceholder_ReturnsEnvironmentValue()
    {
        // Arrange
        var subscription = "env-subs";
        Environment.SetEnvironmentVariable(EnvironmentHelpers.AzureSubscriptionIdEnvironmentVariable, subscription);
        var options = BindSubscriptionOption("--subscription", "<subscription_id>");

        // Act
        var actual = CommandHelper.GetSubscription(options.Subscription);

        // Assert
        Assert.Equal(subscription, actual);
    }

    [Fact]
    public void GetSubscription_NoEnvironmentVariableParameterValueMatchingKnownPlaceholder_ReturnsParameterValue()
    {
        // Arrange
        Environment.SetEnvironmentVariable(EnvironmentHelpers.AzureSubscriptionIdEnvironmentVariable, null);
        var subscription = CommandHelper.GetProfileSubscription();
        var options = BindSubscriptionOption(["--subscription", "<subscription_id>"]);

        // Act
        var actual = CommandHelper.GetSubscription(options.Subscription);

        // Assert
        // If-else this test as being logged in with Azure CLI cannot be mocked out at this time.
        // So, if the running environment is logged in the subscription will be defaulted to the Azure CLI subscription.
        if (subscription != null)
        {
            Assert.Equal(subscription, actual);
        }
        else
        {
            Assert.Equal("<subscription_id>", actual);
        }
    }

    private static ISubscriptionOption BindSubscriptionOption(params string[] args)
    {
        var command = new GroupListCommand(NullLogger<GroupListCommand>.Instance, Substitute.For<IResourceGroupService>(), new SubscriptionResolver());
        var commandDefinition = command.GetCommand();
        return command.BindOptions(commandDefinition.Parse(args));
    }
}
