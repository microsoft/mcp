// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Commands.Shortcut;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Mcp.Tests.Client;

namespace Fabric.Mcp.Tools.OneLake.Tests.Commands.Shortcut;

public class ShortcutDeleteCommandTests : CommandUnitTestsBase<ShortcutDeleteCommand, IOneLakeService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("delete_shortcut", Command.Name);
        Assert.Equal("Delete OneLake Shortcut", Command.Title);
        Assert.Contains("Delete a single shortcut from an item", Command.Description);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.Idempotent);
    }

    [Fact]
    public void GetCommand_ReturnsValidCommand()
    {
        Assert.Equal("delete_shortcut", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Options);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ShortcutDeleteCommand(null!, Service));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOneLakeServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ShortcutDeleteCommand(Logger, null!));
    }

    [Fact]
    public void Metadata_HasCorrectProperties()
    {
        var metadata = Command.Metadata;

        Assert.True(metadata.Destructive);
        Assert.True(metadata.Idempotent);
        Assert.False(metadata.LocalRequired);
        Assert.False(metadata.OpenWorld);
        Assert.False(metadata.ReadOnly);
        Assert.False(metadata.Secret);
    }
}
