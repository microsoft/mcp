// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Core.Areas.Server.Commands.Discovery;
using ModelContextProtocol.Client;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Commands.Discovery;

public class CommandGroupServerProviderTests
{

    [Fact]
    public async Task CreateClientAsync_ReturnsClientInstance()
    {
        // Arrange
        var commandFactory = CommandFactoryHelpers.CreateCommandFactory();

        // Use CommandFactory to get the storage command group
        var storageGroup = commandFactory.RootGroup.SubGroup.FirstOrDefault(g => g.Name == "storage");
        Assert.NotNull(storageGroup);

        // Use the built azmcp.exe as the entry point for testing (should be in the same directory as the test exe)
        var entryPoint = McpTestUtilities.GetAzMcpExecutablePath();
        Assert.True(File.Exists(entryPoint), $"azmcp executable not found at {entryPoint}");

        var mcpCommandGroup = new CommandGroupServerProvider(storageGroup);
        mcpCommandGroup.EntryPoint = entryPoint;
        var options = new McpClientOptions();

        // Act
        var client = await mcpCommandGroup.CreateClientAsync(options);

        // Assert
        Assert.NotNull(client);

        await client.DisposeAsync();
    }
}
