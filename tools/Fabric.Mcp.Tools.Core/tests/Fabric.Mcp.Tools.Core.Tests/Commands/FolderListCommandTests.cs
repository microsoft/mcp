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

public sealed class FolderListCommandTests() : CommandUnitTestsBase<FolderListCommand, IFabricCoreService>
{
    [Fact]
    public void Constructor_DefinesOneReadOnlyRemoteCapableTool()
    {
        Assert.Equal("list-folders", Command.Name);
        Assert.Equal("9b609c7c-5f5e-46da-bcaa-94dceca44c93", Command.Id);
        Assert.Equal("List Fabric Folders", Command.Title);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.False(Command.Metadata.Secret);
        Assert.Same(CoreJsonContext.Default.FolderListCommandResult, Command.ResultTypeInfo);
        Assert.Equal("--workspace-id", Assert.Single(CommandDefinition.Options, option => option.Required).Name);
        Assert.Throws<ArgumentNullException>(() => new FolderListCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new FolderListCommand(Logger, null!));
    }

    [Theory]
    [InlineData(null, null, null)]
    [InlineData(ParentFolderId, true, DocumentedToken)]
    [InlineData(ParentFolderId, false, " raw+/= &?# ")]
    public async Task ExecuteAsync_BindsUnchangedOptionsAndForwardsCancellation(string? root, bool? recursive, string? token)
    {
        var workspace = Guid.Parse(WorkspaceId).ToString("B").ToUpperInvariant();
        Service.ListFoldersAsync(workspace, root, recursive, token, Arg.Any<CancellationToken>())
            .Returns(new FolderListCommandResult([Folder], DocumentedToken, 45));
        List<string> args = ["--workspace-id", workspace];
        if (root is not null)
        {
            args.AddRange(["--root-folder-id", root]);
        }
        if (recursive is not null)
        {
            args.AddRange(["--recursive", recursive.Value.ToString()]);
        }
        if (token is not null)
        {
            args.AddRange(["--continuation-token", token]);
        }

        var response = await ExecuteCommandAsync(args.ToArray());

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.FolderListCommandResult);
        Assert.Equal(Folder, Assert.Single(result.Folders));
        Assert.Equal(DocumentedToken, result.ContinuationToken);
        Assert.Equal(45, result.RetryAfterSeconds);
        Assert.Null(result.RetryAfterUtc);
        await Service.Received(1).ListFoldersAsync(workspace, root, recursive, token, TestContext.Current.CancellationToken);
        Assert.Single(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsAnEmptyPageWithAContinuationToken()
    {
        Service.ListFoldersAsync(WorkspaceId, null, null, null, Arg.Any<CancellationToken>())
            .Returns(new FolderListCommandResult([], DocumentedToken));

        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId);

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.FolderListCommandResult);
        Assert.Empty(result.Folders);
        Assert.Equal(DocumentedToken, result.ContinuationToken);
    }

    [Theory]
    [InlineData("--workspace-id")]
    [InlineData("--root-folder-id")]
    public async Task ExecuteAsync_RejectsInvalidIdsBeforeService(string option)
    {
        foreach (var value in new[] { "", " ", "private-invalid", Guid.Empty.ToString(), "../private", FolderId + "?query=private" })
        {
            var args = new Dictionary<string, string> { ["--workspace-id"] = WorkspaceId, [option] = value };
            var response = await ExecuteCommandAsync(args.SelectMany(pair => new[] { pair.Key, pair.Value }).ToArray());
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
            FabricCoreErrorTestData.AssertSanitized(response);
            Assert.Empty(Service.ReceivedCalls());
        }
    }

    [Fact]
    public async Task ExecuteAsync_RequiresAnExplicitWorkspace()
    {
        var response = await ExecuteCommandAsync();
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--workspace-id", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_RejectsInvalidTokensWithoutLoggingThem()
    {
        foreach (var token in new[] { "", " ", "private\r\ninput", "\uD800", "\uDC00", new string('x', 8193) })
        {
            var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--continuation-token", token);
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
            FabricCoreErrorTestData.AssertSanitized(response);
            Assert.Empty(Service.ReceivedCalls());
            Assert.Empty(Logger.ReceivedCalls());
        }
    }

    [Theory]
    [InlineData("--folder-id")]
    [InlineData("--path")]
    [InlineData("--type")]
    [InlineData("--all")]
    [InlineData("--page-size")]
    [InlineData("--continuation-uri")]
    [InlineData("--url")]
    [InlineData("--preview")]
    [InlineData("--sync")]
    public async Task ExecuteAsync_RejectsUnsupportedOptionsWithoutEchoingInput(string option)
    {
        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, option, FabricCoreErrorTestData.PrivateDetails);
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
        Service.ListFoldersAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<FolderListCommandResult>(exception));

        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--continuation-token", "private-token");

        Assert.Equal(status, response.Status);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.DoesNotContain("private", string.Join(" ", Logger.ReceivedCalls()
            .SelectMany(call => call.GetArguments()).Select(argument => argument?.ToString())), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, null, "Check the workspace")]
    [InlineData(HttpStatusCode.Unauthorized, null, "Authentication failed")]
    [InlineData(HttpStatusCode.Forbidden, null, "Workspace.Read.All")]
    [InlineData(HttpStatusCode.NotFound, null, "root folder was not found")]
    [InlineData(HttpStatusCode.TooManyRequests, null, "Wait before retrying")]
    [InlineData(HttpStatusCode.TooManyRequests, "0", "Wait at least 0 seconds")]
    [InlineData(HttpStatusCode.TooManyRequests, "120", "Wait at least 120 seconds")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "Tue, 01 Jan 2030 00:00:00 GMT", "Retry after 2030-01-01 00:00:00 UTC")]
    [InlineData(HttpStatusCode.BadGateway, null, "No partial page")]
    public async Task ExecuteAsync_RetainsHttpStatusAndValidatedRetryGuidance(HttpStatusCode status, string? retryAfter, string message)
    {
        Service.ListFoldersAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<FolderListCommandResult>(
                new FolderListRequestException(status, retryAfter is null ? null : RetryConditionHeaderValue.Parse(retryAfter))));

        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId);

        Assert.Equal(status, response.Status);
        Assert.Contains(message, response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
    }
}
