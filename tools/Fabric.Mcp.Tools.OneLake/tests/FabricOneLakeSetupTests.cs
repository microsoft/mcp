// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Fabric.Mcp.Tools.OneLake.Tests;

public class FabricOneLakeSetupTests
{
    [Fact]
    public void ConfigureServices_RegistersAllServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var setup = new FabricOneLakeSetup();

        // Act
        setup.ConfigureServices(services);

        // Assert
        Assert.Contains(services, s => s.ServiceType == typeof(IOneLakeService));
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        // Arrange
        var setup = new FabricOneLakeSetup();

        // Act & Assert
        Assert.Equal("onelake", setup.Name);
    }
}