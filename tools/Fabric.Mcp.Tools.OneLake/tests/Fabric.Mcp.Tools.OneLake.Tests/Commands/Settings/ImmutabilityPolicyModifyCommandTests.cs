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

public class ImmutabilityPolicyModifyCommandTests : CommandUnitTestsBase<ImmutabilityPolicyModifyCommand, IOneLakeService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("modify_immutability_policy", Command.Name);
        Assert.Equal("Modify OneLake Immutability Policy", Command.Title);
        Assert.Contains("Modify the workspace-level OneLake immutability policy", Command.Description);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.Idempotent);
    }

    [Fact]
    public void GetCommand_ReturnsValidCommand()
    {
        Assert.Equal("modify_immutability_policy", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Options);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ImmutabilityPolicyModifyCommand(null!, Service));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOneLakeServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ImmutabilityPolicyModifyCommand(Logger, null!));
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
    [InlineData("--workspace-id 85173301-af01-49c9-b667-03edc44517da --scope DiagnosticLogs --retention-days 7", true)]
    [InlineData("--scope DiagnosticLogs --retention-days 7", false)] // missing workspace
    [InlineData("--workspace-id 85173301-af01-49c9-b667-03edc44517da --scope DiagnosticLogs --retention-days 0", false)] // retention < 1
    [InlineData("--workspace-id 85173301-af01-49c9-b667-03edc44517da --scope Invalid --retention-days 7", false)] // bad scope
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.ModifyImmutabilityPolicyAsync(Arg.Any<string>(), Arg.Any<ImmutabilityPolicy>(), Arg.Any<CancellationToken>())
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
    public async Task ExecuteAsync_SuccessfulModification_ReturnsSuccessMessage()
    {
        Service.ModifyImmutabilityPolicyAsync(Arg.Any<string>(), Arg.Any<ImmutabilityPolicy>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var response = await ExecuteCommandAsync(
            "--workspace-id", "85173301-af01-49c9-b667-03edc44517da",
            "--scope", "DiagnosticLogs",
            "--retention-days", "7");

        var result = ValidateAndDeserializeResponse(response, OneLakeJsonContext.Default.ImmutabilityPolicyModifyCommandResult);
        Assert.Contains("successfully", result.Message, StringComparison.OrdinalIgnoreCase);
        await Service.Received(1).ModifyImmutabilityPolicyAsync("85173301-af01-49c9-b667-03edc44517da",
            Arg.Is<ImmutabilityPolicy>(p => p.Scope == "DiagnosticLogs" && p.RetentionDays == 7),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.ModifyImmutabilityPolicyAsync(Arg.Any<string>(), Arg.Any<ImmutabilityPolicy>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Forbidden"));

        var response = await ExecuteCommandAsync(
            "--workspace-id", "85173301-af01-49c9-b667-03edc44517da",
            "--scope", "DiagnosticLogs",
            "--retention-days", "7");

        Assert.NotNull(response);
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }
}

