// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Docs.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Fabric.Mcp.Tools.Docs.Tests;

public class FabricDocsSetupTests
{
    [Fact]
    public void ConfigureServices_RegistersExpectedServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var setup = new FabricDocsSetup();

        // Act
        setup.ConfigureServices(services);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var fabricService = serviceProvider.GetService<IFabricPublicApiService>();

        Assert.NotNull(fabricService);
        Assert.IsType<FabricPublicApiService>(fabricService);
    }

    [Fact]
    public void RegisterCommands_CreatesExpectedCommandStructure()
    {
        // Arrange
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var setup = new FabricDocsSetup();
        var rootGroup = new CommandGroup("root", "Root command group");
        var services = new ServiceCollection();
        services.AddLogging();
        setup.ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var commands = setup.RegisterCommands(serviceProvider);
        rootGroup.AddSubGroup(commands);

        // Assert - flat structure under docs
        var docsGroup = rootGroup.SubGroup.FirstOrDefault(g => g.Name == "docs");
        Assert.NotNull(docsGroup);

        // Verify all 6 commands are registered with verb_object naming
        Assert.Contains("list_workloads", docsGroup.Commands.Keys);
        Assert.Contains("get_api_spec", docsGroup.Commands.Keys);
        Assert.Contains("get_platform_api_spec", docsGroup.Commands.Keys);
        Assert.Contains("get_item_definition", docsGroup.Commands.Keys);
        Assert.Contains("get_best_practices", docsGroup.Commands.Keys);
        Assert.Contains("get_examples", docsGroup.Commands.Keys);
    }
}
