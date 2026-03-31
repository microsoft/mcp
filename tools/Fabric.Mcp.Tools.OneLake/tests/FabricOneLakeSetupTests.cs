// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Fabric.Mcp.Tools.OneLake.Tests;

public sealed class FabricOneLakeRegistrationTests
{
    private readonly FabricOneLakeSetup _setup = new();
    [Fact]
    public void ConfigureServices_RegistersAllServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        _setup.ConfigureServices(services);

        // Assert
        Assert.Contains(services, s => s.ServiceType == typeof(IOneLakeService));
    }

    [Fact]
    public void AreaName_ReturnsCorrectValue()
    {
        // Act & Assert
        Assert.Equal("onelake", _setup.CommandDescriptors.Name);
    }

    [Fact]
    public void GetCommandDescriptors_RegistersAllOneLakeCommands()
    {
        // Act
        var descriptor = _setup.CommandDescriptors;

        // Assert - flat structure with verb_object naming
        Assert.NotNull(descriptor);
        Assert.Equal("onelake", descriptor.Name);
        Assert.Contains(descriptor.Commands, c => c.Name == "list_workspaces");
        Assert.Contains(descriptor.Commands, c => c.Name == "list_items");
        Assert.Contains(descriptor.Commands, c => c.Name == "list_items_dfs");
        Assert.Contains(descriptor.Commands, c => c.Name == "list_files");
        Assert.Contains(descriptor.Commands, c => c.Name == "download_file");
        Assert.Contains(descriptor.Commands, c => c.Name == "upload_file");
        Assert.Contains(descriptor.Commands, c => c.Name == "delete_file");
        Assert.Contains(descriptor.Commands, c => c.Name == "create_directory");
        Assert.Contains(descriptor.Commands, c => c.Name == "delete_directory");

        // Table commands
        Assert.Contains(descriptor.Commands, c => c.Name == "get_table_config");
        Assert.Contains(descriptor.Commands, c => c.Name == "list_tables");
        Assert.Contains(descriptor.Commands, c => c.Name == "get_table");
        Assert.Contains(descriptor.Commands, c => c.Name == "list_table_namespaces");
        Assert.Contains(descriptor.Commands, c => c.Name == "get_table_namespace");
    }
}
