// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.DataFactory;
using Fabric.Mcp.Tools.DataFactory.Commands.Dataflow;
using Fabric.Mcp.Tools.DataFactory.Commands.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace Fabric.Mcp.Tools.DataFactory.UnitTests;

public class DataFactoryAreaSetupTests
{
    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        // Arrange
        var setup = new DataFactoryAreaSetup();

        // Act & Assert
        Assert.Equal("datafactory", setup.Name);
    }

    [Fact]
    public void Title_ReturnsCorrectValue()
    {
        // Arrange
        var setup = new DataFactoryAreaSetup();

        // Act & Assert
        Assert.Equal("Microsoft Fabric Data Factory", setup.Title);
    }

    [Fact]
    public void ConfigureServices_RegistersCommandTypes()
    {
        // Arrange
        var services = new ServiceCollection();
        var setup = new DataFactoryAreaSetup();

        // Act
        setup.ConfigureServices(services);

        // Assert
        Assert.Contains(services, s => s.ServiceType == typeof(ListPipelinesCommand));
        Assert.Contains(services, s => s.ServiceType == typeof(CreatePipelineCommand));
        Assert.Contains(services, s => s.ServiceType == typeof(GetPipelineCommand));
        Assert.Contains(services, s => s.ServiceType == typeof(RunPipelineCommand));
        Assert.Contains(services, s => s.ServiceType == typeof(ListDataflowsCommand));
        Assert.Contains(services, s => s.ServiceType == typeof(CreateDataflowCommand));
        Assert.Contains(services, s => s.ServiceType == typeof(ExecuteQueryCommand));
    }
}
