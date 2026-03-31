// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Docs.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Fabric.Mcp.Tools.Docs.Tests;

public sealed class FabricDocsSetupTests
{
    private readonly FabricDocsSetup _setup = new();
    [Fact]
    public void ConfigureServices_RegistersExpectedServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        _setup.ConfigureServices(services);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var fabricService = serviceProvider.GetService<IFabricPublicApiService>();

        Assert.NotNull(fabricService);
        Assert.IsType<FabricPublicApiService>(fabricService);
    }

    [Fact]
    public void GetCommandDescriptors_CreatesExpectedCommandStructure()
    {
        // Act
        var descriptor = _setup.CommandDescriptors;

        // Assert - flat structure under docs
        Assert.NotNull(descriptor);
        Assert.Equal("docs", descriptor.Name);

        // Verify all 6 commands are registered with noun-based naming
        Assert.Contains(descriptor.Commands, c => c.Name == "workloads");
        Assert.Contains(descriptor.Commands, c => c.Name == "workload-api-spec");
        Assert.Contains(descriptor.Commands, c => c.Name == "platform-api-spec");
        Assert.Contains(descriptor.Commands, c => c.Name == "item-definitions");
        Assert.Contains(descriptor.Commands, c => c.Name == "best-practices");
        Assert.Contains(descriptor.Commands, c => c.Name == "api-examples");
    }

    [Fact]
    public void AreaName_ReturnsCorrectValue()
    {
        // Act & Assert
        Assert.Equal("docs", _setup.CommandDescriptors.Name);
    }

    [Fact]
    public void AreaTitle_ReturnsCorrectValue()
    {
        // Act & Assert
        Assert.Equal("Microsoft Fabric Documentation", _setup.CommandDescriptors.Title);
    }
}
