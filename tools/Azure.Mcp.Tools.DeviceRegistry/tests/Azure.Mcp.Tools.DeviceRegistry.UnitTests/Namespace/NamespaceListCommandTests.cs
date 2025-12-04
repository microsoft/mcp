// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.DeviceRegistry.Commands.Namespace;
using Azure.Mcp.Tools.DeviceRegistry.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.DeviceRegistry.UnitTests.Namespace;

public class NamespaceListCommandTests : CommandTestBase<NamespaceListCommand>
{
    protected override void RegisterServices(IServiceCollection services)
    {
        base.RegisterServices(services);
        services.AddSingleton<IDeviceRegistryService, DeviceRegistryService>();
    }

    [Fact]
    public void Command_HasCorrectMetadata()
    {
        // Arrange & Act
        var command = GetCommand();

        // Assert
        Assert.Equal("8f3d2a5c-4b1e-4c9a-8d7f-2e5b6c9a4d3f", command.Id);
        Assert.Equal("list", command.Name);
        Assert.Equal("List Device Registry Namespaces", command.Title);
        Assert.False(command.Metadata.Destructive);
        Assert.True(command.Metadata.Idempotent);
        Assert.True(command.Metadata.ReadOnly);
        Assert.False(command.Metadata.LocalRequired);
    }

    [Fact]
    public void Command_HasRequiredOptions()
    {
        // Arrange & Act
        var command = GetCommand();
        var cliCommand = command.Build();

        // Assert
        Assert.Contains(cliCommand.Options, o => o.Name == "subscription");
    }

    [Fact]
    public void Command_HasOptionalResourceGroupOption()
    {
        // Arrange & Act
        var command = GetCommand();
        var cliCommand = command.Build();

        // Assert
        Assert.Contains(cliCommand.Options, o => o.Name == "resource-group");
    }
}
