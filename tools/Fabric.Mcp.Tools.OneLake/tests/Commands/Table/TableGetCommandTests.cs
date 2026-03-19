// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using System.Text.Json;
using Fabric.Mcp.Tools.OneLake.Commands.Table;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;

namespace Fabric.Mcp.Tools.OneLake.Tests.Commands.Table;

public class TableGetCommandTests
{
    [Fact]
    public void Constructor_InitializesMetadata()
    {
        var service = Substitute.For<IOneLakeService>();
        var command = new TableGetCommand(NullLogger<TableGetCommand>.Instance, service);

        Assert.Equal("get_table", command.Name);
        Assert.True(command.Metadata.ReadOnly);
        Assert.True(command.Metadata.Idempotent);
        Assert.False(command.Metadata.Destructive);
    }

    [Fact]
    public void GetCommand_ReturnsConfiguredCommand()
    {
        var service = Substitute.For<IOneLakeService>();
        var command = new TableGetCommand(NullLogger<TableGetCommand>.Instance, service);

        var systemCommand = command.GetCommand();

        Assert.NotNull(systemCommand);
        Assert.Equal("get_table", systemCommand.Name);
        Assert.NotEmpty(systemCommand.Options);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsTableDefinition()
    {
        var service = Substitute.For<IOneLakeService>();
        var command = new TableGetCommand(NullLogger<TableGetCommand>.Instance, service);
        var workspaceId = "47242da5-ff3b-46fb-a94f-977909b773d5";
        var itemId = "0e67ed13-2bb6-49be-9c87-a1105a4ea342";
        const string namespaceName = "sales";
        const string tableName = "transactions";

        using var sampleDocument = JsonDocument.Parse("{\"columns\":[{\"name\":\"Id\"}]}");
        var definition = sampleDocument.RootElement.Clone();

        service.GetTableAsync(workspaceId, itemId, namespaceName, tableName, Arg.Any<CancellationToken>())
            .Returns(new TableGetResult(workspaceId, itemId, namespaceName, tableName, definition, "{\"columns\":[{\"name\":\"Id\"}]}"));

        var parseResult = command.GetCommand().Parse($"--workspace-id {workspaceId} --item-id {itemId} --namespace {namespaceName} --table {tableName}");
        var context = CreateContext();

        var response = await command.ExecuteAsync(context, parseResult, CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await service.Received(1).GetTableAsync(workspaceId, itemId, namespaceName, tableName, Arg.Any<CancellationToken>());

        var payload = Serialize(context.Response.Results);
        using var resultDocument = JsonDocument.Parse(payload);
        var root = resultDocument.RootElement;
        Assert.Equal(workspaceId, root.GetProperty("workspace").GetString());
        Assert.Equal(itemId, root.GetProperty("item").GetString());
        Assert.Equal(namespaceName, root.GetProperty("namespace").GetString());
        Assert.Equal(tableName, root.GetProperty("table").GetString());
        Assert.Equal("Id", root.GetProperty("definition").GetProperty("columns")[0].GetProperty("name").GetString());
        Assert.Equal("{\"columns\":[{\"name\":\"Id\"}]}", root.GetProperty("rawResponse").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_AllowsSchemaAlias()
    {
        var service = Substitute.For<IOneLakeService>();
        var command = new TableGetCommand(NullLogger<TableGetCommand>.Instance, service);
        var workspace = "Analytics Workspace";
        var item = "SalesLakehouse.lakehouse";
        const string namespaceName = "sales";
        const string tableName = "transactions";

        using var sampleDocument = JsonDocument.Parse("{}");
        service.GetTableAsync(workspace, item, namespaceName, tableName, Arg.Any<CancellationToken>())
            .Returns(new TableGetResult(workspace, item, namespaceName, tableName, sampleDocument.RootElement.Clone(), "{}"));

        var parseResult = command.GetCommand().Parse($"--workspace \"{workspace}\" --item \"{item}\" --schema {namespaceName} --table {tableName}");
        var context = CreateContext();

        _ = await command.ExecuteAsync(context, parseResult, CancellationToken.None);

        await service.Received(1).GetTableAsync(workspace, item, namespaceName, tableName, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_MissingWorkspace_ReturnsBadRequest()
    {
        var service = Substitute.For<IOneLakeService>();
        var command = new TableGetCommand(NullLogger<TableGetCommand>.Instance, service);

        var parseResult = command.GetCommand().Parse("--item-id item --namespace sales --table transactions");
        var context = CreateContext();

        var response = await command.ExecuteAsync(context, parseResult, CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await service.DidNotReceive().GetTableAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_MissingItem_ReturnsBadRequest()
    {
        var service = Substitute.For<IOneLakeService>();
        var command = new TableGetCommand(NullLogger<TableGetCommand>.Instance, service);

        var parseResult = command.GetCommand().Parse("--workspace-id workspace --namespace sales --table transactions");
        var context = CreateContext();

        var response = await command.ExecuteAsync(context, parseResult, CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await service.DidNotReceive().GetTableAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_MissingNamespace_ReturnsBadRequest()
    {
        var service = Substitute.For<IOneLakeService>();
        var command = new TableGetCommand(NullLogger<TableGetCommand>.Instance, service);

        var parseResult = command.GetCommand().Parse("--workspace-id workspace --item-id item --table transactions");
        var context = CreateContext();

        var response = await command.ExecuteAsync(context, parseResult, CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await service.DidNotReceive().GetTableAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_MissingTable_ReturnsBadRequest()
    {
        var service = Substitute.For<IOneLakeService>();
        var command = new TableGetCommand(NullLogger<TableGetCommand>.Instance, service);

        var parseResult = command.GetCommand().Parse("--workspace-id workspace --item-id item --namespace sales");
        var context = CreateContext();

        var response = await command.ExecuteAsync(context, parseResult, CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await service.DidNotReceive().GetTableAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Constructor_ThrowsForNullDependencies()
    {
        var service = Substitute.For<IOneLakeService>();

        Assert.Throws<ArgumentNullException>(() => new TableGetCommand(null!, service));
        Assert.Throws<ArgumentNullException>(() => new TableGetCommand(NullLogger<TableGetCommand>.Instance, null!));
    }

    private static CommandContext CreateContext(string transport = "stdio")
    {
        var serviceProvider = Substitute.For<IServiceProvider>();
        var options = Microsoft.Extensions.Options.Options.Create(new ServiceStartOptions { Transport = transport });
        serviceProvider.GetService(typeof(IOptions<ServiceStartOptions>)).Returns(options);
        return new CommandContext(serviceProvider);
    }

    private static string Serialize(ResponseResult? result)
    {
        if (result is null)
        {
            return string.Empty;
        }

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            result.Write(writer);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }
}
