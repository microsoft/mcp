// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Commands.Settings;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using Xunit;

namespace Fabric.Mcp.Tools.OneLake.Tests.Commands.Settings;

public class SettingsGetCommandTests : CommandUnitTestsBase<SettingsGetCommand, IOneLakeService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get-settings", Command.Name);
        Assert.Equal("Get OneLake Settings", Command.Title);
        Assert.Contains("Get the OneLake settings for a workspace", Command.Description);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.Idempotent);
    }

    [Fact]
    public void GetCommand_ReturnsValidCommand()
    {
        Assert.Equal("get-settings", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Options);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new SettingsGetCommand(null!, Service));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOneLakeServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new SettingsGetCommand(Logger, null!));
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
    public async Task ExecuteAsync_ReturnsSettingsWrapper()
    {
        const string workspaceId = "32c6efb2-ca3a-4598-83b0-8abe799830cd";
        Service.GetSettingsAsync(workspaceId, Arg.Any<CancellationToken>())
            .Returns(new OneLakeSettings
            {
                Diagnostics = new OneLakeDiagnosticSettings { Status = "Enabled" }
            });

        var response = await ExecuteCommandAsync("--workspace-id", workspaceId);

        var result = ValidateAndDeserializeResponse(response, OneLakeJsonContext.Default.SettingsGetCommandResult);

        Assert.Equal("Enabled", result.Settings.Diagnostics?.Status);
    }
}
