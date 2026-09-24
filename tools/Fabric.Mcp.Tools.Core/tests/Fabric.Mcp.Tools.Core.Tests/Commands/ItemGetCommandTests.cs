using System.Net;
using System.Text.Json;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Commands;

public class ItemGetCommandTests : CommandUnitTestsBase<ItemGetCommand, IFabricCoreService>
{
    private const string WorkspaceId = "aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa";
    private const string ItemId = "bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb";

    [Fact]
    public void Constructor_InitializesReadOnlyCommand()
    {
        Assert.Equal("get-item", Command.Name);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.False(Command.Metadata.Secret);
        Assert.Same(CoreJsonContext.Default.ItemGetCommandResult, Command.ResultTypeInfo);
        Assert.Equal(["--workspace-id", "--item-id"], CommandDefinition.Options.Where(option => option.Required).Select(option => option.Name));
    }

    [Fact]
    public void Constructor_RejectsNullDependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new ItemGetCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new ItemGetCommand(Logger, null!));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsItemMetadata()
    {
        Service.GetItemAsync(WorkspaceId, ItemId, Arg.Any<CancellationToken>())
            .Returns(new FabricItemMetadata
            {
                Id = Guid.Parse(ItemId),
                WorkspaceId = Guid.Parse(WorkspaceId),
                DisplayName = "Sales Lakehouse",
                Description = "Test metadata",
                Type = "Lakehouse"
            });

        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--item-id", ItemId);

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.ItemGetCommandResult);
        Assert.Equal(Guid.Parse(ItemId), result.Item.Id);
        Assert.Equal(Guid.Parse(WorkspaceId), result.Item.WorkspaceId);
        Assert.Equal("Sales Lakehouse", result.Item.DisplayName);
        Assert.Equal("Test metadata", result.Item.Description);
        Assert.Equal("Lakehouse", result.Item.Type);
        await Service.Received(1).GetItemAsync(WorkspaceId, ItemId, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("not-a-guid", ItemId)]
    [InlineData(WorkspaceId, "not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000", ItemId)]
    [InlineData(WorkspaceId, "00000000-0000-0000-0000-000000000000")]
    [InlineData("../workspaces", ItemId)]
    [InlineData(WorkspaceId, "?include=DefaultIdentity")]
    [InlineData("", ItemId)]
    [InlineData(WorkspaceId, " ")]
    [InlineData("https://example.com", ItemId)]
    [InlineData(WorkspaceId, ItemId + "/getDefinition")]
    public async Task ExecuteAsync_RejectsInvalidIdsBeforeCallingService(string workspaceId, string itemId)
    {
        var response = await ExecuteCommandAsync("--workspace-id", workspaceId, "--item-id", itemId);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("")]
    [InlineData("--workspace-id " + WorkspaceId)]
    [InlineData("--item-id " + ItemId)]
    public async Task ExecuteAsync_RejectsMissingIdsBeforeCallingService(string arguments)
    {
        var response = await ExecuteCommandAsync(arguments);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task ExecuteAsync_PreservesHttpFailureStatusWithoutExposingBackendText(HttpStatusCode statusCode)
    {
        Service.GetItemAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<FabricItemMetadata>(new HttpRequestException("private-backend-detail", null, statusCode)));

        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--item-id", ItemId);

        Assert.Equal(statusCode, response.Status);
        Assert.DoesNotContain("private-backend-detail", response.Message);
        Assert.Null(response.Results);
    }

    [Theory]
    [InlineData("authentication", HttpStatusCode.Unauthorized)]
    [InlineData("cancellation", HttpStatusCode.RequestTimeout)]
    [InlineData("json", HttpStatusCode.BadGateway)]
    [InlineData("network", HttpStatusCode.ServiceUnavailable)]
    [InlineData("unexpected", HttpStatusCode.InternalServerError)]
    public async Task ExecuteAsync_SanitizesFailures(string failure, HttpStatusCode expectedStatus)
    {
        Exception exception = failure switch
        {
            "authentication" => new AuthenticationFailedException("private-backend-detail"),
            "cancellation" => new OperationCanceledException("private-backend-detail", TestContext.Current.CancellationToken),
            "json" => new JsonException("private-backend-detail"),
            "network" => new HttpRequestException("private-backend-detail"),
            _ => new Exception("private-backend-detail")
        };
        Service.GetItemAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<FabricItemMetadata>(exception));

        var response = await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--item-id", ItemId);

        Assert.Equal(expectedStatus, response.Status);
        Assert.DoesNotContain("private-backend-detail", response.Message);
        Assert.Null(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsCancellationToken()
    {
        Service.GetItemAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FabricItemMetadata
            {
                Id = Guid.Parse(ItemId),
                WorkspaceId = Guid.Parse(WorkspaceId),
                DisplayName = "Sales",
                Type = "Lakehouse"
            });

        await ExecuteCommandAsync("--workspace-id", WorkspaceId, "--item-id", ItemId);

        await Service.Received(1).GetItemAsync(WorkspaceId, ItemId, TestContext.Current.CancellationToken);
    }
}
