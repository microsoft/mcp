// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.Docs.Commands.BestPractices;
using Fabric.Mcp.Tools.Docs.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Fabric.Mcp.Tools.Docs.Tests.Commands;

public class GetExamplesCommandTests : CommandUnitTestsBase<GetExamplesCommand, IFabricPublicApiService>
{
    [Fact]
    public void GetExamplesCommand_HasCorrectProperties()
    {
        Assert.Equal("api-examples", Command.Name);
        Assert.NotEmpty(Command.Description);
        Assert.Equal("API Examples", Command.Title);
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.ReadOnly);
    }

    [Fact]
    public void GetExamplesCommand_GetCommand_ReturnsValidCommand()
    {
        Assert.NotNull(CommandDefinition);
        Assert.Equal("api-examples", CommandDefinition.Name);
    }

    [Fact]
    public async Task GetExamplesCommand_ExecuteAsync_WithValidItemTypeType_ReturnsExamples()
    {
        // Arrange
        var expectedExamples = new Dictionary<string, string>
        {
            { "example1.json", "content1" },
            { "example2.json", "content2" }
        };

        Service.GetExamplesAsync("notebook", Arg.Any<CancellationToken>()).Returns(expectedExamples);

        // Act
        var result = await ExecuteCommandAsync("--item-type", "notebook");

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Results);
        await Service.Received(1).GetExamplesAsync("notebook", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetExamplesCommand_ExecuteAsync_WithEmptyItemTypeType_ReturnsBadRequest()
    {
        // Arrange & Act
        var result = await ExecuteCommandAsync([]);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
        Assert.Contains("Missing Required options: --itemType-type", result.Message);
        await Service.DidNotReceive().GetExamplesAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetExamplesCommand_ExecuteAsync_WithServiceException_ReturnsInternalServerError()
    {
        // Arrange
        Service.GetExamplesAsync("notebook", Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("Service error"));

        // Act
        var result = await ExecuteCommandAsync("--item-type", "notebook");

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, result.Status);
        Assert.NotEmpty(result.Message);
    }
}
