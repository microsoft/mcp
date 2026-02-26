// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Fabric.Mcp.Tools.OneLake.Commands.Table;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;

namespace Fabric.Mcp.Tools.OneLake.Tests.Commands.Table;

public class TableConfigGetCommandTests
{
    [Fact]
    public void Constructor_InitializesMetadata()
    {
        var service = Substitute.For<IOneLakeService>();
        var command = new TableConfigGetCommand(NullLogger<TableConfigGetCommand>.Instance, service);

        Assert.Equal("get_table_config", command.Name);
        Assert.True(command.Metadata.ReadOnly);
        Assert.True(command.Metadata.Idempotent);
        Assert.False(command.Metadata.Destructive);
    }

    [Fact]
    public void GetCommand_ReturnsConfiguredCommand()
    {
        var service = Substitute.For<IOneLakeService>();
        var command = new TableConfigGetCommand(NullLogger<TableConfigGetCommand>.Instance, service);

        var systemCommand = command.GetCommand();

        Assert.NotNull(systemCommand);
        Assert.Equal("get_table_config", systemCommand.Name);
        Assert.NotEmpty(systemCommand.Options);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsConfiguration()
    {
        var service = Substitute.For<IOneLakeService>();
        var command = new TableConfigGetCommand(NullLogger<TableConfigGetCommand>.Instance, service);
        var workspaceId = "47242da5-ff3b-46fb-a94f-977909b773d5";
        var itemId = "0e67ed13-2bb6-49be-9c87-a110";
        using var sampleDocument = JsonDocument.Parse("{\"setting\":\"value\"}");
        var configuration = sampleDocument.RootElement.Clone();

        service.GetTableConfigurationAsync(workspaceId, itemId, Arg.Any<CancellationToken>())
            .Returns(new TableConfigurationResult(workspaceId, itemId, configuration, "{\"setting\":\"value\"}"));

        var parseResult = command.GetCommand().Parse($"--workspace-id {workspaceId} --item-id {itemId}");
        var context = CreateContext();

        var response = await command.ExecuteAsync(context, parseResult, CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await service.Received(1).GetTableConfigurationAsync(workspaceId, itemId, Arg.Any<CancellationToken>());

        var payload = Serialize(context.Response.Results);
        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;
        Assert.Equal(workspaceId, root.GetProperty("workspace").GetString());
        Assert.Equal(itemId, root.GetProperty("item").GetString());
        Assert.Equal("value", root.GetProperty("configuration").GetProperty("setting").GetString());
        Assert.Equal("{\"setting\":\"value\"}", root.GetProperty("rawResponse").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_MissingWorkspace_ReturnsBadRequest()
    {
        var service = Substitute.For<IOneLakeService>();
        var command = new TableConfigGetCommand(NullLogger<TableConfigGetCommand>.Instance, service);

        var parseResult = command.GetCommand().Parse("--item-id lakehouse");
        var context = CreateContext();

        var response = await command.ExecuteAsync(context, parseResult, CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await service.DidNotReceive().GetTableConfigurationAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_MissingItem_ReturnsBadRequest()
    {
        var service = Substitute.For<IOneLakeService>();
        var command = new TableConfigGetCommand(NullLogger<TableConfigGetCommand>.Instance, service);

        var parseResult = command.GetCommand().Parse("--workspace-id workspace");
        var context = CreateContext();

        var response = await command.ExecuteAsync(context, parseResult, CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await service.DidNotReceive().GetTableConfigurationAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Constructor_ThrowsForNullDependencies()
    {
        var service = Substitute.For<IOneLakeService>();

        Assert.Throws<ArgumentNullException>(() => new TableConfigGetCommand(null!, service));
        Assert.Throws<ArgumentNullException>(() => new TableConfigGetCommand(NullLogger<TableConfigGetCommand>.Instance, null!));
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
