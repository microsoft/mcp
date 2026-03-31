// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Fabric.Mcp.Tools.Core.Tests;

public sealed class FabricCoreSetupTests
{
    private readonly FabricCoreSetup _setup = new();
    [Fact]
    public void ConfigureServices_RegistersAllServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        _setup.ConfigureServices(services);

        // Assert
        Assert.Contains(services, s => s.ServiceType == typeof(IFabricCoreService));
    }

    [Fact]
    public void AreaName_ReturnsCorrectValue()
    {
        // Act & Assert
        Assert.Equal("core", _setup.CommandDescriptors.Name);
    }

    [Fact]
    public void GetCommandDescriptors_RegistersCoreCommands()
    {
        // Act
        var descriptor = _setup.CommandDescriptors;

        // Assert
        Assert.NotNull(descriptor);
        Assert.Equal("core", descriptor.Name);
        Assert.Single(descriptor.Commands);
        Assert.Contains(descriptor.Commands, c => c.Name == "create-item");
    }

    [Fact]
    public void AreaTitle_ReturnsCorrectValue()
    {
        // Act & Assert
        Assert.Equal("Microsoft Fabric Core", _setup.CommandDescriptors.Title);
    }
}
