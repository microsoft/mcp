// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Commands;

public sealed class ItemUpdateCommandTests() : CommandUnitTestsBase<ItemUpdateCommand, IFabricCoreService>
{
    [Fact]
    public void Metadata_DescribesAnIdempotentRemoteMutation()
    {
        Assert.Equal("update-item", Command.Name);
        Assert.Equal("Update Fabric Item", Command.Title);
        Assert.Equal(ToolOperationPlane.Control, Command.Metadata.OperationPlane);
        Assert.True(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.False(Command.Metadata.Secret);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.Same(CoreJsonContext.Default.ItemUpdateCommandResult, Command.ResultTypeInfo);
        Assert.Throws<ArgumentNullException>(() => new ItemUpdateCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new ItemUpdateCommand(Logger, null!));
    }

    [Theory]
    [InlineData(null, ItemUpdateTestData.ItemId)]
    [InlineData(ItemUpdateTestData.WorkspaceId, null)]
    [InlineData("", ItemUpdateTestData.ItemId)]
    [InlineData(ItemUpdateTestData.WorkspaceId, "")]
    [InlineData("workspace-name", ItemUpdateTestData.ItemId)]
    [InlineData(ItemUpdateTestData.WorkspaceId, "not-a-uuid")]
    [InlineData("../other?token=value", ItemUpdateTestData.ItemId)]
    [InlineData(ItemUpdateTestData.WorkspaceId, "https://example.com")]
    [InlineData("00000000-0000-0000-0000-000000000000", ItemUpdateTestData.ItemId)]
    [InlineData(ItemUpdateTestData.WorkspaceId, "00000000-0000-0000-0000-000000000000")]
    public async Task ExecuteAsync_RejectsInvalidIdentifiersBeforeService(string? workspaceId, string? itemId)
    {
        List<string> args = ["--description", "Updated description"];
        if (workspaceId is not null)
        {
            args.AddRange(["--workspace-id", workspaceId]);
        }
        if (itemId is not null)
        {
            args.AddRange(["--item-id", itemId]);
        }

        var response = await ExecuteCommandAsync([.. args]);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_RequiresAtLeastOneUpdate()
    {
        var response = await ExecuteCommandAsync(
            "--workspace-id", ItemUpdateTestData.WorkspaceId, "--item-id", ItemUpdateTestData.ItemId);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("at least one", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--description")]
    [InlineData("--display-name")]
    public async Task ExecuteAsync_RejectsAnOptionWithoutAValue(string option)
    {
        var response = await ExecuteWithUpdateAsync(option);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public async Task ExecuteAsync_RejectsBlankDisplayName(string displayName)
    {
        var response = await ExecuteWithUpdateAsync("--display-name", displayName);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData(0, HttpStatusCode.OK)]
    [InlineData(256, HttpStatusCode.OK)]
    [InlineData(257, HttpStatusCode.BadRequest)]
    public async Task ExecuteAsync_ValidatesDescriptionBoundaries(int length, HttpStatusCode expectedStatus)
    {
        Service.UpdateItemAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<UpdateItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(ItemUpdateTestData.Metadata());

        var response = await ExecuteWithUpdateAsync("--description", new string('d', length));

        Assert.Equal(expectedStatus, response.Status);
        await Service.Received(expectedStatus == HttpStatusCode.OK ? 1 : 0).UpdateItemAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<UpdateItemRequest>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("New name", null)]
    [InlineData(null, "New description")]
    [InlineData(null, "")]
    [InlineData(null, " \t\r\n ")]
    [InlineData("New name", "")]
    [InlineData("New name", "New description")]
    public async Task ExecuteAsync_PreservesSuppliedAndOmittedProperties(string? displayName, string? description)
    {
        Service.UpdateItemAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<UpdateItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(ItemUpdateTestData.Metadata(description));
        List<string> args = [];
        if (displayName is not null)
        {
            args.AddRange(["--display-name", displayName]);
        }
        if (description is not null)
        {
            args.AddRange(["--description", description]);
        }

        var response = await ExecuteWithUpdateAsync([.. args]);

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.ItemUpdateCommandResult);
        Assert.Equal(ItemUpdateTestData.ItemId, result.Item.Id);
        Assert.Equal(description, result.Item.Description);
        await Service.Received(1).UpdateItemAsync(
            ItemUpdateTestData.WorkspaceId,
            ItemUpdateTestData.ItemId,
            Arg.Is<UpdateItemRequest>(r => r.DisplayName == displayName && r.Description == description),
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotInventAnItemTypeNameLimit()
    {
        Service.UpdateItemAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<UpdateItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(ItemUpdateTestData.Metadata());

        var response = await ExecuteWithUpdateAsync("--display-name", new string('n', 257));

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Theory]
    [InlineData("--definition")]
    [InlineData("--item-type")]
    [InlineData("--tags")]
    [InlineData("--workspace")]
    [InlineData("--default-identity")]
    public async Task ExecuteAsync_RejectsUnsupportedOptions(string option)
    {
        var response = await ExecuteWithUpdateAsync("--display-name", "New name", option, "{}");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "rejected")]
    [InlineData(HttpStatusCode.Unauthorized, "Authentication")]
    [InlineData(HttpStatusCode.Forbidden, "read and write")]
    [InlineData(HttpStatusCode.NotFound, "not found")]
    [InlineData(HttpStatusCode.Conflict, "conflicts")]
    [InlineData(HttpStatusCode.TooManyRequests, "throttled")]
    [InlineData(HttpStatusCode.InternalServerError, "500")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "503")]
    public async Task ExecuteAsync_PreservesStatusAndSanitizesErrors(HttpStatusCode status, string guidance)
    {
        Service.UpdateItemAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<UpdateItemRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException(ItemUpdateTestData.PrivateDetails, null, status));

        var response = await ExecuteWithUpdateAsync("--description", ItemUpdateTestData.PrivateDetails);

        Assert.Equal(status, response.Status);
        Assert.Contains(guidance, response.Message);
        AssertSanitized(response);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(7)]
    public async Task ExecuteAsync_NotFoundHasOneSeparatorBeforeMitigation(int? retryAfterSeconds)
    {
        Service.UpdateItemAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<UpdateItemRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ItemUpdateRequestException(HttpStatusCode.NotFound, retryAfterSeconds));

        var response = await ExecuteWithUpdateAsync("--description", "Updated");

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        var retryGuidance = retryAfterSeconds is null ? "" : " Wait at least 7 seconds before another request.";
        Assert.Equal(
            "The Fabric workspace or item was not found, or the caller does not have access." + retryGuidance +
            " To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.",
            response.Message);
        AssertSanitized(response);
    }

    [Fact]
    public async Task ExecuteAsync_SanitizesAuthenticationFailure()
    {
        Service.UpdateItemAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<UpdateItemRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new AuthenticationFailedException(ItemUpdateTestData.PrivateDetails));

        var response = await ExecuteWithUpdateAsync("--description", "Updated");

        Assert.Equal(HttpStatusCode.Unauthorized, response.Status);
        AssertSanitized(response);
    }

    [Theory]
    [InlineData("network", HttpStatusCode.ServiceUnavailable)]
    [InlineData("timeout", HttpStatusCode.GatewayTimeout)]
    [InlineData("invalid-response", HttpStatusCode.BadGateway)]
    [InlineData("unexpected", HttpStatusCode.InternalServerError)]
    public async Task ExecuteAsync_SanitizesNonHttpFailures(string failure, HttpStatusCode expectedStatus)
    {
        Exception exception = failure switch
        {
            "network" => new HttpRequestException(ItemUpdateTestData.PrivateDetails),
            "timeout" => new TaskCanceledException(ItemUpdateTestData.PrivateDetails),
            "invalid-response" => new InvalidDataException(ItemUpdateTestData.PrivateDetails),
            _ => new InvalidOperationException(ItemUpdateTestData.PrivateDetails)
        };
        Service.UpdateItemAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<UpdateItemRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

        var response = await ExecuteWithUpdateAsync("--description", "Updated");

        Assert.Equal(expectedStatus, response.Status);
        AssertSanitized(response);
    }

    [Fact]
    public async Task ExecuteAsync_PropagatesCallerCancellation()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellation.Cancel();
        Service.UpdateItemAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<UpdateItemRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException(cancellation.Token));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Command.ExecuteAsync(
            Context,
            new() { WorkspaceId = ItemUpdateTestData.WorkspaceId, ItemId = ItemUpdateTestData.ItemId, Description = "" },
            cancellation.Token));
    }

    private Task<CommandResponse> ExecuteWithUpdateAsync(params string[] args) =>
        ExecuteCommandAsync(["--workspace-id", ItemUpdateTestData.WorkspaceId, "--item-id", ItemUpdateTestData.ItemId, .. args]);

    private void AssertSanitized(CommandResponse response)
    {
        Assert.DoesNotContain(ItemUpdateTestData.PrivateDetails, response.Message);
        Assert.Null(response.Results);
        foreach (var call in Logger.ReceivedCalls())
        {
            Assert.All(call.GetArguments(), argument =>
            {
                Assert.False(argument is Exception);
                Assert.DoesNotContain(ItemUpdateTestData.PrivateDetails, argument?.ToString() ?? "");
            });
        }
    }
}
