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

public sealed class FolderDeleteCommandTests() : CommandUnitTestsBase<FolderDeleteCommand, IFabricCoreService>
{
    [Fact]
    public void Constructor_DefinesOneRemoteCapableEmptyFolderDeletion()
    {
        Assert.Equal("delete-folder", Command.Name);
        Assert.Equal("db1ef2ca-f276-40ae-9b96-9ce98f8298b2", Command.Id);
        Assert.Equal("Delete Fabric Folder", Command.Title);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.False(Command.Metadata.Secret);
        Assert.Same(CoreJsonContext.Default.FolderDeleteCommandResult, Command.ResultTypeInfo);
        Assert.Equal(["--folder-id", "--workspace-id"],
            CommandDefinition.Options.Where(option => option.Required).Select(option => option.Name).Order());
        Assert.Contains("FolderNotEmpty", Command.Description);
        Assert.Contains("soft-deleted items", Command.Description);
        Assert.Contains("does not document folder recovery or a retention period", Command.Description);
        Assert.DoesNotContain("Preview", Command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Throws<ArgumentNullException>(() => new FolderDeleteCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new FolderDeleteCommand(Logger, null!));
    }

    [Fact]
    public async Task ExecuteAsync_BindsExplicitIdsAndCancellationAndReturnsTypedReceipt()
    {
        var workspace = Guid.Parse(WorkspaceId).ToString("B").ToUpperInvariant();
        var folder = Guid.Parse(FolderId).ToString("N").ToUpperInvariant();
        Service.DeleteFolderAsync(workspace, folder, Arg.Any<CancellationToken>())
            .Returns(new FolderDeleteCommandResult(Guid.Parse(WorkspaceId), Guid.Parse(FolderId)));

        var response = await ExecuteCommandAsync("--workspace-id", workspace, "--folder-id", folder);

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.FolderDeleteCommandResult);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal(Guid.Parse(WorkspaceId), result.WorkspaceId);
        Assert.Equal(Guid.Parse(FolderId), result.FolderId);
        Assert.Equal("Deleted", result.DeletionStatus);
        Assert.Contains("confirmed deletion of the requested empty folder (HTTP 200)", response.Message);
        Assert.Equal(result.Guidance, response.Message);
        await Service.Received(1).DeleteFolderAsync(workspace, folder, TestContext.Current.CancellationToken);
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
            ["--workspace-id", WorkspaceId, "--folder-id", "private/path"]
        })
        {
            var response = await ExecuteCommandAsync(args);
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
            Assert.Contains("No DELETE request was sent", response.Message);
            FabricCoreErrorTestData.AssertSanitized(response);
        }
        foreach (var option in new[] { "--path", "--parent-folder-id", "--display-name", "--item-id",
            "--recursive", "--force", "--hard-delete", "--url", "--preview", "--sync", "--operation-id" })
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
        Service.DeleteFolderAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<FolderDeleteCommandResult>(FabricCoreErrorTestData.CreateException(failure)));
        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--folder-id", FolderId);
        Assert.Equal(status, response.Status);
        Assert.Equal(failure is "credentials" or "authentication" or "argument",
            response.Message.Contains("No DELETE request was sent", StringComparison.Ordinal));
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.NotEmpty(Logger.ReceivedCalls());
        Assert.DoesNotContain("private", string.Join(" ", Logger.ReceivedCalls()
            .SelectMany(call => call.GetArguments()).Select(argument => argument?.ToString())), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(false, null, "may have been deleted")]
    [InlineData(true, null, "No DELETE request was sent")]
    [InlineData(false, HttpStatusCode.OK, "confirmed folder deletion (HTTP 200)")]
    public async Task ExecuteAsync_DistinguishesMutationOutcomes(bool notSent, HttpStatusCode? received, string message)
    {
        Service.DeleteFolderAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<FolderDeleteCommandResult>(new FolderDeleteRequestException(
                HttpStatusCode.RequestTimeout, requestNotSent: notSent, responseStatusCode: received)));
        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--folder-id", FolderId);
        Assert.Equal(HttpStatusCode.RequestTimeout, response.Status);
        Assert.Contains(message, response.Message);
        Assert.Equal(!notSent && received is null, response.Message.Contains("may have been deleted", StringComparison.Ordinal));
        FabricCoreErrorTestData.AssertSanitized(response);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "FolderNotEmpty")]
    [InlineData(HttpStatusCode.Conflict, "FolderNotEmpty")]
    [InlineData(HttpStatusCode.NotFound, "FolderNotFound")]
    [InlineData(HttpStatusCode.Forbidden, "InsufficientPrivileges")]
    [InlineData(HttpStatusCode.TooManyRequests, "RequestBlocked")]
    public async Task ExecuteAsync_KnownRejectionsKeepStatusAndSafeRetryGuidance(HttpStatusCode status, string code)
    {
        Service.DeleteFolderAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<FolderDeleteCommandResult>(new FolderDeleteRequestException(
                status, new RetryConditionHeaderValue(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero)),
                code, errorMetadataUnavailable: true, responseStatusCode: status)));
        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--folder-id", FolderId);
        Assert.Equal(status, response.Status);
        Assert.Contains(code, response.Message);
        Assert.Contains("Optional Fabric error metadata was unavailable", response.Message);
        Assert.Contains("Wait until at least 2030-01-01 00:00:00 UTC", response.Message);
        Assert.DoesNotContain("may have been deleted", response.Message);
        Assert.DoesNotContain("No DELETE request was sent", response.Message);
        if (code == "FolderNotEmpty")
        {
            Assert.Contains("soft-deleted items", response.Message);
        }
        FabricCoreErrorTestData.AssertSanitized(response);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotExposeUnsafeErrorCodes()
    {
        Service.DeleteFolderAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<FolderDeleteCommandResult>(new FolderDeleteRequestException(
                HttpStatusCode.TooManyRequests, new RetryConditionHeaderValue(TimeSpan.FromSeconds(60)),
                "private unsafe", responseStatusCode: HttpStatusCode.TooManyRequests)));
        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--folder-id", FolderId);
        Assert.Contains("Wait at least 60 seconds", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
    }
}
