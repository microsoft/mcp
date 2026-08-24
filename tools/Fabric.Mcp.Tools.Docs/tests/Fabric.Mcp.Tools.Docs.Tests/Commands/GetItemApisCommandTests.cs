// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.Docs.Commands.PublicApis;
using Fabric.Mcp.Tools.Docs.Models;
using Fabric.Mcp.Tools.Docs.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Fabric.Mcp.Tools.Docs.Tests.Commands;

public class GetItemApisCommandTests : CommandUnitTestsBase<GetItemApisCommand, IFabricPublicApiService>
{
    [Fact]
    public void GetApiSpecCommand_HasCorrectProperties()
    {
        Assert.Equal("item-api-spec", Command.Name);
        Assert.NotEmpty(Command.Description);
        Assert.Equal("Item API Specification", Command.Title);
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.ReadOnly);
    }

    [Fact]
    public void GetApiSpecCommand_GetCommand_ReturnsValidCommand()
    {
        Assert.Equal("item-api-spec", CommandDefinition.Name);
    }

    [Fact]
    public async Task GetApiSpecCommand_ExecuteAsync_WithValidItemType_ReturnsApis()
    {
        // Arrange
        var expectedApi = new FabricPublicApi("api-spec", new Dictionary<string, string> { { "model1", "definition1" } });

        Service.GetPublicApis("notebook", Arg.Any<CancellationToken>()).Returns(expectedApi);

        // Act
        var result = await ExecuteCommandAsync("--item-type", "notebook");

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Results);
        await Service.Received(1).GetPublicApis("notebook", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetApiSpecCommand_ExecuteAsync_WithEmptyItemType_ReturnsBadRequest()
    {
        // Arrange & Act
        var result = await ExecuteCommandAsync([]);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
        Assert.Contains("Missing Required options: --item-type", result.Message);
        await Service.DidNotReceive().GetPublicApis(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetApiSpecCommand_ExecuteAsync_WithCommonItemType_ReturnsNotFound()
    {
        // Arrange & Act
        var result = await ExecuteCommandAsync("--item-type", "common");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, result.Status);
        Assert.Contains("No item type 'common' exists", result.Message);
        Assert.Contains("Did you mean 'platform'?", result.Message);
        await Service.DidNotReceive().GetPublicApis(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetApiSpecCommand_ExecuteAsync_WithHttpNotFoundError_ReturnsNotFound()
    {
        // Arrange
        var httpException = new HttpRequestException("Not found", null, HttpStatusCode.NotFound);
        Service.GetPublicApis("invalid-item-type", Arg.Any<CancellationToken>()).ThrowsAsync(httpException);

        // Act
        var result = await ExecuteCommandAsync("--item-type", "invalid-item-type");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, result.Status);
        Assert.Contains("No item type 'invalid-item-type' exists", result.Message);
        Assert.Contains("list-item-types", result.Message);
    }

    [Fact]
    public async Task GetApiSpecCommand_ExecuteAsync_WithHttpError_ReturnsMappedStatusCode()
    {
        // Arrange
        var httpException = new HttpRequestException("Service unavailable", null, HttpStatusCode.ServiceUnavailable);
        Service.GetPublicApis("notebook", Arg.Any<CancellationToken>()).ThrowsAsync(httpException);

        // Act
        var result = await ExecuteCommandAsync("--item-type", "notebook");

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, result.Status);
        Assert.Equal("Service unavailable", result.Message);
    }

    [Fact]
    public async Task GetApiSpecCommand_ExecuteAsync_WithGeneralException_ReturnsInternalServerError()
    {
        // Arrange
        Service.GetPublicApis("notebook", Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("Service error"));

        // Act
        var result = await ExecuteCommandAsync("--item-type", "notebook");

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, result.Status);
        Assert.NotEmpty(result.Message);
    }
}
