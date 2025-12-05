// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.DeviceRegistry.Commands.Namespace;
using Azure.Mcp.Tools.DeviceRegistry.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.DeviceRegistry.UnitTests.Namespace;

public class NamespaceGetCommandTests : CommandTestBase<NamespaceGetCommand>
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
        Assert.Equal("7a4e8b2c-5d9f-4c1a-8e6f-2b5c9d7a3e1f", command.Id);
        Assert.Equal("get", command.Name);
        Assert.Equal("Get Device Registry Namespace", command.Title);
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
        Assert.Contains(cliCommand.Options, o => o.Name == "resource-group");
        Assert.Contains(cliCommand.Options, o => o.Name == "name");
    }
}
