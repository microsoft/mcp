// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Commands.Shortcut;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using Xunit;

namespace Fabric.Mcp.Tools.OneLake.Tests.Commands.Shortcut;

public class ShortcutGetCommandTests : CommandUnitTestsBase<ShortcutGetCommand, IOneLakeService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get-shortcut", Command.Name);
        Assert.Equal("Get OneLake Shortcut", Command.Title);
        Assert.Contains("Get the properties of a single shortcut", Command.Description);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.Idempotent);
    }

    [Fact]
    public void GetCommand_ReturnsValidCommand()
    {
        Assert.Equal("get-shortcut", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Options);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ShortcutGetCommand(null!, Service));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOneLakeServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ShortcutGetCommand(Logger, null!));
    }

    [Fact]
    public void Metadata_HasCorrectProperties()
    {
        var metadata = Command.Metadata;

        Assert.False(metadata.Destructive);
        Assert.True(metadata.Idempotent);
        Assert.False(metadata.LocalRequired);
        Assert.False(metadata.OpenWorld);
        Assert.True(metadata.ReadOnly);
        Assert.False(metadata.Secret);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsShortcutWrapper()
    {
        Service.GetShortcutAsync("ws1", "item1", "Files/landing", "shortcut1", Arg.Any<CancellationToken>())
            .Returns(new OneLakeShortcut { Path = "Files/landing", Name = "shortcut1" });

        var response = await ExecuteCommandAsync(
            "--workspace-id", "ws1",
            "--item-id", "item1",
            "--shortcut-path", "Files/landing",
            "--shortcut-name", "shortcut1");

        var result = ValidateAndDeserializeResponse(response, OneLakeJsonContext.Default.ShortcutGetCommandResult);

        Assert.Equal("shortcut1", result.Shortcut.Name);
        Assert.Equal("Files/landing", result.Shortcut.Path);
    }
}
