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

public class TableGetCommandTests : CommandUnitTestsBase<TableGetCommand, IOneLakeService>
{
    [Fact]
    public void Constructor_InitializesMetadata()
    {
        Assert.Equal("get_table", Command.Name);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.Destructive);
    }

    [Fact]
    public void GetCommand_ReturnsConfiguredCommand()
    {
        Assert.Equal("get_table", CommandDefinition.Name);
        Assert.NotEmpty(CommandDefinition.Options);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsTableDefinition()
    {
        var workspaceId = "47242da5-ff3b-46fb-a94f-977909b773d5";
        var itemId = "0e67ed13-2bb6-49be-9c87-a1105a4ea342";
        const string namespaceName = "sales";
        const string tableName = "transactions";

        using var sampleDocument = JsonDocument.Parse("{\"columns\":[{\"name\":\"Id\"}]}");
        var definition = sampleDocument.RootElement.Clone();

        Service.GetTableAsync(workspaceId, itemId, namespaceName, tableName, Arg.Any<CancellationToken>())
            .Returns(new TableGetResult(workspaceId, itemId, namespaceName, tableName, definition, "{\"columns\":[{\"name\":\"Id\"}]}"));

        var response = await ExecuteCommandAsync(
            "--workspace-id", workspaceId,
            "--item-id", itemId,
            "--namespace", namespaceName,
            "--table", tableName);

        await Service.Received(1).GetTableAsync(workspaceId, itemId, namespaceName, tableName, Arg.Any<CancellationToken>());

        var result = ValidateAndDeserializeResponse(response, OneLakeJsonContext.Default.TableGetCommandResult);

        Assert.Equal(workspaceId, result.Workspace);
        Assert.Equal(itemId, result.Item);
        Assert.Equal(namespaceName, result.Namespace);
        Assert.Equal(tableName, result.Table);
        Assert.Equal("Id", result.Definition.GetProperty("columns")[0].GetProperty("name").GetString());
        Assert.Equal("{\"columns\":[{\"name\":\"Id\"}]}", result.RawResponse);
    }

    [Fact]
    public async Task ExecuteAsync_AcceptsWorkspaceAndItemByName()
    {
        var workspace = "Analytics Workspace";
        var item = "SalesLakehouse.lakehouse";
        const string namespaceName = "sales";
        const string tableName = "transactions";

        using var sampleDocument = JsonDocument.Parse("{}");
        Service.GetTableAsync(workspace, item, namespaceName, tableName, Arg.Any<CancellationToken>())
            .Returns(new TableGetResult(workspace, item, namespaceName, tableName, sampleDocument.RootElement.Clone(), "{}"));

        _ = await ExecuteCommandAsync(
            "--workspace", workspace,
            "--item", item,
            "--namespace", namespaceName,
            "--table", tableName);

        await Service.Received(1).GetTableAsync(workspace, item, namespaceName, tableName, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("--workspace-id")]
    [InlineData("--item-id")]
    [InlineData("--namespace")]
    [InlineData("--table")]
    public async Task ExecuteAsync_MissingOption_ReturnsBadRequest(string missingOption)
    {
        var response = await ExecuteCommandAsync(ArgBuilder.BuildArgs(missingOption,
            ("--workspace-id", "workspace"),
            ("--item-id", "item"),
            ("--namespace", "sales"),
            ("--table", "transactions")));

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceive().GetTableAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Constructor_ThrowsForNullDependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new TableGetCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new TableGetCommand(Logger, null!));
    }
}
