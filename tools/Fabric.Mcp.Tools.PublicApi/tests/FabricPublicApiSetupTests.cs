// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.PublicApi.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Fabric.Mcp.Tools.PublicApi.Tests;

public class FabricPublicApiSetupTests
{
    [Fact]
    public void ConfigureServices_RegistersExpectedServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var setup = new FabricPublicApiSetup();

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
        var setup = new FabricPublicApiSetup();
        var rootGroup = new CommandGroup("root", "Root command group");

        // Act
        setup.RegisterCommands(rootGroup, loggerFactory);

        // Assert
        var fabricGroup = rootGroup.SubGroup.FirstOrDefault(g => g.Name == "fabric");
        Assert.NotNull(fabricGroup);
        
        var publicApiGroup = fabricGroup.SubGroup.FirstOrDefault(g => g.Name == "publicapi");
        Assert.NotNull(publicApiGroup);

        var apiSpecGroup = fabricGroup.SubGroup.FirstOrDefault(g => g.Name == "apispec");
        Assert.NotNull(apiSpecGroup);

        Assert.Contains("list", apiSpecGroup.Commands.Keys);
        Assert.Contains("details", apiSpecGroup.Commands.Keys);
    }
}
