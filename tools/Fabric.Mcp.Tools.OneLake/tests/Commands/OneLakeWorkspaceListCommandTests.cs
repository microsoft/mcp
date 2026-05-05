// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Commands.Workspace;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Mcp.Tests.Client;

namespace Fabric.Mcp.Tools.OneLake.Tests.Commands;

public class OneLakeWorkspaceListCommandTests : CommandUnitTestsBase<OneLakeWorkspaceListCommand, IOneLakeService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("list_workspaces", Command.Name);
        Assert.Equal("List OneLake Workspaces", Command.Title);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(string.IsNullOrEmpty(Command.Description));
        Assert.Contains("OneLake", Command.Description);
        Assert.True(Command.Description.Length <= 1024, "Description should not exceed 1024 characters");
    }

    [Fact]
    public void GetCommand_ReturnsValidCommand()
    {
        Assert.Equal("list_workspaces", CommandDefinition.Name);
        Assert.False(string.IsNullOrEmpty(CommandDefinition.Description));
        Assert.NotEmpty(CommandDefinition.Options);
    }
}
