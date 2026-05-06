// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.Docs.Commands.BestPractices;
using Fabric.Mcp.Tools.Docs.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Fabric.Mcp.Tools.Docs.UnitTests.Commands;

public class GetBestPracticesCommandTests : CommandUnitTestsBase<GetBestPracticesCommand, IFabricPublicApiService>
{
    [Fact]
    public void GetBestPracticesCommand_HasCorrectProperties()
    {
        Assert.Equal("best-practices", Command.Name);
        Assert.NotEmpty(Command.Description);
        Assert.Equal("Best Practices", Command.Title);
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.ReadOnly);
    }

    [Fact]
    public void GetBestPracticesCommand_GetCommand_ReturnsValidCommand()
    {
        Assert.Equal("best-practices", CommandDefinition.Name);
    }

    [Fact]
    public async Task GetBestPracticesCommand_ExecuteAsync_WithValidTopic_ReturnsBestPractices()
    {
        // Arrange
        var expectedPractices = new[] { "practice1", "practice2" };

        Service.GetTopicBestPractices("pagination").Returns(expectedPractices);

        // Act
        var result = await ExecuteCommandAsync("--topic", "pagination");

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Results);
        Service.Received(1).GetTopicBestPractices("pagination");
    }

    [Fact]
    public async Task GetBestPracticesCommand_ExecuteAsync_WithEmptyTopic_ReturnsBadRequest()
    {
        // Arrange & Act
        var result = await ExecuteCommandAsync([]);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
        Assert.Equal("Missing Required options: --topic", result.Message);
        Service.DidNotReceive().GetTopicBestPractices(Arg.Any<string>());
    }

    [Fact]
    public async Task GetBestPracticesCommand_ExecuteAsync_WithInvalidTopic_ReturnsNotFound()
    {
        // Arrange
        Service.GetTopicBestPractices("invalid-topic").Throws(new ArgumentException("Topic not found"));

        // Act
        var result = await ExecuteCommandAsync("--topic", "invalid-topic");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, result.Status);
        Assert.Contains("No best practice resources found for invalid-topic", result.Message);
    }

    [Fact]
    public async Task GetBestPracticesCommand_ExecuteAsync_WithServiceException_ReturnsInternalServerError()
    {
        // Arrange
        Service.GetTopicBestPractices("pagination").Throws(new InvalidOperationException("Service error"));

        // Act
        var result = await ExecuteCommandAsync("--topic", "pagination");

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, result.Status);
        Assert.NotEmpty(result.Message);
    }
}
