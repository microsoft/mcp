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

namespace Azure.Mcp.Core.Tests.Services.Azure.Subscription;

public class SubscriptionResolverTests
{
    private readonly SubscriptionResolver _real = new();

    [Fact]
    public void ResolveSubscription_EmptySubscriptionParameter_ReturnsEnvironmentValue()
    {
        // Arrange
        var subscription = "env-subs";
        var options = BindSubscriptionOption(CreateResolver(null, subscription), "--subscription", "");

        // Act & Assert
        Assert.Equal(subscription, options.Subscription);
    }

    [Fact]
    public void ResolveSubscription_EmptySubscriptionParameter_ReturnsCliValue()
    {
        // Arrange
        var subscription = "cli-subs";
        var options = BindSubscriptionOption(CreateResolver(subscription, null), "--subscription", "");

        // Act & Assert
        Assert.Equal(subscription, options.Subscription);
    }

    [Fact]
    public void ResolveSubscription_EmptySubscriptionParameter_PrefersCliOverEnvironmentValue()
    {
        // Arrange
        var environmentSubscription = "env-subs";
        var cliSubscription = "cli-subs";

        var options = BindSubscriptionOption(CreateResolver(cliSubscription, environmentSubscription), "--subscription", "");

        // Act & Assert
        Assert.Equal(cliSubscription, options.Subscription);
    }

    [Fact]
    public void ResolveSubscription_MissingSubscriptionParameter_ReturnsEnvironmentValue()
    {
        // Arrange
        var subscription = "env-subs";

        var options = BindSubscriptionOption(CreateResolver(null, subscription));

        // Act & Assert
        Assert.Equal(subscription, options.Subscription);
    }

    [Fact]
    public void ResolveSubscription_MissingSubscriptionParameter_ReturnsCliValue()
    {
        // Arrange
        var subscription = "cli-subs";

        var options = BindSubscriptionOption(CreateResolver(subscription, null));

        // Act & Assert
        Assert.Equal(subscription, options.Subscription);
    }

    [Fact]
    public void ResolveSubscription_MissingSubscriptionParameter_PrefersCliOverEnvironmentValue()
    {
        // Arrange
        var environmentSubscription = "env-subs";
        var cliSubscription = "cli-subs";

        var options = BindSubscriptionOption(CreateResolver(cliSubscription, environmentSubscription));

        // Act & Assert
        Assert.Equal(cliSubscription, options.Subscription);
    }

    [Fact]
    public void ResolveSubscription_ValidSubscriptionParameter_ReturnsParameterValue()
    {
        // Arrange
        var subscription = "param-subs";
        var options = BindSubscriptionOption(CreateResolver("cli-subs", "env-subs"), "--subscription", subscription);

        // Act & Assert
        Assert.Equal(subscription, options.Subscription);
    }

    [Fact]
    public void ResolveSubscription_ParameterValueContainingSubscription_ReturnsParameterValue()
    {
        // Arrange
        var subscription = "Azure subscription 1";
        var options = BindSubscriptionOption(CreateResolver("cli-subs", "env-subs"), "--subscription", subscription);

        // Act & Assert
        Assert.Equal(subscription, options.Subscription);
    }

    [Fact]
    public void ResolveSubscription_ParameterValueMatchingKnownPlaceholder_ReturnsEnvironmentValue()
    {
        // Arrange
        var subscription = "env-subs";
        var options = BindSubscriptionOption(CreateResolver(null, subscription), "--subscription", "<subscription_id>");

        // Act & Assert
        Assert.Equal(subscription, options.Subscription);
    }

    [Fact]
    public void ResolveSubscription_ParameterValueMatchingKnownPlaceholder_ReturnsCliValue()
    {
        // Arrange
        var subscription = "env-subs";
        var options = BindSubscriptionOption(CreateResolver(subscription, null), "--subscription", "<subscription_id>");

        // Act & Assert
        Assert.Equal(subscription, options.Subscription);
    }

    [Fact]
    public void ResolveSubscription_ParameterValueMatchingKnownPlaceholder_PrefersCliOverEnvironmentValue()
    {
        // Arrange
        var environmentSubscription = "env-subs";
        var cliSubscription = "cli-subs";

        var options = BindSubscriptionOption(CreateResolver(cliSubscription, environmentSubscription), "--subscription", "<subscription_id>");

        // Act & Assert
        Assert.Equal(cliSubscription, options.Subscription);
    }

    [Fact]
    public void ResolveSubscription_NoEnvironmentVariableParameterValueMatchingKnownPlaceholder_ReturnsParameterValue()
    {
        // Arrange
        var subscription = "<subscription_id>";
        var options = BindSubscriptionOption(CreateResolver(null, null), "--subscription", subscription);

        // Act & Assert
        Assert.Equal(subscription, options.Subscription);
    }

    private ISubscriptionResolver CreateResolver(string? cliSubscription, string? environmentSubscription)
    {
        Environment.SetEnvironmentVariable(EnvironmentHelpers.AzureSubscriptionIdEnvironmentVariable, environmentSubscription);

        // Set up a mock ISubscriptionResolver that delegates to the real SubscriptionResolver for testing purposes.
        var subscriptionResolver = Substitute.For<ISubscriptionResolver>();
        subscriptionResolver.GetProfileSubscriptionId().Returns(cliSubscription);
        subscriptionResolver.GetEnvironmentSubscriptionId().Returns(_real.GetEnvironmentSubscriptionId());
        subscriptionResolver.ResolveSubscription(Arg.Any<string?>()).Returns(args => SubscriptionResolver.ResolveSubscriptionInternal(subscriptionResolver, args.ArgAt<string?>(0)));

        return subscriptionResolver;
    }

    private static ISubscriptionOption BindSubscriptionOption(ISubscriptionResolver subscriptionResolver, params string[] args)
    {
        var command = new GroupListCommand(NullLogger<GroupListCommand>.Instance, Substitute.For<IResourceGroupService>(), subscriptionResolver);
        var commandDefinition = command.GetCommand();
        return command.BindOptions(commandDefinition.Parse(args));
    }
}
