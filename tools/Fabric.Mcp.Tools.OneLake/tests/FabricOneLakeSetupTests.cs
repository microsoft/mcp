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
    public void RegisterCommands_RegistersAllOneLakeCommands()
    {
        // Arrange
        var services = new ServiceCollection();
        var setup = new FabricOneLakeSetup();
        setup.ConfigureServices(services);
        using var provider = services.BuildServiceProvider();

        // Act
        var rootGroup = setup.RegisterCommands(provider);

        // Assert - flat structure with verb_object naming
        Assert.True(rootGroup.Commands.ContainsKey("list-workspaces"), "Should have list-workspaces command");
        Assert.True(rootGroup.Commands.ContainsKey("list-items"), "Should have list-items command");
        Assert.True(rootGroup.Commands.ContainsKey("list-items-dfs"), "Should have list-items-dfs command");
        Assert.True(rootGroup.Commands.ContainsKey("list-files"), "Should have list-files command");
        Assert.True(rootGroup.Commands.ContainsKey("download-file"), "Should have download-file command");
        Assert.True(rootGroup.Commands.ContainsKey("upload-file"), "Should have upload-file command");
        Assert.True(rootGroup.Commands.ContainsKey("delete-file"), "Should have delete-file command");
        Assert.True(rootGroup.Commands.ContainsKey("create-directory"), "Should have create-directory command");
        Assert.True(rootGroup.Commands.ContainsKey("delete-directory"), "Should have delete-directory command");

        // Table commands
        Assert.True(rootGroup.Commands.ContainsKey("get-table-config"), "Should have get-table-config command");
        Assert.True(rootGroup.Commands.ContainsKey("list-tables"), "Should have list-tables command");
        Assert.True(rootGroup.Commands.ContainsKey("get-table"), "Should have get-table command");
        Assert.True(rootGroup.Commands.ContainsKey("list-table-namespaces"), "Should have list-table-namespaces command");
        Assert.True(rootGroup.Commands.ContainsKey("get-table-namespace"), "Should have get-table-namespace command");
    }
}
