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
        setup.ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var commands = setup.RegisterCommands(serviceProvider);
        rootGroup.AddSubGroup(commands);

        // Assert
        var publicApisGroup = rootGroup.SubGroup.FirstOrDefault(g => g.Name == "docs");
        Assert.NotNull(publicApisGroup);

        var bestPracticesGroup = publicApisGroup.SubGroup.FirstOrDefault(g => g.Name == "bestpractices");
        Assert.NotNull(bestPracticesGroup);

        Assert.Contains("list", publicApisGroup.Commands.Keys);
        Assert.Contains("get", publicApisGroup.Commands.Keys);
        Assert.Contains("get", bestPracticesGroup.Commands.Keys);
    }
}
