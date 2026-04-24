// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Commands.Item;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Mcp.Tests.Client;

namespace Fabric.Mcp.Tools.OneLake.Tests.Commands;

public class OneLakeItemListCommandTests : CommandUnitTestsBase<OneLakeItemListCommand, IOneLakeService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("list_items", Command.Name);
        Assert.Equal("List OneLake Items", Command.Title);
        Assert.Contains("Lists OneLake items in a Fabric workspace", Command.Description);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.Idempotent);
    }

    [Fact]
    public void GetCommand_ReturnsValidCommand()
    {
        Assert.Equal("list_items", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Options);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new OneLakeItemListCommand(null!, Service));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOneLakeServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new OneLakeItemListCommand(Logger, null!));
    }
}
