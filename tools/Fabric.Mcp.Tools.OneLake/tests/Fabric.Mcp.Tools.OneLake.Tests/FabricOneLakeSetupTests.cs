// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

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

        // Assert - flat structure with kebab-case verb-object naming
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

        // Shortcut commands
        Assert.True(rootGroup.Commands.ContainsKey("list-shortcuts"), "Should have list-shortcuts command");
        Assert.True(rootGroup.Commands.ContainsKey("get-shortcut"), "Should have get-shortcut command");
        Assert.True(rootGroup.Commands.ContainsKey("delete-shortcut"), "Should have delete-shortcut command");
        Assert.True(rootGroup.Commands.ContainsKey("reset-shortcut-cache"), "Should have reset-shortcut-cache command");
        Assert.True(rootGroup.Commands.ContainsKey("create-shortcut-onelake"), "Should have create-shortcut-onelake command");
        Assert.True(rootGroup.Commands.ContainsKey("create-shortcut-adls-gen2"), "Should have create-shortcut-adls-gen2 command");
        Assert.True(rootGroup.Commands.ContainsKey("create-shortcut-amazon-s3"), "Should have create-shortcut-amazon-s3 command");
        Assert.True(rootGroup.Commands.ContainsKey("create-shortcut-azure-blob"), "Should have create-shortcut-azure-blob command");
        Assert.True(rootGroup.Commands.ContainsKey("create-shortcut-gcs"), "Should have create-shortcut-gcs command");
        Assert.True(rootGroup.Commands.ContainsKey("create-shortcut-s3-compatible"), "Should have create-shortcut-s3-compatible command");
        Assert.True(rootGroup.Commands.ContainsKey("create-shortcut-dataverse"), "Should have create-shortcut-dataverse command");
        Assert.True(rootGroup.Commands.ContainsKey("create-shortcut-onedrive-sharepoint"), "Should have create-shortcut-onedrive-sharepoint command");
    }
}
