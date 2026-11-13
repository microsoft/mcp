// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Commands;

public class WorkspaceUpdateCommandTests() : CommandUnitTestsBase<WorkspaceUpdateCommand, IFabricCoreService>
{
    [Fact]
    public void Constructor_RegistersMutatingIdempotentCommandAndTypedResult()
    {
        Assert.Equal("update-workspace", Command.Name);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.False(Command.Metadata.Secret);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.Same(CoreJsonContext.Default.WorkspaceUpdateCommandResult, Command.ResultTypeInfo);
        Assert.Equal(["--description", "--display-name", "--workspace-id"], CommandDefinition.Options.Select(static option => option.Name).Order());
        Assert.Throws<ArgumentNullException>(() => new WorkspaceUpdateCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new WorkspaceUpdateCommand(Logger, null!));
    }

    [Theory]
    [InlineData("Finance", null)]
    [InlineData(null, "A new description")]
    [InlineData(null, "")]
    [InlineData("Finance", "")]
    [InlineData("Finance", "New description")]
    [InlineData(null, " \t ")]
    [InlineData(" \u8ca1\u52d9 ", "\u03b1\n\"quoted\" \\ text")]
    public async Task ExecuteAsync_PreservesSuppliedFields(string? displayName, string? description)
    {
        Service.UpdateWorkspaceAsync(Arg.Any<string>(), Arg.Any<UpdateWorkspaceRequest>(), Arg.Any<CancellationToken>())
            .Returns(WorkspaceUpdateTestData.CreateWorkspace());
        List<string> arguments = ["--workspace-id", WorkspaceUpdateTestData.WorkspaceId];
        if (displayName is not null)
        {
            arguments.AddRange(["--display-name", displayName]);
        }
        if (description is not null)
        {
            arguments.AddRange(["--description", description]);
        }

        var response = await ExecuteCommandAsync([.. arguments]);

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.WorkspaceUpdateCommandResult);
        Assert.Equal(Guid.Parse(WorkspaceUpdateTestData.WorkspaceId), result.Workspace.Id);
        Assert.Equal("", result.Workspace.Description);
        await Service.Received(1).UpdateWorkspaceAsync(
            WorkspaceUpdateTestData.WorkspaceId,
            Arg.Is<UpdateWorkspaceRequest>(request => request.DisplayName == displayName && request.Description == description),
            TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("not-a-uuid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("../other-workspace?secret=value")]
    [InlineData("https://example.invalid/workspace")]
    public async Task ExecuteAsync_RejectsInvalidIds(string workspaceId)
    {
        var response = await ExecuteCommandAsync("--workspace-id", workspaceId, "--description", "");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_RejectsMissingIdAndMissingUpdates()
    {
        var missingId = await ExecuteCommandAsync("--description", "");
        Assert.Equal(HttpStatusCode.BadRequest, missingId.Status);

        var missingUpdate = await ExecuteCommandAsync("--workspace-id", WorkspaceUpdateTestData.WorkspaceId);
        Assert.Equal(HttpStatusCode.BadRequest, missingUpdate.Status);
        Assert.Contains("at least one", missingUpdate.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" \t ")]
    [InlineData("Admin monitoring")]
    [InlineData("ADMIN MONITORING")]
    public async Task ExecuteAsync_RejectsBlankAndReservedNamesEvenWithDescription(string displayName)
    {
        var response = await ExecuteCommandAsync(
            "--workspace-id", WorkspaceUpdateTestData.WorkspaceId, "--display-name", displayName, "--description", "");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData(256, 4000, true)]
    [InlineData(257, 0, false)]
    [InlineData(1, 4001, false)]
    public async Task ExecuteAsync_EnforcesLengthBoundaries(int nameLength, int descriptionLength, bool valid)
    {
        Service.UpdateWorkspaceAsync(Arg.Any<string>(), Arg.Any<UpdateWorkspaceRequest>(), Arg.Any<CancellationToken>())
            .Returns(WorkspaceUpdateTestData.CreateWorkspace());

        var response = await ExecuteCommandAsync(
            "--workspace-id", WorkspaceUpdateTestData.WorkspaceId,
            "--display-name", new string('n', nameLength), "--description", new string('d', descriptionLength));

        Assert.Equal(valid ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        await Service.Received(valid ? 1 : 0).UpdateWorkspaceAsync(
            Arg.Any<string>(), Arg.Any<UpdateWorkspaceRequest>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(400)]
    [InlineData(401)]
    [InlineData(403)]
    [InlineData(404)]
    [InlineData(409)]
    [InlineData(429)]
    [InlineData(500)]
    [InlineData(503)]
    public async Task ExecuteAsync_PreservesStatusWithoutLeakingBackendDetails(int status)
    {
        Service.UpdateWorkspaceAsync(Arg.Any<string>(), Arg.Any<UpdateWorkspaceRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("private-backend-body test-token", null, (HttpStatusCode)status));

        var response = await ExecuteCommandAsync(
            "--workspace-id", WorkspaceUpdateTestData.WorkspaceId, "--description", "private-description");

        Assert.Equal((HttpStatusCode)status, response.Status);
        Assert.Null(response.Results);
        Assert.DoesNotContain("private-", response.Message);
        Assert.DoesNotContain("test-token", response.Message);
        var logs = string.Join('\n', Logger.ReceivedCalls().SelectMany(static call => call.GetArguments()).Select(static argument => argument?.ToString()));
        Assert.DoesNotContain("private-", logs);
        Assert.DoesNotContain("test-token", logs);
    }

    [Theory]
    [InlineData("authentication", HttpStatusCode.Unauthorized)]
    [InlineData("network", HttpStatusCode.ServiceUnavailable)]
    [InlineData("cancellation", HttpStatusCode.RequestTimeout)]
    [InlineData("json", HttpStatusCode.BadGateway)]
    [InlineData("unexpected", HttpStatusCode.InternalServerError)]
    public async Task ExecuteAsync_SanitizesNonHttpFailures(string failure, HttpStatusCode status)
    {
        Exception exception = failure switch
        {
            "authentication" => new AuthenticationFailedException("private-credential"),
            "network" => new HttpRequestException("private-network"),
            "cancellation" => new OperationCanceledException("private-cancellation"),
            "json" => new JsonException("private-payload"),
            _ => new Exception("private-unexpected")
        };
        Service.UpdateWorkspaceAsync(Arg.Any<string>(), Arg.Any<UpdateWorkspaceRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceUpdateTestData.WorkspaceId, "--description", "");

        Assert.Equal(status, response.Status);
        Assert.Null(response.Results);
        Assert.DoesNotContain("private-", response.Message);
    }
}
