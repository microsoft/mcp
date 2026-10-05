// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests;

public class FabricCoreSetupTests
{
    [Fact]
    public void ConfigureServices_RegistersAllServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var setup = new FabricCoreSetup();

        // Act
        setup.ConfigureServices(services);

        // Assert
        Assert.Contains(services, s => s.ServiceType == typeof(IFabricCoreService));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(CapacityGetCommand) && descriptor.Lifetime == ServiceLifetime.Singleton);
        Assert.Contains(services, s => s.ServiceType == typeof(CapacityListCommand) && s.Lifetime == ServiceLifetime.Singleton);
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(WorkspaceGetCommand) && descriptor.Lifetime == ServiceLifetime.Singleton);
        Assert.Contains(services, s => s.ServiceType == typeof(WorkspaceListCommand) && s.Lifetime == ServiceLifetime.Singleton);
        Assert.Contains(services, s => s.ServiceType == typeof(ItemListCommand) && s.Lifetime == ServiceLifetime.Singleton);
        Assert.Contains(services, s => s.ServiceType == typeof(WorkspaceCreateCommand) && s.Lifetime == ServiceLifetime.Singleton);
        Assert.Contains(services, s => s.ServiceType == typeof(WorkspaceUpdateCommand) && s.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        // Arrange
        var setup = new FabricCoreSetup();

        // Act & Assert
        Assert.Equal("core", setup.Name);
    }

    [Fact]
    public void RegisterCommands_RegistersCoreCommands()
    {
        // Arrange
        var services = new ServiceCollection();
        var setup = new FabricCoreSetup();
        setup.ConfigureServices(services);
        using var provider = services.BuildServiceProvider();

        // Act
        var rootGroup = setup.RegisterCommands(provider);

        // Assert
        Assert.True(rootGroup.Commands.ContainsKey("create-item"), "Should have create-item command");
        Assert.True(rootGroup.Commands.ContainsKey("search-catalog"), "Should have search-catalog command");
        Assert.True(rootGroup.Commands.ContainsKey("get-capacity"), "Should have get-capacity command");
        Assert.True(rootGroup.Commands.ContainsKey("list-capacities"), "Should have list-capacities command");
        Assert.True(rootGroup.Commands.ContainsKey("get-workspace"), "Should have get-workspace command");
        Assert.True(rootGroup.Commands.ContainsKey("list-workspaces"), "Should have list-workspaces command");
        Assert.True(rootGroup.Commands.ContainsKey("list-items"), "Should have list-items command");
        Assert.True(rootGroup.Commands.ContainsKey("create-workspace"), "Should have create-workspace command");
        Assert.True(rootGroup.Commands.ContainsKey("update-workspace"), "Should have update-workspace command");
        Assert.Equal(9, rootGroup.Commands.Count);
    }

    [Fact]
    public void Title_ReturnsCorrectValue()
    {
        // Arrange
        var setup = new FabricCoreSetup();

        // Act & Assert
        Assert.Equal("Microsoft Fabric Core", setup.Title);
    }
}
