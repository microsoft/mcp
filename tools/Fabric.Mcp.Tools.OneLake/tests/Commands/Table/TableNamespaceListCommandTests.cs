// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Fabric.Mcp.Tools.OneLake.Commands.Table;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Mcp.Core.TestUtilities;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;

namespace Fabric.Mcp.Tools.OneLake.Tests.Commands.Table;

public class TableNamespaceListCommandTests : CommandUnitTestsBase<TableNamespaceListCommand, IOneLakeService>
{
    [Fact]
    public void Constructor_InitializesMetadata()
    {
        Assert.Equal("list_table_namespaces", Command.Name);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.Destructive);
    }

    [Fact]
    public void GetCommand_ReturnsConfiguredCommand()
    {
        Assert.Equal("list_table_namespaces", CommandDefinition.Name);
        Assert.NotEmpty(CommandDefinition.Options);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNamespaces()
    {
        var workspaceId = "47242da5-ff3b-46fb-a94f-977909b773d5";
        var itemId = "0e67ed13-2bb6-49be-9c87-a1105a4ea342";

        using var sampleDocument = JsonDocument.Parse("[\"dbo\",\"sales\"]");
        var namespaces = sampleDocument.RootElement.Clone();

        Service.ListTableNamespacesAsync(workspaceId, itemId, Arg.Any<CancellationToken>())
            .Returns(new TableNamespaceListResult(workspaceId, itemId, namespaces, "[\"dbo\",\"sales\"]"));

        var response = await ExecuteCommandAsync("--workspace-id", workspaceId, "--item-id", itemId);

        await Service.Received(1).ListTableNamespacesAsync(workspaceId, itemId, Arg.Any<CancellationToken>());

        var result = ValidateAndDeserializeResponse(response, OneLakeJsonContext.Default.TableNamespaceListCommandResult);

        Assert.Equal(workspaceId, result.Workspace);
        Assert.Equal(itemId, result.Item);
        Assert.Equal(2, result.Namespaces.GetArrayLength());
        Assert.Equal("dbo", result.Namespaces[0].GetString());
        Assert.Equal("sales", result.Namespaces[1].GetString());
        Assert.Equal("[\"dbo\",\"sales\"]", result.RawResponse);
    }

    [Theory]
    [InlineData("--workspace-id")]
    [InlineData("--item-id")]
    public async Task ExecuteAsync_MissingOption_ReturnsBadRequest(string missingOption)
    {
        var response = await ExecuteCommandAsync(ArgBuilder.BuildArgs(missingOption,
            ("--workspace-id", "workspace"),
            ("--item-id", "item")
        ));

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceive().ListTableNamespacesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Constructor_ThrowsForNullDependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new TableNamespaceListCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new TableNamespaceListCommand(Logger, null!));
    }
}
