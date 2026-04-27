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

public class TableListCommandTests : CommandUnitTestsBase<TableListCommand, IOneLakeService>
{
    [Fact]
    public void Constructor_InitializesMetadata()
    {
        Assert.Equal("list_tables", Command.Name);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.Destructive);
    }

    [Fact]
    public void GetCommand_ReturnsConfiguredCommand()
    {
        Assert.Equal("list_tables", CommandDefinition.Name);
        Assert.NotEmpty(CommandDefinition.Options);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsTables()
    {
        var workspaceId = "47242da5-ff3b-46fb-a94f-977909b773d5";
        var itemId = "0e67ed13-2bb6-49be-9c87-a1105a4ea342";
        const string namespaceName = "sales";

        using var sampleDocument = JsonDocument.Parse("[{\"name\":\"transactions\"}]");
        var tables = sampleDocument.RootElement.Clone();

        Service.ListTablesAsync(workspaceId, itemId, namespaceName, Arg.Any<CancellationToken>())
            .Returns(new TableListResult(workspaceId, itemId, namespaceName, tables, "[{\"name\":\"transactions\"}]"));

        var response = await ExecuteCommandAsync(
            "--workspace-id", workspaceId,
            "--item-id", itemId,
            "--namespace", namespaceName);

        await Service.Received(1).ListTablesAsync(workspaceId, itemId, namespaceName, Arg.Any<CancellationToken>());

        var result = ValidateAndDeserializeResponse(response, OneLakeJsonContext.Default.TableListCommandResult);

        Assert.Equal(workspaceId, result.Workspace);
        Assert.Equal(itemId, result.Item);
        Assert.Equal(namespaceName, result.Namespace);
        Assert.Single(result.Tables.EnumerateArray());
        Assert.Equal("transactions", result.Tables[0].GetProperty("name").GetString());
        Assert.Equal("[{\"name\":\"transactions\"}]", result.RawResponse);
    }

    [Theory]
    [InlineData("--workspace-id")]
    [InlineData("--item-id")]
    [InlineData("--namespace")]
    public async Task ExecuteAsync_MissingOption_ReturnsBadRequest(string missingOption)
    {
        var response = await ExecuteCommandAsync(ArgBuilder.BuildArgs(missingOption,
            ("--workspace-id", "workspace"),
            ("--item-id", "item"),
            ("--namespace", "sales")
        ));

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceive().ListTablesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Constructor_ThrowsForNullDependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new TableListCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new TableListCommand(Logger, null!));
    }
}
