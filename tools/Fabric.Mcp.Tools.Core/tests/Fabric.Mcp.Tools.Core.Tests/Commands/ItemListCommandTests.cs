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

public class ItemListCommandTests : CommandUnitTestsBase<ItemListCommand, IFabricCoreService>
{
    [Fact]
    public void Constructor_DeclaresReadOnlyRemoteMetadata()
    {
        Assert.Equal("list-items", Command.Name);
        Assert.Equal("List Fabric Items", Command.Title);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.False(Command.Metadata.Secret);
        Assert.Same(CoreJsonContext.Default.ItemListCommandResult, Command.ResultTypeInfo);
        Assert.Equal("--workspace-id", Assert.Single(CommandDefinition.Options, option => option.Required).Name);
        Assert.Equal(5, CommandDefinition.Options.Count);
        Assert.Throws<ArgumentNullException>(() => new ItemListCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new ItemListCommand(Logger, null!));
    }

    [Fact]
    public async Task ExecuteAsync_DefaultsToRecursiveAndReturnsEmptyPage()
    {
        ConfigurePage(new([]));

        var response = await ExecuteCommandAsync("--workspace-id", ItemListTestData.WorkspaceId);

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.ItemListCommandResult);
        Assert.Empty(result.Items);
        Assert.Null(result.ContinuationToken);
        Assert.Null(result.ContinuationUri);
        await Service.Received(1).ListItemsAsync(ItemListTestData.WorkspaceId, null, true, null, null, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExecuteAsync_ForwardsAllFiltersAndContinuation(bool recursive)
    {
        var page = JsonSerializer.Deserialize(ItemListTestData.PageJson, CoreJsonContext.Default.ItemListResponse)!;
        ConfigurePage(page);

        var response = await ExecuteCommandAsync(
            "--workspace-id", ItemListTestData.WorkspaceId, "--type", "FutureItemType",
            "--recursive", recursive.ToString(), "--root-folder-id", ItemListTestData.FolderId,
            "--continuation-token", ItemListTestData.Token);

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.ItemListCommandResult);
        Assert.Equal("FutureItemType", Assert.Single(result.Items).Type);
        Assert.Equal(ItemListTestData.Token, result.ContinuationToken);
        Assert.Equal(ItemListTestData.ContinuationUri, result.ContinuationUri);
        await Service.Received(1).ListItemsAsync(ItemListTestData.WorkspaceId, "FutureItemType", recursive,
            ItemListTestData.FolderId, ItemListTestData.Token, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("--workspace-id", "")]
    [InlineData("--workspace-id", " ")]
    [InlineData("--workspace-id", "not-a-uuid")]
    [InlineData("--workspace-id", "00000000-0000-0000-0000-000000000000")]
    [InlineData("--workspace-id", ItemListTestData.WorkspaceId + "/items?include=DefaultIdentity")]
    [InlineData("--root-folder-id", "")]
    [InlineData("--root-folder-id", " ")]
    [InlineData("--root-folder-id", "not-a-uuid")]
    [InlineData("--root-folder-id", "00000000-0000-0000-0000-000000000000")]
    [InlineData("--root-folder-id", "../items")]
    [InlineData("--type", "")]
    [InlineData("--type", " ")]
    [InlineData("--continuation-token", "")]
    [InlineData("--continuation-token", " ")]
    [InlineData("--recursive", "invalid")]
    public async Task ExecuteAsync_RejectsInvalidOptionsWithoutCallingService(string option, string value)
    {
        string[] args = option == "--workspace-id"
            ? [option, value]
            : ["--workspace-id", ItemListTestData.WorkspaceId, option, value];

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Null(response.Results);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_RequiresWorkspace()
    {
        var response = await ExecuteCommandAsync(Array.Empty<string>());

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("workspace-id", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_AcceptsAlternateUuidForms()
    {
        ConfigurePage(new([]));
        var workspace = $"{{{ItemListTestData.WorkspaceId.ToUpperInvariant()}}}";
        var folder = Guid.Parse(ItemListTestData.FolderId).ToString("N").ToUpperInvariant();

        var response = await ExecuteCommandAsync("--workspace-id", workspace, "--root-folder-id", folder);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).ListItemsAsync(workspace, null, true, folder, null, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("http400", HttpStatusCode.BadRequest)]
    [InlineData("http401", HttpStatusCode.Unauthorized)]
    [InlineData("http403", HttpStatusCode.Forbidden)]
    [InlineData("http404", HttpStatusCode.NotFound)]
    [InlineData("http429", HttpStatusCode.TooManyRequests)]
    [InlineData("http500", HttpStatusCode.InternalServerError)]
    [InlineData("http503", HttpStatusCode.ServiceUnavailable)]
    [InlineData("network", HttpStatusCode.ServiceUnavailable)]
    [InlineData("auth", HttpStatusCode.Unauthorized)]
    [InlineData("json", HttpStatusCode.BadGateway)]
    [InlineData("cancel", HttpStatusCode.RequestTimeout)]
    [InlineData("task-cancel", HttpStatusCode.RequestTimeout)]
    [InlineData("timeout", HttpStatusCode.GatewayTimeout)]
    [InlineData("unexpected", HttpStatusCode.InternalServerError)]
    public async Task ExecuteAsync_SanitizesFailuresAndPreservesStatus(string kind, HttpStatusCode expected)
    {
        Exception exception = kind switch
        {
            "network" => new HttpRequestException("private-detail"),
            "auth" => new AuthenticationFailedException("private-detail"),
            "json" => new JsonException("private-detail"),
            "cancel" => new OperationCanceledException("private-detail"),
            "task-cancel" => new TaskCanceledException("private-detail"),
            "timeout" => new TimeoutException("private-detail"),
            "unexpected" => new Exception("private-detail"),
            _ => new HttpRequestException("private-detail", null, expected)
        };
        Service.ListItemsAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<CancellationToken>()).ThrowsAsync(exception);

        var response = await ExecuteCommandAsync("--workspace-id", ItemListTestData.WorkspaceId);

        Assert.Equal(expected, response.Status);
        Assert.Null(response.Results);
        Assert.DoesNotContain("private-detail", response.Message);
        Assert.Contains("troubleshooting", response.Message);
        Assert.All(Logger.ReceivedCalls(), call =>
            Assert.DoesNotContain("private-detail", string.Join(" ", call.GetArguments().Select(value => value?.ToString()))));
    }

    private void ConfigurePage(ItemListResponse page) =>
        Service.ListItemsAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(page);
}
