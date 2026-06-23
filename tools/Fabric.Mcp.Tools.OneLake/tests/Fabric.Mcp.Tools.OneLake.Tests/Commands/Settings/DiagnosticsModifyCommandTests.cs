// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.OneLake.Commands.Settings;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Fabric.Mcp.Tools.OneLake.Tests.Commands.Settings;

public class DiagnosticsModifyCommandTests : CommandUnitTestsBase<DiagnosticsModifyCommand, IOneLakeService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("modify_diagnostics", Command.Name);
        Assert.Equal("Modify OneLake Diagnostics", Command.Title);
        Assert.Contains("Enable or disable workspace-level OneLake diagnostic logging", Command.Description);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.Idempotent);
    }

    [Fact]
    public void GetCommand_ReturnsValidCommand()
    {
        Assert.Equal("modify_diagnostics", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Options);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new DiagnosticsModifyCommand(null!, Service));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOneLakeServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new DiagnosticsModifyCommand(Logger, null!));
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
    [InlineData("--workspace-id 85173301-af01-49c9-b667-03edc44517da --status Disabled", true)]
    [InlineData("--workspace-id 85173301-af01-49c9-b667-03edc44517da --status Enabled --destination-lakehouse-workspace-id 85173301-af01-49c9-b667-03edc44517da --destination-lakehouse-item-id eceb53c6-6227-41f1-a649-62ebe7ee9eb1", true)]
    [InlineData("--status Disabled", false)] // missing workspace
    [InlineData("--workspace-id 85173301-af01-49c9-b667-03edc44517da --status Enabled", false)] // missing destination when enabled
    [InlineData("--workspace-id 85173301-af01-49c9-b667-03edc44517da --status Disabled --destination-lakehouse-workspace-id 85173301-af01-49c9-b667-03edc44517da --destination-lakehouse-item-id eceb53c6-6227-41f1-a649-62ebe7ee9eb1", false)] // destination provided when disabled
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.ModifyDiagnosticsAsync(Arg.Any<string>(), Arg.Any<OneLakeDiagnosticSettings>(), Arg.Any<CancellationToken>())
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
    public async Task ExecuteAsync_DisablesDiagnostics_ReturnsSuccessMessage()
    {
        Service.ModifyDiagnosticsAsync(Arg.Any<string>(), Arg.Any<OneLakeDiagnosticSettings>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var response = await ExecuteCommandAsync(
            "--workspace-id", "85173301-af01-49c9-b667-03edc44517da",
            "--status", "Disabled");

        var result = ValidateAndDeserializeResponse(response, OneLakeJsonContext.Default.DiagnosticsModifyCommandResult);
        Assert.Contains("successfully", result.Message, StringComparison.OrdinalIgnoreCase);
        await Service.Received(1).ModifyDiagnosticsAsync("85173301-af01-49c9-b667-03edc44517da", Arg.Is<OneLakeDiagnosticSettings>(s => s.Status == "Disabled" && s.Destination == null), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_EnablesDiagnostics_BuildsCorrectModel()
    {
        Service.ModifyDiagnosticsAsync(Arg.Any<string>(), Arg.Any<OneLakeDiagnosticSettings>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var response = await ExecuteCommandAsync(
            "--workspace-id", "85173301-af01-49c9-b667-03edc44517da",
            "--status", "Enabled",
            "--destination-lakehouse-workspace-id", "85173301-af01-49c9-b667-03edc44517da",
            "--destination-lakehouse-item-id", "eceb53c6-6227-41f1-a649-62ebe7ee9eb1");

        var result = ValidateAndDeserializeResponse(response, OneLakeJsonContext.Default.DiagnosticsModifyCommandResult);
        Assert.Contains("successfully", result.Message, StringComparison.OrdinalIgnoreCase);
        await Service.Received(1).ModifyDiagnosticsAsync("85173301-af01-49c9-b667-03edc44517da",
            Arg.Is<OneLakeDiagnosticSettings>(s =>
                s.Status == "Enabled" &&
                s.Destination != null &&
                s.Destination.Lakehouse != null &&
                s.Destination.Lakehouse.WorkspaceId == "85173301-af01-49c9-b667-03edc44517da" &&
                s.Destination.Lakehouse.ItemId == "eceb53c6-6227-41f1-a649-62ebe7ee9eb1"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.ModifyDiagnosticsAsync(Arg.Any<string>(), Arg.Any<OneLakeDiagnosticSettings>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Forbidden"));

        var response = await ExecuteCommandAsync(
            "--workspace-id", "85173301-af01-49c9-b667-03edc44517da",
            "--status", "Disabled");

        Assert.NotNull(response);
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }
}

