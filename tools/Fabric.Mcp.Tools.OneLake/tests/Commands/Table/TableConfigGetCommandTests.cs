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

public class TableConfigGetCommandTests : CommandUnitTestsBase<TableConfigGetCommand, IOneLakeService>
{
    [Fact]
    public void Constructor_InitializesMetadata()
    {
        Assert.Equal("get_table_config", Command.Name);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.Destructive);
    }

    [Fact]
    public void GetCommand_ReturnsConfiguredCommand()
    {
        Assert.Equal("get_table_config", CommandDefinition.Name);
        Assert.NotEmpty(CommandDefinition.Options);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsConfiguration()
    {
        var workspaceId = "47242da5-ff3b-46fb-a94f-977909b773d5";
        var itemId = "0e67ed13-2bb6-49be-9c87-a110";
        using var sampleDocument = JsonDocument.Parse("{\"setting\":\"value\"}");
        var configuration = sampleDocument.RootElement.Clone();

        Service.GetTableConfigurationAsync(workspaceId, itemId, Arg.Any<CancellationToken>())
            .Returns(new TableConfigurationResult(workspaceId, itemId, configuration, "{\"setting\":\"value\"}"));

        var response = await ExecuteCommandAsync("--workspace-id", workspaceId, "--item-id", itemId);

        await Service.Received(1).GetTableConfigurationAsync(workspaceId, itemId, Arg.Any<CancellationToken>());

        var result = ValidateAndDeserializeResponse(response, OneLakeJsonContext.Default.TableConfigGetCommandResult);

        Assert.Equal(workspaceId, result.Workspace);
        Assert.Equal(itemId, result.Item);
        Assert.Equal("value", result.Configuration.GetProperty("setting").GetString());
        Assert.Equal("{\"setting\":\"value\"}", result.RawResponse);
    }

    [Theory]
    [InlineData("--workspace-id")]
    [InlineData("--item-id")]
    public async Task ExecuteAsync_MissingOption_ReturnsBadRequest(string missingOption)
    {
        var response = await ExecuteCommandAsync(ArgBuilder.BuildArgs(missingOption,
            ("--workspace-id", "workspace"),
            ("--item-id", "lakehouse")
        ));

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceive().GetTableConfigurationAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Constructor_ThrowsForNullDependencies()
    {

        Assert.Throws<ArgumentNullException>(() => new TableConfigGetCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new TableConfigGetCommand(Logger, null!));
    }
}
