// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.Docs.Commands;
using Fabric.Mcp.Tools.Docs.Commands.BestPractices;
using Fabric.Mcp.Tools.Docs.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Fabric.Mcp.Tools.Docs.Tests.Commands;

public class GetItemDefinitionCommandTests : CommandUnitTestsBase<GetItemDefinitionCommand, IFabricPublicApiService>
{
    [Fact]
    public void GetItemDefinitionCommand_HasCorrectProperties()
    {
        Assert.Equal("item-definitions", Command.Name);
        Assert.NotEmpty(Command.Description);
        Assert.Equal("Item Definitions", Command.Title);
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.ReadOnly);
    }

    [Fact]
    public void GetItemDefinitionCommand_GetCommand_ReturnsValidCommand()
    {
        Assert.Equal("item-definitions", CommandDefinition.Name);
    }

    [Fact]
    public async Task GetItemDefinitionCommand_ExecuteAsync_WithValidItemType_ReturnsDefinition()
    {
        // Arrange
        var expectedDefinition = "{ \"schema\": \"definition\" }";

        Service.GetItemDefinition("notebook").Returns(expectedDefinition);

        // Act
        var result = await ExecuteCommandAsync("--item-type", "notebook");

        // Assert
        var response = ValidateAndDeserializeResponse(
            result,
            FabricJsonContext.Default.GetItemDefinitionCommandResult);
        Assert.Equal(expectedDefinition, response.Definition);
        Service.Received(1).GetItemDefinition("notebook");
    }

    [Fact]
    public async Task GetItemDefinitionCommand_ExecuteAsync_WithEmptyItemType_ReturnsBadRequest()
    {
        // Arrange & Act
        var result = await ExecuteCommandAsync([]);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
        Assert.Contains("Missing Required options: --item-type", result.Message);
        Service.DidNotReceive().GetItemDefinition(Arg.Any<string>());
    }

    [Fact]
    public async Task GetItemDefinitionCommand_ExecuteAsync_WithInvalidItemType_ReturnsNotFound()
    {
        // Arrange
        Service.GetItemDefinition("invalid-item-type").Throws(new ArgumentException("Item type not found"));

        // Act
        var result = await ExecuteCommandAsync("--item-type", "invalid-item-type");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, result.Status);
        Assert.Contains("No item definition found for item type invalid-item-type", result.Message);
    }

    [Fact]
    public async Task GetItemDefinitionCommand_ExecuteAsync_WithServiceException_ReturnsInternalServerError()
    {
        // Arrange
        Service.GetItemDefinition("notebook").Throws(new InvalidOperationException("Service error"));

        // Act
        var result = await ExecuteCommandAsync("--item-type", "notebook");

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, result.Status);
        Assert.NotEmpty(result.Message);
    }
}
