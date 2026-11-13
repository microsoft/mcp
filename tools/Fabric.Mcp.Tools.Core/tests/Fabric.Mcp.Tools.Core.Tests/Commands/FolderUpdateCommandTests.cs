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

public sealed class FolderUpdateCommandTests() : CommandUnitTestsBase<FolderUpdateCommand, IFabricCoreService>
{
    [Fact]
    public void Constructor_DefinesOneRemoteCapableMetadataReplacement()
    {
        Assert.Equal("update-folder", Command.Name);
        Assert.Equal("178a1c7d-7799-4ade-b5cd-f386cdb9aa64", Command.Id);
        Assert.Equal("Update Fabric Folder", Command.Title);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
        Assert.True(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.False(Command.Metadata.Secret);
        Assert.Same(CoreJsonContext.Default.FolderUpdateCommandResult, Command.ResultTypeInfo);
        Assert.Equal(["--display-name", "--folder-id", "--workspace-id"],
            CommandDefinition.Options.Where(option => option.Required).Select(option => option.Name).Order());
        Assert.DoesNotContain("Preview", Command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Throws<ArgumentNullException>(() => new FolderUpdateCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new FolderUpdateCommand(Logger, null!));
    }

    [Fact]
    public async Task ExecuteAsync_BindsUnchangedInputsAndCancellationAndReturnsTyped200()
    {
        var workspace = Guid.Parse(WorkspaceId).ToString("B").ToUpperInvariant();
        var folderId = Guid.Parse(FolderId).ToString("N").ToUpperInvariant();
        const string name = "Sales \u00e9\U0001F680";
        var folder = Folder with { DisplayName = name };
        Service.UpdateFolderAsync(workspace, folderId, name, Arg.Any<CancellationToken>()).Returns(new FolderUpdateCommandResult(folder, 0));

        var response = await ExecuteCommandAsync("--workspace-id", workspace, "--folder-id", folderId, "--display-name", name);

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.FolderUpdateCommandResult);
        Assert.Equal(folder, result.Folder);
        Assert.Equal("Updated", result.UpdateStatus);
        Assert.Equal(0, result.RetryAfterSeconds);
        Assert.Contains("confirmed folder update (HTTP 200)", response.Message);
        await Service.Received(1).UpdateFolderAsync(workspace, folderId, name, TestContext.Current.CancellationToken);
        Assert.Single(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_RejectsInvalidMissingAndUnsupportedInputsBeforeService()
    {
        foreach (var args in new[]
        {
            Array.Empty<string>(), ["--workspace-id", WorkspaceId, "--folder-id", FolderId],
            ["--display-name", "Sales", "--folder-id", FolderId],
            ["--workspace-id", WorkspaceId, "--display-name", "Sales"],
            ["--workspace-id", "private-invalid", "--folder-id", FolderId, "--display-name", "Sales"],
            ["--workspace-id", WorkspaceId, "--folder-id", Guid.Empty.ToString(), "--display-name", "Sales"],
            ["--workspace-id", WorkspaceId, "--folder-id", FolderId, "--display-name", "private/name"]
        })
        {
            var response = await ExecuteCommandAsync(args);
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
            FabricCoreErrorTestData.AssertSanitized(response);
        }
        foreach (var option in new[] { "--path", "--parent-folder-id", "--description", "--recursive", "--url", "--preview", "--sync" })
        {
            var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--folder-id", FolderId, "--display-name", "Sales", option, "private");
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
        Service.UpdateFolderAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<FolderUpdateCommandResult>(FabricCoreErrorTestData.CreateException(failure)));
        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--folder-id", FolderId, "--display-name", "Sales");
        Assert.Equal(status, response.Status);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.DoesNotContain("private", string.Join(" ", Logger.ReceivedCalls()
            .SelectMany(call => call.GetArguments()).Select(argument => argument?.ToString())), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(false, null, "may have been applied")]
    [InlineData(true, null, "request was not sent")]
    [InlineData(false, HttpStatusCode.OK, "confirmed folder update (HTTP 200)")]
    public async Task ExecuteAsync_DistinguishesMutationOutcomesAndKeepsRetryGuidance(bool notSent, HttpStatusCode? received, string message)
    {
        Service.UpdateFolderAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<FolderUpdateCommandResult>(new FolderUpdateRequestException(
                HttpStatusCode.RequestTimeout, new RetryConditionHeaderValue(TimeSpan.FromSeconds(60)),
                requestNotSent: notSent, responseStatusCode: received)));
        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--folder-id", FolderId, "--display-name", "Sales");
        Assert.Equal(HttpStatusCode.RequestTimeout, response.Status);
        Assert.Contains(message, response.Message);
        Assert.Contains("Wait at least 60 seconds", response.Message);
        Assert.Equal(!notSent && received is null, response.Message.Contains("may have been applied", StringComparison.Ordinal));
        FabricCoreErrorTestData.AssertSanitized(response);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "InvalidFolderDisplayName")]
    [InlineData(HttpStatusCode.Conflict, "FolderDisplayNameAlreadyInUse")]
    [InlineData(HttpStatusCode.NotFound, "FolderNotFound")]
    public async Task ExecuteAsync_KnownRejectionsRemainFailuresWithSafeCodes(HttpStatusCode status, string code)
    {
        Service.UpdateFolderAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<FolderUpdateCommandResult>(new FolderUpdateRequestException(
                status, new RetryConditionHeaderValue(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero)),
                code, errorMetadataUnavailable: true, responseStatusCode: status)));
        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--folder-id", FolderId, "--display-name", "Sales");
        Assert.Equal(status, response.Status);
        Assert.Contains(code, response.Message);
        Assert.Contains("Additional error metadata was unavailable", response.Message);
        Assert.Contains("Wait until at least 2030-01-01 00:00:00 UTC", response.Message);
        Assert.DoesNotContain("may have been applied", response.Message);
        Assert.DoesNotContain("request was not sent", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
    }
}
