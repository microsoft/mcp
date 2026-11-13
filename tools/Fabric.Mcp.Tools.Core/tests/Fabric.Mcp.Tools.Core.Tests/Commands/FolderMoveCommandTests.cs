// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;
using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using Xunit;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.FolderListTestData;

namespace Fabric.Mcp.Tools.Core.Tests.Commands;

public sealed class FolderMoveCommandTests() : CommandUnitTestsBase<FolderMoveCommand, IFabricCoreService>
{
    [Fact]
    public void Constructor_DefinesOneRemoteCapableHierarchyMutation()
    {
        Assert.Equal("move-folder", Command.Name);
        Assert.Equal("b9e01f3d-d6e4-4b8a-9c31-5f29bce16b7d", Command.Id);
        Assert.Equal("Move Fabric Folder", Command.Title);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
        Assert.True(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.False(Command.Metadata.Secret);
        Assert.Same(CoreJsonContext.Default.FolderMoveCommandResult, Command.ResultTypeInfo);
        Assert.Equal(["--folder-id", "--workspace-id"],
            CommandDefinition.Options.Where(option => option.Required).Select(option => option.Name).Order());
        Assert.DoesNotContain("Preview", Command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("contained folders and items", Command.Description);
        Assert.Throws<ArgumentNullException>(() => new FolderMoveCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new FolderMoveCommand(Logger, null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(ParentFolderId)]
    public async Task ExecuteAsync_BindsUnchangedInputsAndCancellationAndReturnsTyped200(string? target)
    {
        var workspace = Guid.Parse(WorkspaceId).ToString("B").ToUpperInvariant();
        var folderId = Guid.Parse(FolderId).ToString("N").ToUpperInvariant();
        var folder = Folder with { ParentFolderId = target is null ? null : Guid.Parse(target) };
        Service.MoveFolderAsync(workspace, folderId, target, Arg.Any<CancellationToken>()).Returns(new FolderMoveCommandResult(folder, 0));
        var arguments = new List<string> { "--workspace-id", workspace, "--folder-id", folderId };
        if (target is not null)
        {
            arguments.AddRange(["--target-folder-id", target]);
        }

        var response = await ExecuteCommandAsync([.. arguments]);

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.FolderMoveCommandResult);
        Assert.Equal(folder, result.Folder);
        Assert.Equal("Moved", result.MoveStatus);
        Assert.Equal(0, result.RetryAfterSeconds);
        Assert.Contains("confirmed folder movement (HTTP 200)", response.Message);
        await Service.Received(1).MoveFolderAsync(workspace, folderId, target, TestContext.Current.CancellationToken);
        Assert.Single(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_RejectsInvalidMissingAndUnsupportedInputsBeforeService()
    {
        foreach (var args in new[]
        {
            Array.Empty<string>(), ["--workspace-id", WorkspaceId], ["--folder-id", FolderId],
            ["--workspace-id", "private-invalid", "--folder-id", FolderId],
            ["--workspace-id", WorkspaceId, "--folder-id", Guid.Empty.ToString()],
            ["--workspace-id", WorkspaceId, "--folder-id", FolderId, "--target-folder-id", "private/name"],
            ["--workspace-id", WorkspaceId, "--folder-id", FolderId, "--target-folder-id", FolderId]
        })
        {
            var response = await ExecuteCommandAsync(args);
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
            FabricCoreErrorTestData.AssertSanitized(response);
        }
        foreach (var option in new[] { "--path", "--parent-folder-id", "--display-name", "--target-workspace-id", "--item-id",
            "--recursive", "--url", "--preview", "--sync" })
        {
            var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--folder-id", FolderId, option, "private");
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
            FabricCoreErrorTestData.AssertSanitized(response);
        }
        Assert.Empty(Service.ReceivedCalls());
        Assert.Empty(Logger.ReceivedCalls());
    }

    [Theory]
    [InlineData("credentials", HttpStatusCode.Unauthorized)]
    [InlineData("authentication", HttpStatusCode.Unauthorized)]
    [InlineData("argument", HttpStatusCode.BadRequest)]
    [InlineData("operation-canceled", HttpStatusCode.RequestTimeout)]
    [InlineData("timeout", HttpStatusCode.GatewayTimeout)]
    [InlineData("unexpected", HttpStatusCode.InternalServerError)]
    public async Task ExecuteAsync_SanitizesFailuresAndLogs(string failure, HttpStatusCode status)
    {
        Service.MoveFolderAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<FolderMoveCommandResult>(FabricCoreErrorTestData.CreateException(failure)));
        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--folder-id", FolderId);
        Assert.Equal(status, response.Status);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.NotEmpty(Logger.ReceivedCalls());
        Assert.DoesNotContain("private", string.Join(" ", Logger.ReceivedCalls()
            .SelectMany(call => call.GetArguments()).Select(argument => argument?.ToString())), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(false, null, "may have moved")]
    [InlineData(true, null, "request was not sent")]
    [InlineData(false, HttpStatusCode.OK, "confirmed folder movement (HTTP 200)")]
    public async Task ExecuteAsync_DistinguishesMutationOutcomesAndKeepsRetryGuidance(bool notSent, HttpStatusCode? received, string message)
    {
        Service.MoveFolderAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<FolderMoveCommandResult>(new FolderMoveRequestException(
                HttpStatusCode.RequestTimeout, new RetryConditionHeaderValue(TimeSpan.FromSeconds(60)),
                requestNotSent: notSent, responseStatusCode: received)));
        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--folder-id", FolderId);
        Assert.Equal(HttpStatusCode.RequestTimeout, response.Status);
        Assert.Contains(message, response.Message);
        Assert.Contains("Wait at least 60 seconds", response.Message);
        Assert.Equal(!notSent && received is null, response.Message.Contains("may have moved", StringComparison.Ordinal));
        FabricCoreErrorTestData.AssertSanitized(response);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "FolderDepthOutOfRange")]
    [InlineData(HttpStatusCode.BadRequest, "InfiniteFolderHierarchyLoop")]
    [InlineData(HttpStatusCode.Conflict, "FolderDisplayNameAlreadyInUse")]
    [InlineData(HttpStatusCode.NotFound, "FolderNotFound")]
    public async Task ExecuteAsync_KnownRejectionsRemainFailuresWithSafeCodes(HttpStatusCode status, string code)
    {
        Service.MoveFolderAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<FolderMoveCommandResult>(new FolderMoveRequestException(
                status, new RetryConditionHeaderValue(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero)),
                code, errorMetadataUnavailable: true, responseStatusCode: status)));
        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--folder-id", FolderId);
        Assert.Equal(status, response.Status);
        Assert.Contains(code, response.Message);
        Assert.Contains("Additional error metadata was unavailable", response.Message);
        Assert.Contains("Wait until at least 2030-01-01 00:00:00 UTC", response.Message);
        Assert.DoesNotContain("may have moved", response.Message);
        Assert.DoesNotContain("request was not sent", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
    }
}
