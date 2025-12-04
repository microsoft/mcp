// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.DeviceRegistry.Commands.Namespace;
using Azure.Mcp.Tools.DeviceRegistry.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.DeviceRegistry.UnitTests.Namespace;

public class NamespaceCreateCommandTests : CommandTestBase<NamespaceCreateCommand>
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
        Assert.Equal("9c2b3f7a-6d8e-4a1c-9e5f-3b7c8d6a2e4f", command.Id);
        Assert.Equal("create", command.Name);
        Assert.Equal("Create Device Registry Namespace", command.Title);
        Assert.False(command.Metadata.Destructive);
        Assert.True(command.Metadata.Idempotent);
        Assert.False(command.Metadata.ReadOnly);
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
        Assert.Contains(cliCommand.Options, o => o.Name == "location");
    }

    [Fact]
    public void Command_HasOptionalTagsOption()
    {
        // Arrange & Act
        var command = GetCommand();
        var cliCommand = command.Build();

        // Assert
        Assert.Contains(cliCommand.Options, o => o.Name == "tags");
    }
}
