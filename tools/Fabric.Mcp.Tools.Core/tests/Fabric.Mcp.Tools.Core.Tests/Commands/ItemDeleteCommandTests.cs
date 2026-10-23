// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Commands;

public class ItemDeleteCommandTests() : CommandUnitTestsBase<ItemDeleteCommand, IFabricCoreService>
{
    [Fact]
    public void RegisteredCommand_ExposesDestructiveMetadataAndOptionalValueTakingBoolean()
    {
        RegisterCommand();

        Assert.Equal("delete-item", Command.Name);
        Assert.Equal("Delete Fabric Item", Command.Title);
        Assert.Equal(ToolOperationPlane.Control, Command.Metadata.OperationPlane);
        Assert.True(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.False(Command.Metadata.Secret);
        Assert.Same(CoreJsonContext.Default.ItemDeleteCommandResult, Command.ResultTypeInfo);

        var hardDelete = Assert.IsType<Option<bool?>>(Assert.Single(CommandDefinition.Options, option => option.Name == "--hard-delete"));
        Assert.False(hardDelete.Required);
        Assert.False(hardDelete.HasDefaultValue);
        Assert.Equal(ArgumentArity.ExactlyOne, hardDelete.Arity);
        Assert.Null(Command.BindOptions(CommandDefinition.Parse(ItemDeleteTestData.RequiredArguments)).HardDelete);
    }

    [Theory]
    [InlineData("", null)]
    [InlineData("--hard-delete false", false)]
    [InlineData("--hard-delete true", true)]
    [InlineData("--hard-delete=true", true)]
    [InlineData("--hard-delete=False", false)]
    public async Task ExecuteAsync_PreservesRequestedModeAndReturnsOnlyConfirmation(string arguments, bool? hardDelete)
    {
        RegisterCommand();
        Service.DeleteItemAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var response = await ExecuteCommandAsync($"{ItemDeleteTestData.RequiredArguments} {arguments}");

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.ItemDeleteCommandResult);
        Assert.Equal(Guid.Parse(ItemDeleteTestData.WorkspaceId), result.WorkspaceId);
        Assert.Equal(Guid.Parse(ItemDeleteTestData.ItemId), result.ItemId);
        Assert.Equal(hardDelete is true, result.HardDeleteRequested);
        await Service.Received(1).DeleteItemAsync(ItemDeleteTestData.WorkspaceId, ItemDeleteTestData.ItemId, hardDelete,
            TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("--hard-delete")]
    [InlineData("--hard-delete \"\"")]
    [InlineData("--hard-delete= ")]
    [InlineData("--hard-delete yes")]
    [InlineData("--hard-delete 1")]
    [InlineData("--hard-delete false true")]
    [InlineData("--hard-delete false --hard-delete true")]
    [InlineData("--hard-delete true --hard-delete false")]
    [InlineData("--hard-delete true --hard-delete true")]
    [InlineData("--hard-delete --hard-delete true")]
    [InlineData("--hard-delete true --hard-delete")]
    public async Task ExecuteAsync_RejectsValuelessInvalidAndRepeatedHardDeleteBeforeService(string arguments)
    {
        RegisterCommand();

        var response = await ExecuteCommandAsync($"{ItemDeleteTestData.RequiredArguments} {arguments}");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Null(response.Results);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("", ItemDeleteTestData.ItemId)]
    [InlineData("not-a-uuid", ItemDeleteTestData.ItemId)]
    [InlineData("00000000-0000-0000-0000-000000000000", ItemDeleteTestData.ItemId)]
    [InlineData(ItemDeleteTestData.WorkspaceId, "")]
    [InlineData(ItemDeleteTestData.WorkspaceId, "../items")]
    [InlineData(ItemDeleteTestData.WorkspaceId, "00000000-0000-0000-0000-000000000000")]
    [InlineData(ItemDeleteTestData.WorkspaceId, ItemDeleteTestData.ItemId + "?hardDelete=true")]
    public async Task ExecuteAsync_RejectsInvalidIdsBeforeService(string workspaceId, string itemId)
    {
        RegisterCommand();

        var response = await ExecuteCommandAsync("--workspace-id", workspaceId, "--item-id", itemId, "--hard-delete", "true");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Null(response.Results);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("")]
    [InlineData("--workspace-id " + ItemDeleteTestData.WorkspaceId)]
    [InlineData("--item-id " + ItemDeleteTestData.ItemId)]
    public async Task ExecuteAsync_RequiresBothIds(string arguments)
    {
        RegisterCommand();

        var response = await ExecuteCommandAsync(arguments);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task ExecuteAsync_PreservesStatusAndSanitizesFailures(HttpStatusCode status)
    {
        RegisterCommand();
        Service.DeleteItemAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("private-backend-detail", null, status));

        var response = await ExecuteCommandAsync(ItemDeleteTestData.RequiredArguments);

        Assert.Equal(status, response.Status);
        Assert.Null(response.Results);
        Assert.DoesNotContain("private-", response.Message);
        await Service.Received(1).DeleteItemAsync(
            ItemDeleteTestData.WorkspaceId, ItemDeleteTestData.ItemId, null, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(false, "nonempty workspace and item UUIDs")]
    [InlineData(true, "item-type soft-deletion support, and tenant settings")]
    public async Task ExecuteAsync_DistinguishesInvalidInputFromFabricRejection(bool upstream, string guidance)
    {
        RegisterCommand();
        Exception exception = upstream
            ? new HttpRequestException("private-body", null, HttpStatusCode.BadRequest)
            : new ArgumentException("private-input");
        Service.DeleteItemAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

        var response = await ExecuteCommandAsync(ItemDeleteTestData.RequiredArguments + " --hard-delete false");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(guidance, response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
        if (upstream)
        {
            Assert.DoesNotContain("Provide nonempty", response.Message);
            Assert.Contains("No fallback to permanent deletion was attempted", response.Message);
        }
        await Service.Received(1).DeleteItemAsync(
            ItemDeleteTestData.WorkspaceId, ItemDeleteTestData.ItemId, false, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("authentication", HttpStatusCode.Unauthorized)]
    [InlineData("cancellation", HttpStatusCode.RequestTimeout)]
    [InlineData("timeout", HttpStatusCode.GatewayTimeout)]
    [InlineData("unexpected", HttpStatusCode.InternalServerError)]
    public async Task ExecuteAsync_SanitizesOtherFailures(string failure, HttpStatusCode status)
    {
        RegisterCommand();
        Exception exception = failure switch
        {
            "authentication" => new AuthenticationFailedException("private-credential-detail"),
            "cancellation" => new OperationCanceledException("private-cancellation-detail", TestContext.Current.CancellationToken),
            "timeout" => new TimeoutException("private-timeout-detail"),
            _ => new Exception("private-exception-detail")
        };
        Service.DeleteItemAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

        var response = await ExecuteCommandAsync(ItemDeleteTestData.RequiredArguments);

        Assert.Equal(status, response.Status);
        Assert.Null(response.Results);
        Assert.DoesNotContain("private-", response.Message);
    }

    private void RegisterCommand()
    {
        new FabricCoreSetup().ConfigureServices(Services);
        Services.AddSingleton(Service);
    }
}
