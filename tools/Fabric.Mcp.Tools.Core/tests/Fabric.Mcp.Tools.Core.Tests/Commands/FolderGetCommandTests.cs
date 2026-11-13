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

public sealed class FolderGetCommandTests() : CommandUnitTestsBase<FolderGetCommand, IFabricCoreService>
{
    [Fact]
    public void Constructor_DefinesOneReadOnlyRemoteCapableMetadataTool()
    {
        Assert.Equal("get-folder", Command.Name);
        Assert.Equal("86cfc30e-580c-4c62-9da8-b346177bca68", Command.Id);
        Assert.Equal("Get Fabric Folder", Command.Title);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.False(Command.Metadata.Secret);
        Assert.Same(CoreJsonContext.Default.FolderGetCommandResult, Command.ResultTypeInfo);
        Assert.Equal(["--folder-id", "--workspace-id"], CommandDefinition.Options.Where(option => option.Required).Select(option => option.Name).Order());
        Assert.DoesNotContain("Preview", Command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Throws<ArgumentNullException>(() => new FolderGetCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new FolderGetCommand(Logger, null!));
    }

    [Fact]
    public async Task ExecuteAsync_BindsBothIdsAndForwardsCancellation()
    {
        var workspace = Guid.Parse(WorkspaceId).ToString("B").ToUpperInvariant();
        var folder = Guid.Parse(FolderId).ToString("N").ToUpperInvariant();
        Service.GetFolderAsync(workspace, folder, Arg.Any<CancellationToken>())
            .Returns(new FolderGetCommandResult(Folder, 45));

        var response = await ExecuteCommandAsync("--workspace-id", workspace, "--folder-id", folder);

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.FolderGetCommandResult);
        Assert.Equal(Folder, result.Folder);
        Assert.Equal(45, result.RetryAfterSeconds);
        Assert.Null(result.RetryAfterUtc);
        await Service.Received(1).GetFolderAsync(workspace, folder, TestContext.Current.CancellationToken);
        Assert.Single(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--workspace-id")]
    [InlineData("--folder-id")]
    public async Task ExecuteAsync_RequiresBothIdsAndRejectsInvalidIdsBeforeService(string option)
    {
        var arguments = new Dictionary<string, string> { ["--workspace-id"] = WorkspaceId, ["--folder-id"] = FolderId };
        arguments.Remove(option);
        var missing = await ExecuteCommandAsync(arguments.SelectMany(pair => new[] { pair.Key, pair.Value }).ToArray());
        Assert.Equal(HttpStatusCode.BadRequest, missing.Status);
        Assert.Contains(option, missing.Message);
        FabricCoreErrorTestData.AssertSanitized(missing);
        foreach (var value in new[] { "", " ", "private-invalid", Guid.Empty.ToString(), "../private", FolderId + "?query=private" })
        {
            arguments[option] = value;
            var response = await ExecuteCommandAsync(arguments.SelectMany(pair => new[] { pair.Key, pair.Value }).ToArray());
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
            FabricCoreErrorTestData.AssertSanitized(response);
        }
        Assert.Empty(Service.ReceivedCalls());
        Assert.Empty(Logger.ReceivedCalls());
    }

    [Theory]
    [InlineData("--display-name")]
    [InlineData("--parent-folder-id")]
    [InlineData("--root-folder-id")]
    [InlineData("--path")]
    [InlineData("--recursive")]
    [InlineData("--continuation-token")]
    [InlineData("--continuation-uri")]
    [InlineData("--url")]
    [InlineData("--preview")]
    [InlineData("--sync")]
    public async Task ExecuteAsync_RejectsUnsupportedOptionsWithoutEchoingInput(string option)
    {
        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--folder-id", FolderId,
            option, FabricCoreErrorTestData.PrivateDetails);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("credentials", HttpStatusCode.Unauthorized)]
    [InlineData("authentication", HttpStatusCode.Unauthorized)]
    [InlineData("argument", HttpStatusCode.BadRequest)]
    [InlineData("operation-canceled", HttpStatusCode.RequestTimeout)]
    [InlineData("task-canceled", HttpStatusCode.RequestTimeout)]
    [InlineData("timeout", HttpStatusCode.GatewayTimeout)]
    [InlineData("json", HttpStatusCode.BadGateway)]
    [InlineData("network", HttpStatusCode.ServiceUnavailable)]
    [InlineData("unexpected", HttpStatusCode.InternalServerError)]
    public async Task ExecuteAsync_SanitizesFailuresAndLogs(string failure, HttpStatusCode status)
    {
        var exception = failure == "network"
            ? new HttpRequestException(FabricCoreErrorTestData.PrivateDetails)
            : FabricCoreErrorTestData.CreateException(failure);
        Service.GetFolderAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<FolderGetCommandResult>(exception));

        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--folder-id", FolderId);

        Assert.Equal(status, response.Status);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.DoesNotContain("private", string.Join(" ", Logger.ReceivedCalls()
            .SelectMany(call => call.GetArguments()).Select(argument => argument?.ToString())), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("creation", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("created", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("mutation", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, null, "Provide valid workspace")]
    [InlineData(HttpStatusCode.Unauthorized, null, "Authentication failed")]
    [InlineData(HttpStatusCode.Forbidden, null, "Workspace.Read.All")]
    [InlineData(HttpStatusCode.NotFound, null, "folder was not found")]
    [InlineData(HttpStatusCode.TooManyRequests, null, "Wait before retrying")]
    [InlineData(HttpStatusCode.TooManyRequests, "0", "Wait at least 0 seconds")]
    [InlineData(HttpStatusCode.TooManyRequests, "120", "Wait at least 120 seconds")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "Tue, 01 Jan 2030 00:00:00 GMT", "Retry after 2030-01-01 00:00:00 UTC")]
    [InlineData(HttpStatusCode.BadGateway, null, "invalid folder metadata")]
    public async Task ExecuteAsync_PreservesStatusSafeCodesAndRetryGuidance(HttpStatusCode status, string? retryAfter, string message)
    {
        Service.GetFolderAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<FolderGetCommandResult>(new FolderGetRequestException(
                status, retryAfter is null ? null : RetryConditionHeaderValue.Parse(retryAfter), "FolderNotFound")));
        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--folder-id", FolderId);
        Assert.Equal(status, response.Status);
        Assert.Contains(message, response.Message);
        Assert.Contains("Fabric error code: FolderNotFound", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);

        Service.GetFolderAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<FolderGetCommandResult>(new FolderGetRequestException(
                status, errorCode: FabricCoreErrorTestData.PrivateDetails, errorMetadataUnavailable: true)));
        var unavailable = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--folder-id", FolderId);
        Assert.Equal(status, unavailable.Status);
        Assert.Contains("Additional error metadata was unavailable", unavailable.Message);
        FabricCoreErrorTestData.AssertSanitized(unavailable);
    }
}
