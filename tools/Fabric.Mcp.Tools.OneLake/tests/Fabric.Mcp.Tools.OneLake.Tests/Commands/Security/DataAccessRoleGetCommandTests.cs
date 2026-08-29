// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Commands.Security;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using Xunit;

namespace Fabric.Mcp.Tools.OneLake.Tests.Commands.Security;

public class DataAccessRoleGetCommandTests : CommandUnitTestsBase<DataAccessRoleGetCommand, IOneLakeService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get-data-access-role", Command.Name);
        Assert.Equal("Get OneLake Data Access Role", Command.Title);
        Assert.Contains("Get the full definition of a single data access role", Command.Description);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.Idempotent);
    }

    [Fact]
    public void GetCommand_ReturnsValidCommand()
    {
        Assert.Equal("get-data-access-role", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Options);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new DataAccessRoleGetCommand(null!, Service));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOneLakeServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new DataAccessRoleGetCommand(Logger, null!));
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
    public async Task ExecuteAsync_ReturnsRoleWrapper()
    {
        const string workspaceId = "32c6efb2-ca3a-4598-83b0-8abe799830cd";
        var expected = new DataAccessRole { Name = "TestRole" };
        Service.GetDataAccessRoleAsync(workspaceId, "item1", "TestRole", Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync(
            "--workspace-id", workspaceId,
            "--item-id", "item1",
            "--role-name", "TestRole");

        var result = ValidateAndDeserializeResponse(response, OneLakeJsonContext.Default.DataAccessRoleGetCommandResult);

        Assert.Equal("TestRole", result.Role.Name);
    }
}
