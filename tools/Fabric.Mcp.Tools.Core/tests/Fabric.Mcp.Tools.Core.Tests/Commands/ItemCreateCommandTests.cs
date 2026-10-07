// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Commands;

public class ItemCreateCommandTests : CommandUnitTestsBase<ItemCreateCommand, IFabricCoreService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("create-item", Command.Name);
        Assert.Equal("Create Fabric Item", Command.Title);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.Idempotent);
        Assert.NotNull(Command.Description);
        Assert.NotEmpty(Command.Description);
    }

    [Fact]
    public void GetCommand_ReturnsValidCommand()
    {
        Assert.Equal("create-item", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
    }

    [Fact]
    public void CommandOptions_ContainsRequiredOptions()
    {
        Assert.NotEmpty(CommandDefinition.Options);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ItemCreateCommand(null!, Service));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenFabricCoreServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ItemCreateCommand(Logger, null!));
    }

    [Fact]
    public void Metadata_HasCorrectProperties()
    {
        var metadata = Command.Metadata;

        Assert.False(metadata.Destructive);
        Assert.False(metadata.Idempotent);
        Assert.False(metadata.LocalRequired);
        Assert.False(metadata.OpenWorld);
        Assert.False(metadata.ReadOnly);
        Assert.False(metadata.Secret);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsItem_WhenCreationSucceeds()
    {
        Service.CreateItemAsync(Arg.Any<string>(), Arg.Any<CreateItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(new FabricItem { Id = "item-id", DisplayName = "Sales", Type = "Lakehouse", WorkspaceId = "workspace-id" });

        var response = await ExecuteCommandAsync("--workspace-id", "workspace-id", "--display-name", "Sales", "--item-type", "Lakehouse");

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.ItemCreateCommandResult);
        Assert.Equal("item-id", result.Item.Id);
        Assert.Equal("Sales", result.Item.DisplayName);
        Assert.Equal("Lakehouse", result.Item.Type);
        Assert.Equal("workspace-id", result.Item.WorkspaceId);
        await Service.Received(1).CreateItemAsync("workspace-id", Arg.Any<CreateItemRequest>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "item type, display name, and description")]
    [InlineData(HttpStatusCode.Unauthorized, "configured Fabric identity")]
    [InlineData(HttpStatusCode.Forbidden, "supported and enabled for the tenant and capacity")]
    [InlineData(HttpStatusCode.NotFound, "workspace was not found")]
    [InlineData(HttpStatusCode.Conflict, "Check for an existing item before retrying")]
    [InlineData(HttpStatusCode.TooManyRequests, "Retry the request later")]
    [InlineData(HttpStatusCode.InternalServerError, "HTTP 500")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "HTTP 503")]
    public async Task ExecuteAsync_PreservesHttpStatusAndProvidesGuidance(HttpStatusCode status, string guidance)
    {
        Service.CreateItemAsync(Arg.Any<string>(), Arg.Any<CreateItemRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("upstream-error", null, status));

        var response = await ExecuteCommandAsync("--workspace-id", "workspace-id", "--display-name", "Sales", "--item-type", "Lakehouse");

        Assert.Equal(status, response.Status);
        Assert.Contains(guidance, response.Message);
        Assert.Contains("upstream-error", response.Message);
        Assert.DoesNotContain("Service unavailable or network connectivity issues", response.Message);
        Assert.NotNull(response.Results);
        await Service.Received(1).CreateItemAsync("workspace-id", Arg.Any<CreateItemRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_PreservesNetworkFallback_WhenHttpStatusIsMissing()
    {
        Service.CreateItemAsync(Arg.Any<string>(), Arg.Any<CreateItemRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("network-failure"));

        var response = await ExecuteCommandAsync("--workspace-id", "workspace-id", "--display-name", "Sales", "--item-type", "Lakehouse");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.Status);
        Assert.StartsWith("Service unavailable or network connectivity issues. Details: network-failure", response.Message);
        await Service.Received(1).CreateItemAsync("workspace-id", Arg.Any<CreateItemRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_PreservesNonHttpErrorMapping()
    {
        Service.CreateItemAsync(Arg.Any<string>(), Arg.Any<CreateItemRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("configuration-error"));

        var response = await ExecuteCommandAsync("--workspace-id", "workspace-id", "--display-name", "Sales", "--item-type", "Lakehouse");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.Status);
        Assert.StartsWith("configuration-error.", response.Message);
        await Service.Received(1).CreateItemAsync("workspace-id", Arg.Any<CreateItemRequest>(), Arg.Any<CancellationToken>());
    }
}
