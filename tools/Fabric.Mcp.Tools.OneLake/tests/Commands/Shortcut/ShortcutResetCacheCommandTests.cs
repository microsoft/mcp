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

public class ShortcutResetCacheCommandTests : CommandUnitTestsBase<ShortcutResetCacheCommand, IOneLakeService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("reset_shortcut_cache", Command.Name);
        Assert.Equal("Reset OneLake Shortcut Cache", Command.Title);
        Assert.Contains("Drop cached shortcut reads", Command.Description);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.Idempotent);
    }

    [Fact]
    public void GetCommand_ReturnsValidCommand()
    {
        Assert.Equal("reset_shortcut_cache", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Options);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ShortcutResetCacheCommand(null!, Service));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOneLakeServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ShortcutResetCacheCommand(Logger, null!));
    }

    [Fact]
    public void Metadata_HasCorrectProperties()
    {
        var metadata = Command.Metadata;

        Assert.False(metadata.Destructive);
        Assert.True(metadata.Idempotent);
        Assert.False(metadata.LocalRequired);
        Assert.False(metadata.OpenWorld);
        Assert.False(metadata.ReadOnly);
        Assert.False(metadata.Secret);
    }

    [Theory]
    [InlineData("--workspace-id ws1", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.ResetShortcutCacheAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);
        }

        var response = await ExecuteCommandAsync(args);

        Assert.NotNull(response);
        if (shouldSucceed)
            Assert.Equal(HttpStatusCode.OK, response.Status);
        else
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_NoItemRequired_SucceedsWithWorkspaceOnly()
    {
        Service.ResetShortcutCacheAsync("ws1", Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var response = await ExecuteCommandAsync("--workspace-id", "ws1");

        var result = ValidateAndDeserializeResponse(response, OneLakeJsonContext.Default.ShortcutResetCacheCommandResult);
        Assert.Contains("successfully", result.Message, StringComparison.OrdinalIgnoreCase);
        await Service.Received(1).ResetShortcutCacheAsync("ws1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.ResetShortcutCacheAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        var response = await ExecuteCommandAsync("--workspace-id", "ws1");

        Assert.NotNull(response);
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public void BindOptions_RequiresWorkspaceId()
    {
        var parseResult = CommandDefinition.Parse(string.Empty);
        var isValid = Command.Validate(parseResult.CommandResult);
        Assert.False(isValid.IsValid);
    }
}

