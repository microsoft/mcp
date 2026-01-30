// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Commands;

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

    [Fact]
    public void RegisterCommands_RegistersTableCommands()
    {
        var services = new ServiceCollection();
        var setup = new FabricOneLakeSetup();
        setup.ConfigureServices(services);
        using var provider = services.BuildServiceProvider();

        var rootGroup = setup.RegisterCommands(provider);

        var tableGroup = rootGroup.SubGroup.FirstOrDefault(g => g.Name == "table");
        Assert.NotNull(tableGroup);

        var configGroup = tableGroup!.SubGroup.FirstOrDefault(g => g.Name == "config");
        Assert.NotNull(configGroup);

        Assert.True(configGroup!.Commands.ContainsKey("get"));

        Assert.True(tableGroup.Commands.ContainsKey("list"));
    Assert.True(tableGroup.Commands.ContainsKey("get"));

        var namespaceGroup = tableGroup.SubGroup.FirstOrDefault(g => g.Name == "namespace");
        Assert.NotNull(namespaceGroup);

        Assert.True(namespaceGroup!.Commands.ContainsKey("list"));
        Assert.True(namespaceGroup.Commands.ContainsKey("get"));
    }
}
