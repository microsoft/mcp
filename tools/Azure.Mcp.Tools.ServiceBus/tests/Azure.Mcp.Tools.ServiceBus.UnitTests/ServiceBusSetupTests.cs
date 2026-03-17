// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Tenant;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Commands;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ServiceBus.UnitTests;

public class ServiceBusSetupTests
{
    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Arrange & Act
        var setup = new ServiceBusSetup();

        // Assert
        Assert.NotNull(setup);
    }

    [Fact]
    public void RegisterCommands_WithValidParameters_ShouldSucceed()
    {
        // Arrange
        var setup = new ServiceBusSetup();
        var services = CreateServiceProvider(setup);

        // Act & Assert (should not throw)
        var result = setup.RegisterCommands(services);
        Assert.NotNull(result);
    }

    [Fact]
    public void RegisterCommands_ShouldReturnServiceBusGroup()
    {
        // Arrange
        var setup = new ServiceBusSetup();
        var services = CreateServiceProvider(setup);

        // Act
        var serviceBusGroup = setup.RegisterCommands(services);

        // Assert
        Assert.NotNull(serviceBusGroup);
        Assert.Equal("servicebus", serviceBusGroup.Name);
        Assert.NotNull(serviceBusGroup.Description);
        Assert.NotEmpty(serviceBusGroup.Description);
    }

    [Fact]
    public void RegisterCommands_ServiceBusGroup_ShouldHaveImprovedDescription()
    {
        // Arrange
        var setup = new ServiceBusSetup();
        var services = CreateServiceProvider(setup);

        // Act
        var serviceBusGroup = setup.RegisterCommands(services);

        // Assert
        Assert.NotNull(serviceBusGroup);

        // Verify key terms are present in the description
        Assert.Contains("Service Bus", serviceBusGroup.Description);
        Assert.Contains("asynchronous messaging", serviceBusGroup.Description);
        Assert.Contains("enterprise integration", serviceBusGroup.Description);
        Assert.Contains("point-to-point", serviceBusGroup.Description);
        Assert.Contains("publish-subscribe", serviceBusGroup.Description);
        Assert.Contains("dead-letter handling", serviceBusGroup.Description);
        Assert.Contains("decoupled architectures", serviceBusGroup.Description);

        Assert.Contains("decoupled architectures", serviceBusGroup.Description);
        Assert.Contains("intended for real-time communication", serviceBusGroup.Description);
        Assert.Contains("direct API calls", serviceBusGroup.Description);
        Assert.Contains("database", serviceBusGroup.Description);

        // Verify MCP router information
        Assert.Contains("hierarchical MCP command model", serviceBusGroup.Description);
        Assert.Contains("learn=true", serviceBusGroup.Description);
    }

    [Fact]
    public void RegisterCommands_ShouldAddQueueSubGroup()
    {
        // Arrange
        var setup = new ServiceBusSetup();
        var services = CreateServiceProvider(setup);

        // Act
        var serviceBusGroup = setup.RegisterCommands(services);

        // Assert
        Assert.NotNull(serviceBusGroup);

        var queueGroup = serviceBusGroup.SubGroup.FirstOrDefault(g => g.Name == "queue");
        Assert.NotNull(queueGroup);
        Assert.Equal("queue", queueGroup.Name);
        Assert.Contains("Queue operations", queueGroup.Description);
    }

    [Fact]
    public void RegisterCommands_ShouldAddTopicSubGroup()
    {
        // Arrange
        var setup = new ServiceBusSetup();
        var services = CreateServiceProvider(setup);

        // Act
        var serviceBusGroup = setup.RegisterCommands(services);

        // Assert
        Assert.NotNull(serviceBusGroup);

        var topicGroup = serviceBusGroup.SubGroup.FirstOrDefault(g => g.Name == "topic");
        Assert.NotNull(topicGroup);
        Assert.Equal("topic", topicGroup.Name);
        Assert.Contains("Topic operations", topicGroup.Description);
    }

    [Fact]
    public void RegisterCommands_TopicGroup_ShouldHaveSubscriptionSubGroup()
    {
        // Arrange
        var setup = new ServiceBusSetup();
        var services = CreateServiceProvider(setup);

        // Act
        var serviceBusGroup = setup.RegisterCommands(services);

        // Assert
        Assert.NotNull(serviceBusGroup);

        var topicGroup = serviceBusGroup.SubGroup.FirstOrDefault(g => g.Name == "topic");
        Assert.NotNull(topicGroup);

        var subscriptionGroup = topicGroup.SubGroup.FirstOrDefault(g => g.Name == "subscription");
        Assert.NotNull(subscriptionGroup);
        Assert.Equal("subscription", subscriptionGroup.Name);
        Assert.Contains("Subscription operations", subscriptionGroup.Description);
    }

    [Fact]
    public void RegisterCommands_WithNullServiceProvider_ShouldThrow()
    {
        // Arrange
        var setup = new ServiceBusSetup();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => setup.RegisterCommands(null!));
    }

    [Fact]
    public void ServiceBusSetup_TypeValidation_ShouldHaveCorrectProperties()
    {
        // Act
        var type = typeof(ServiceBusSetup);

        // Assert
        Assert.True(type.IsClass);
        Assert.False(type.IsAbstract);
        Assert.False(type.IsInterface);

        // Verify it has a public constructor
        var constructors = type.GetConstructors();
        Assert.NotEmpty(constructors);

        // Verify it has the RegisterCommands method with the correct signature
        var registerMethod = type.GetMethod("RegisterCommands", new[] { typeof(IServiceProvider) });
        Assert.NotNull(registerMethod);
        Assert.True(registerMethod.IsPublic);
        Assert.Equal(typeof(CommandGroup), registerMethod.ReturnType);
    }

    private static IServiceProvider CreateServiceProvider(ServiceBusSetup setup)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        // Add required dependencies
        services.AddSingleton(Substitute.For<ITenantService>());
        setup.ConfigureServices(services);
        return services.BuildServiceProvider();
    }
}
