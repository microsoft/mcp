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
        Assert.True(rootGroup.Commands.ContainsKey("list_workspaces"), "Should have list_workspaces command");
        Assert.True(rootGroup.Commands.ContainsKey("list_items"), "Should have list_items command");
        Assert.True(rootGroup.Commands.ContainsKey("list_items_dfs"), "Should have list_items_dfs command");
        Assert.True(rootGroup.Commands.ContainsKey("list_files"), "Should have list_files command");
        Assert.True(rootGroup.Commands.ContainsKey("download_file"), "Should have download_file command");
        Assert.True(rootGroup.Commands.ContainsKey("upload_file"), "Should have upload_file command");
        Assert.True(rootGroup.Commands.ContainsKey("delete_file"), "Should have delete_file command");
        Assert.True(rootGroup.Commands.ContainsKey("create_directory"), "Should have create_directory command");
        Assert.True(rootGroup.Commands.ContainsKey("delete_directory"), "Should have delete_directory command");

        // Table commands
        Assert.True(rootGroup.Commands.ContainsKey("get_table_config"), "Should have get_table_config command");
        Assert.True(rootGroup.Commands.ContainsKey("list_tables"), "Should have list_tables command");
        Assert.True(rootGroup.Commands.ContainsKey("get_table"), "Should have get_table command");
        Assert.True(rootGroup.Commands.ContainsKey("list_table_namespaces"), "Should have list_table_namespaces command");
        Assert.True(rootGroup.Commands.ContainsKey("get_table_namespace"), "Should have get_table_namespace command");

        // Shortcut commands
        Assert.True(rootGroup.Commands.ContainsKey("list_shortcuts"), "Should have list_shortcuts command");
        Assert.True(rootGroup.Commands.ContainsKey("get_shortcut"), "Should have get_shortcut command");
        Assert.True(rootGroup.Commands.ContainsKey("delete_shortcut"), "Should have delete_shortcut command");
        Assert.True(rootGroup.Commands.ContainsKey("reset_shortcut_cache"), "Should have reset_shortcut_cache command");
        Assert.True(rootGroup.Commands.ContainsKey("create_shortcut_onelake"), "Should have create_shortcut_onelake command");
        Assert.True(rootGroup.Commands.ContainsKey("create_shortcut_adls_gen2"), "Should have create_shortcut_adls_gen2 command");
        Assert.True(rootGroup.Commands.ContainsKey("create_shortcut_amazon_s3"), "Should have create_shortcut_amazon_s3 command");
        Assert.True(rootGroup.Commands.ContainsKey("create_shortcut_azure_blob"), "Should have create_shortcut_azure_blob command");
        Assert.True(rootGroup.Commands.ContainsKey("create_shortcut_gcs"), "Should have create_shortcut_gcs command");
        Assert.True(rootGroup.Commands.ContainsKey("create_shortcut_s3_compatible"), "Should have create_shortcut_s3_compatible command");
        Assert.True(rootGroup.Commands.ContainsKey("create_shortcut_dataverse"), "Should have create_shortcut_dataverse command");
        Assert.True(rootGroup.Commands.ContainsKey("create_shortcut_onedrive_sharepoint"), "Should have create_shortcut_onedrive_sharepoint command");
    }
}
