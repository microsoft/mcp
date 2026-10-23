// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.Docs.Commands.PublicApis;
using Fabric.Mcp.Tools.Docs.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Fabric.Mcp.Tools.Docs.Tests.Commands;

public class ListItemTypesCommandTests : CommandUnitTestsBase<ListItemTypesCommand, IFabricPublicApiService>
{
    [Fact]
    public void ListItemTypesCommand_HasCorrectProperties()
    {
        Assert.Equal("list-item-types", Command.Name);
        Assert.NotEmpty(Command.Description);
        Assert.Equal("Available Fabric Item Types", Command.Title);
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.ReadOnly);
    }

    [Fact]
    public void ListItemTypesCommand_GetCommand_ReturnsValidCommand()
    {
        Assert.Equal("list-item-types", CommandDefinition.Name);
    }

    [Fact]
    public async Task ListItemTypesCommand_ExecuteAsync_ReturnsItemTypes()
    {
        // Arrange
        var expectedItemTypes = new[] { "notebook", "report", "platform" };

        Service.ListItemTypesAsync(Arg.Any<CancellationToken>()).Returns(expectedItemTypes);

        // Act
        var result = await ExecuteCommandAsync([]);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Results);
        await Service.Received(1).ListItemTypesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListItemTypesCommand_ExecuteAsync_HandlesException()
    {
        // Arrange
        Service.ListItemTypesAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("Service error"));

        // Act
        var result = await ExecuteCommandAsync([]);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, result.Status);
        Assert.NotEmpty(result.Message);
    }
}
