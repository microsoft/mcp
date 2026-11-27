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
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemMoveTestData;

namespace Fabric.Mcp.Tools.Core.Tests.Commands;

public sealed class ItemMoveCommandTests() : CommandUnitTestsBase<ItemMoveCommand, IFabricCoreService>
{
    [Fact]
    public void Constructor_DefinesOneRemoteCapablePlacementMutation()
    {
        Assert.Equal("move-item", Command.Name);
        Assert.Equal("693dfc6f-6a98-4a80-bb45-891526b00ee3", Command.Id);
        Assert.Equal("Move Fabric Item", Command.Title);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
        Assert.True(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.False(Command.Metadata.Secret);
        Assert.Same(CoreJsonContext.Default.ItemMoveCommandResult, Command.ResultTypeInfo);
        Assert.Equal(["--item-id", "--workspace-id"],
            CommandDefinition.Options.Where(option => option.Required).Select(option => option.Name).Order());
        Assert.DoesNotContain("Preview", Command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CannotMoveChildOnly", Command.Description);
        Assert.Contains("complete metadata collection", Command.Description);
        Assert.Throws<ArgumentNullException>(() => new ItemMoveCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new ItemMoveCommand(Logger, null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(FolderId)]
    public async Task ExecuteAsync_BindsUnchangedInputsAndCancellationAndReturnsTheCollection(string? target)
    {
        var workspace = Guid.Parse(WorkspaceId).ToString("B").ToUpperInvariant();
        var itemId = Guid.Parse(ItemId).ToString("N").ToUpperInvariant();
        FabricMovedItem[] items = [Item(target), Item(target) with { Id = Guid.Parse(ChildId), Type = "SQLEndpoint" }];
        Service.MoveItemAsync(workspace, itemId, target, Arg.Any<CancellationToken>()).Returns(new ItemMoveCommandResult(items, 0));
        var arguments = new List<string> { "--workspace-id", workspace, "--item-id", itemId };
        if (target is not null)
        {
            arguments.AddRange(["--target-folder-id", target]);
        }

        var response = await ExecuteCommandAsync([.. arguments]);

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.ItemMoveCommandResult);
        Assert.Equal(items, result.Items);
        Assert.Equal("Moved", result.MoveStatus);
        Assert.Equal(0, result.RetryAfterSeconds);
        Assert.Contains("confirmed item movement (HTTP 200)", response.Message);
        await Service.Received(1).MoveItemAsync(workspace, itemId, target, TestContext.Current.CancellationToken);
        Assert.Single(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_RejectsInvalidMissingAndUnsupportedInputsBeforeService()
    {
        foreach (var args in new[]
        {
            Array.Empty<string>(), ["--workspace-id", WorkspaceId], ["--item-id", ItemId],
            ["--workspace-id", "private-invalid", "--item-id", ItemId],
            ["--workspace-id", WorkspaceId, "--item-id", Guid.Empty.ToString()],
            ["--workspace-id", WorkspaceId, "--item-id", ItemId, "--target-folder-id", "private/name"],
            ["--workspace-id", WorkspaceId, "--item-id", ItemId, "--target-folder-id", ""]
        })
        {
            var response = await ExecuteCommandAsync(args);
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
            FabricCoreErrorTestData.AssertSanitized(response);
        }
        foreach (var option in new[] { "--path", "--folder-id", "--display-name", "--target-workspace-id", "--parent-item-id",
            "--recursive", "--url", "--preview", "--sync", "--continuation-token" })
        {
            var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--item-id", ItemId, option, "private");
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
        Service.MoveItemAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ItemMoveCommandResult>(FabricCoreErrorTestData.CreateException(failure)));
        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--item-id", ItemId);
        Assert.Equal(status, response.Status);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.NotEmpty(Logger.ReceivedCalls());
        Assert.DoesNotContain("private", string.Join(" ", Logger.ReceivedCalls()
            .SelectMany(call => call.GetArguments()).Select(argument => argument?.ToString())), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(false, null, "may have moved")]
    [InlineData(true, null, "request was not sent")]
    [InlineData(false, HttpStatusCode.OK, "confirmed item movement (HTTP 200)")]
    public async Task ExecuteAsync_DistinguishesMutationOutcomesAndKeepsRetryGuidance(bool notSent, HttpStatusCode? received, string message)
    {
        Service.MoveItemAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ItemMoveCommandResult>(new ItemMoveRequestException(
                HttpStatusCode.RequestTimeout, new RetryConditionHeaderValue(TimeSpan.FromSeconds(60)),
                requestNotSent: notSent, responseStatusCode: received)));
        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--item-id", ItemId);
        Assert.Equal(HttpStatusCode.RequestTimeout, response.Status);
        Assert.Contains(message, response.Message);
        Assert.Contains("Wait at least 60 seconds", response.Message);
        Assert.Equal(!notSent && received is null, response.Message.Contains("may have moved", StringComparison.Ordinal));
        FabricCoreErrorTestData.AssertSanitized(response);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "CannotMoveChildOnly")]
    [InlineData(HttpStatusCode.NotFound, "ItemNotFound")]
    [InlineData(HttpStatusCode.NotFound, "FolderNotFound")]
    [InlineData(HttpStatusCode.Forbidden, "InsufficientPrivileges")]
    [InlineData(HttpStatusCode.Conflict, "Conflict")]
    [InlineData(HttpStatusCode.TooManyRequests, "RequestBlocked")]
    public async Task ExecuteAsync_KnownRejectionsRemainFailuresWithSafeCodes(HttpStatusCode status, string code)
    {
        Service.MoveItemAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ItemMoveCommandResult>(new ItemMoveRequestException(
                status, new RetryConditionHeaderValue(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero)),
                code, errorMetadataUnavailable: true, responseStatusCode: status)));
        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--item-id", ItemId);
        Assert.Equal(status, response.Status);
        Assert.Contains(code, response.Message);
        Assert.Contains("Additional error metadata was unavailable", response.Message);
        Assert.Contains("Wait until at least 2030-01-01 00:00:00 UTC", response.Message);
        Assert.DoesNotContain("may have moved", response.Message);
        Assert.DoesNotContain("request was not sent", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
    }
}
