// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Commands.Discovery;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tests.Client.Helpers;
using ModelContextProtocol.Client;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Commands.Discovery;

public class CommandGroupServerProviderTests
{
    private readonly CommandFactory _commandFactory;
    public CommandGroupServerProviderTests()
    {
        _commandFactory = CommandFactoryHelpers.CreateCommandFactory();
    }

    [Fact]
    public void CreateMetadata_ReturnsExpectedMetadata()
    {
        // Arrange
        // For testGroup, CommandFactory does not have it by default, so fallback to direct instantiation
        var commandGroup = new CommandGroup("testGroup", "Test Description");
        var mcpCommandGroup = new CommandGroupServerProvider(commandGroup);

        // Act
        var metadata = mcpCommandGroup.CreateMetadata();

        // Assert
        Assert.Equal("testGroup", metadata.Id);
        Assert.Equal("testGroup", metadata.Name);
        Assert.Equal("Test Description", metadata.Description);
    }

    [Fact]
    public async Task CreateClientAsync_ReturnsClientInstance()
    {
        // Arrange
        // Use CommandFactory to get the storage command group
        var storageGroup = _commandFactory.RootGroup.SubGroup.FirstOrDefault(g => g.Name == "storage");
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

    [Fact]
    public void ReadOnly_Property_DefaultsToFalse()
    {
        // Arrange
        var storageGroup = _commandFactory.RootGroup.SubGroup.First(g => g.Name == "storage");

        // Act
        var mcpCommandGroup = new CommandGroupServerProvider(storageGroup);

        // Assert
        Assert.False(mcpCommandGroup.ReadOnly);
    }

    [Fact]
    public void ReadOnly_Property_CanBeSet()
    {
        // Arrange
        var storageGroup = _commandFactory.RootGroup.SubGroup.First(g => g.Name == "storage");
        var mcpCommandGroup = new CommandGroupServerProvider(storageGroup);

        // Act
        mcpCommandGroup.ReadOnly = true;

        // Assert
        Assert.True(mcpCommandGroup.ReadOnly);
    }

    [Fact]
    public void EntryPoint_SetToNull_UsesDefault()
    {
        // Arrange
        var storageGroup = _commandFactory.RootGroup.SubGroup.First(g => g.Name == "storage");
        var mcpCommandGroup = new CommandGroupServerProvider(storageGroup);
        var originalEntryPoint = mcpCommandGroup.EntryPoint;
        // Act
        mcpCommandGroup.EntryPoint = null!;

        // Assert
        Assert.Equal(originalEntryPoint, mcpCommandGroup.EntryPoint);
        Assert.False(string.IsNullOrWhiteSpace(mcpCommandGroup.EntryPoint));
    }

    [Fact]
    public void EntryPoint_SetToEmpty_UsesDefault()
    {
        // Arrange
        var storageGroup = _commandFactory.RootGroup.SubGroup.First(g => g.Name == "storage");
        var mcpCommandGroup = new CommandGroupServerProvider(storageGroup);
        var originalEntryPoint = mcpCommandGroup.EntryPoint;

        // Act
        mcpCommandGroup.EntryPoint = "";

        // Assert
        Assert.Equal(originalEntryPoint, mcpCommandGroup.EntryPoint);
        Assert.False(string.IsNullOrWhiteSpace(mcpCommandGroup.EntryPoint));
    }

    [Fact]
    public void EntryPoint_SetToValidValue_UsesProvidedValue()
    {
        // Arrange
        var storageGroup = _commandFactory.RootGroup.SubGroup.First(g => g.Name == "storage");
        var mcpCommandGroup = new CommandGroupServerProvider(storageGroup);
        var customEntryPoint = "/custom/path/to/executable";

        // Act
        mcpCommandGroup.EntryPoint = customEntryPoint;

        // Assert
        Assert.Equal(customEntryPoint, mcpCommandGroup.EntryPoint);
    }

    [Fact]
    public void BuildArguments_WithoutReadOnly_ReturnsBasicArguments()
    {
        // Arrange
        var commandGroup = new CommandGroup("testGroup", "Test Description");
        var provider = new CommandGroupServerProvider(commandGroup);
        provider.ReadOnly = false;

        // Act
        var arguments = provider.BuildArguments();

        // Assert
        var expected = new[] { "server", "start", "--mode", "all", "--namespace", "testGroup" };
        Assert.Equal(expected, arguments);
    }

    [Fact]
    public void BuildArguments_WithReadOnly_IncludesReadOnlyFlag()
    {
        // Arrange
        var commandGroup = new CommandGroup("testGroup", "Test Description");
        var provider = new CommandGroupServerProvider(commandGroup);
        provider.ReadOnly = true;

        // Act
        var arguments = provider.BuildArguments();

        // Assert
        var expected = new[] { "server", "start", "--mode", "all", "--namespace", "testGroup", "--read-only" };
        Assert.Equal(expected, arguments);
    }
}
