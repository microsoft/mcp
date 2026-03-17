// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Core.Services;
using Microsoft.Extensions.DependencyInjection;

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
        Assert.True(rootGroup.Commands.ContainsKey("create_item"), "Should have create_item command");
        Assert.Single(rootGroup.Commands);
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
