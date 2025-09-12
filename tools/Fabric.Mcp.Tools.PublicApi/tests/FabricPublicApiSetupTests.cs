// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Services.Http;
using Fabric.Mcp.Tools.PublicApi.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fabric.Mcp.Tools.PublicApi.Tests;

public class FabricPublicApiSetupTests
{
    [Fact]
    public void ConfigureServices_RegistersExpectedServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(Substitute.For<IHttpClientService>());
        services.AddLogging();
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
        var publicApisGroup = rootGroup.SubGroup.FirstOrDefault(g => g.Name == "publicapis");
        Assert.NotNull(publicApisGroup);

        var bestPracticesGroup = publicApisGroup.SubGroup.FirstOrDefault(g => g.Name == "bestpractices");
        Assert.NotNull(bestPracticesGroup);

        Assert.Contains("list", publicApisGroup.Commands.Keys);
        Assert.Contains("get", publicApisGroup.Commands.Keys);
        Assert.Contains("get", bestPracticesGroup.Commands.Keys);
    }
}
