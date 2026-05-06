// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.Docs.Commands.BestPractices;
using Fabric.Mcp.Tools.Docs.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Fabric.Mcp.Tools.Docs.UnitTests.Commands;

public class GetWorkloadDefinitionCommandTests : CommandUnitTestsBase<GetWorkloadDefinitionCommand, IFabricPublicApiService>
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
    public async Task GetItemDefinitionCommand_ExecuteAsync_WithValidWorkloadType_ReturnsDefinition()
    {
        // Arrange
        var expectedDefinition = "{ \"schema\": \"definition\" }";

        Service.GetWorkloadItemDefinition("notebook").Returns(expectedDefinition);

        // Act
        var result = await ExecuteCommandAsync("--workload-type", "notebook");

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Results);
        Service.Received(1).GetWorkloadItemDefinition("notebook");
    }

    [Fact]
    public async Task GetItemDefinitionCommand_ExecuteAsync_WithEmptyWorkloadType_ReturnsBadRequest()
    {
        // Arrange & Act
        var result = await ExecuteCommandAsync([]);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
        Assert.Equal("Missing Required options: --workload-type", result.Message);
        Service.DidNotReceive().GetWorkloadItemDefinition(Arg.Any<string>());
    }

    [Fact]
    public async Task GetItemDefinitionCommand_ExecuteAsync_WithInvalidWorkloadType_ReturnsNotFound()
    {
        // Arrange
        Service.GetWorkloadItemDefinition("invalid-workload").Throws(new ArgumentException("Workload not found"));

        // Act
        var result = await ExecuteCommandAsync("--workload-type", "invalid-workload");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, result.Status);
        Assert.Contains("No item definition found for workload invalid-workload", result.Message);
    }

    [Fact]
    public async Task GetItemDefinitionCommand_ExecuteAsync_WithServiceException_ReturnsInternalServerError()
    {
        // Arrange
        Service.GetWorkloadItemDefinition("notebook").Throws(new InvalidOperationException("Service error"));

        // Act
        var result = await ExecuteCommandAsync("--workload-type", "notebook");

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, result.Status);
        Assert.NotEmpty(result.Message);
    }
}
