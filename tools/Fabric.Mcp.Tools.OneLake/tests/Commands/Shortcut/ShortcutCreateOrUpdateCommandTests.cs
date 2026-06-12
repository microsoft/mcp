// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.OneLake.Commands.Shortcut;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Fabric.Mcp.Tools.OneLake.Tests.Commands.Shortcut;

public class ShortcutCreateOrUpdateCommandTests : CommandUnitTestsBase<ShortcutCreateOrUpdateCommand, IOneLakeService>
{
    private const string ValidJson = "{\"createShortcutRequests\":[{\"path\":\"Files/folder\",\"name\":\"sc1\",\"target\":{\"oneLake\":{\"workspaceId\":\"ws2\",\"itemId\":\"item2\",\"path\":\"Tables\"}}}]}";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("create_or_update_shortcuts", Command.Name);
        Assert.Equal("Create or Update OneLake Shortcuts", Command.Title);
        Assert.Contains("bulk create", Command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.Idempotent);
    }

    [Fact]
    public void GetCommand_ReturnsValidCommand()
    {
        Assert.Equal("create_or_update_shortcuts", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Options);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ShortcutCreateOrUpdateCommand(null!, Service));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOneLakeServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ShortcutCreateOrUpdateCommand(Logger, null!));
    }

    [Fact]
    public void Metadata_HasCorrectProperties()
    {
        var metadata = Command.Metadata;

        Assert.False(metadata.Destructive);
        Assert.False(metadata.Idempotent);
        Assert.False(metadata.LocalRequired);
        Assert.False(metadata.OpenWorld);
        Assert.False(metadata.ReadOnly);
        Assert.False(metadata.Secret);
    }

    [Theory]
    [InlineData("--workspace-id ws1 --item-id item1", true)]
    [InlineData("--item-id item1", false)]   // missing workspace
    [InlineData("--workspace-id ws1", false)] // missing item
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.CreateOrUpdateShortcutsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(new BulkCreateShortcutResponse());
        }

        var fullArgs = string.IsNullOrWhiteSpace(args)
            ? $"--shortcuts {ValidJson}"
            : $"{args} --shortcuts {ValidJson}";

        var response = await ExecuteCommandAsync(fullArgs);

        Assert.NotNull(response);
        if (shouldSucceed)
            Assert.Equal(HttpStatusCode.OK, response.Status);
        else
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_SingleShortcut_CallsBulkCreateOnce()
    {
        var expected = new BulkCreateShortcutResponse
        {
            Value = [new CreateShortcutResponse { Status = "Succeeded" }]
        };

        Service.CreateOrUpdateShortcutsAsync("ws1", "item1", Arg.Any<string>(), null, Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync(
            "--workspace-id", "ws1",
            "--item-id", "item1",
            "--shortcuts", ValidJson);

        var result = ValidateAndDeserializeResponse(response, OneLakeJsonContext.Default.BulkCreateShortcutResponse);
        Assert.Single(result.Value!);
        Assert.Equal("Succeeded", result.Value![0].Status);
        await Service.Received(1).CreateOrUpdateShortcutsAsync("ws1", "item1", Arg.Any<string>(), null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithConflictPolicy_PassesPolicyToService()
    {
        Service.CreateOrUpdateShortcutsAsync("ws1", "item1", Arg.Any<string>(), "CreateOrOverwrite", Arg.Any<CancellationToken>())
            .Returns(new BulkCreateShortcutResponse { Value = [] });

        var response = await ExecuteCommandAsync(
            "--workspace-id", "ws1",
            "--item-id", "item1",
            "--shortcuts", ValidJson,
            "--shortcut-conflict-policy", "CreateOrOverwrite");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateOrUpdateShortcutsAsync("ws1", "item1", Arg.Any<string>(), "CreateOrOverwrite", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.CreateOrUpdateShortcutsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Conflict"));

        var response = await ExecuteCommandAsync(
            "--workspace-id", "ws1",
            "--item-id", "item1",
            "--shortcuts", ValidJson);

        Assert.NotNull(response);
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }
}

