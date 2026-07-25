// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Commands.Security;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;

namespace Fabric.Mcp.Tools.OneLake.Tests.Commands.Security;

public class DataAccessRoleListCommandTests : CommandUnitTestsBase<DataAccessRoleListCommand, IOneLakeService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("list_data_access_roles", Command.Name);
        Assert.Equal("List OneLake Data Access Roles", Command.Title);
        Assert.Contains("List all data access roles", Command.Description);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.Idempotent);
    }

    [Fact]
    public void GetCommand_ReturnsValidCommand()
    {
        Assert.Equal("list_data_access_roles", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Options);
    }

    [Fact]
    public void GetCommand_RegistersContinuationTokenOption()
    {
        var option = CommandDefinition.Options.FirstOrDefault(o => o.Name == "--continuation-token");
        Assert.NotNull(option);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new DataAccessRoleListCommand(null!, Service));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOneLakeServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new DataAccessRoleListCommand(Logger, null!));
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
    public async Task ExecuteAsync_ReturnsRolesWrapper()
    {
        const string workspaceId = "32c6efb2-ca3a-4598-83b0-8abe799830cd";
        Service.ListDataAccessRolesAsync(workspaceId, "item1", null, Arg.Any<CancellationToken>())
            .Returns(new DataAccessRoleListResponse
            {
                Value = [new DataAccessRole { Name = "TestRole" }],
                ContinuationToken = "next-token",
                ContinuationUri = "https://example.test/roles"
            });

        var response = await ExecuteCommandAsync(
            "--workspace-id", workspaceId,
            "--item-id", "item1");

        var result = ValidateAndDeserializeResponse(response, OneLakeJsonContext.Default.DataAccessRoleListCommandResult);

        Assert.Collection(result.Roles, role => Assert.Equal("TestRole", role.Name));
        Assert.Equal("next-token", result.ContinuationToken);
        Assert.Equal("https://example.test/roles", result.ContinuationUri);
    }
}
