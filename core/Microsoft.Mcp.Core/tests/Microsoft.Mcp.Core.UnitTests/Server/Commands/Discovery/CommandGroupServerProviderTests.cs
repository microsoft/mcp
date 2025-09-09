// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Areas.Server.Commands.Discovery;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.UnitTests.Server.Helpers;
using Xunit;

namespace Microsoft.Mcp.Core.UnitTests.Areas.Server.Commands.Discovery;

public class CommandGroupServerProviderTests
{
    private readonly MockCommandFactory _commandFactory = new MockCommandFactory();
    private readonly CommandGroup _subGroupA = new CommandGroup("A", "A_description");
    private readonly CommandGroup _subGroupB = new CommandGroup("B", "B_description");
    private readonly CommandGroup _subGroupC = new CommandGroup("C", "C_description");
    private readonly CommandGroup _extensionSubGroup = new CommandGroup("extension", "extensions should be ignored.");
    private readonly CommandGroup _serverSubGroup = new CommandGroup("server", "server should be ignored here.");

    public CommandGroupServerProviderTests()
    {
        _commandFactory.RootGroup.SubGroup.Add(_subGroupA);
        _commandFactory.RootGroup.SubGroup.Add(_subGroupB);
        _commandFactory.RootGroup.SubGroup.Add(_subGroupC);
        _commandFactory.RootGroup.SubGroup.Add(_extensionSubGroup);
        _commandFactory.RootGroup.SubGroup.Add(_serverSubGroup);
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
    public void ReadOnly_Property_DefaultsToFalse()
    {
        // Arrange
        var storageGroup = _commandFactory.RootGroup.SubGroup.First(g => g.Name == _subGroupB.Name);

        // Act
        var mcpCommandGroup = new CommandGroupServerProvider(storageGroup);

        // Assert
        Assert.False(mcpCommandGroup.ReadOnly);
    }

    [Fact]
    public void ReadOnly_Property_CanBeSet()
    {
        // Arrange
        var storageGroup = _commandFactory.RootGroup.SubGroup.First(g => g.Name == _subGroupB.Name);
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
        var storageGroup = _commandFactory.RootGroup.SubGroup.First(g => g.Name == _subGroupC.Name);
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
        var storageGroup = _commandFactory.RootGroup.SubGroup.First(g => g.Name == _subGroupA.Name);
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
        var storageGroup = _commandFactory.RootGroup.SubGroup.First(g => g.Name == _subGroupB.Name);
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
