// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.Docs.Commands.PublicApis;
using Fabric.Mcp.Tools.Docs.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Fabric.Mcp.Tools.Docs.UnitTests.Commands;

public class ListWorkloadCommandTests : CommandUnitTestsBase<ListWorkloadsCommand, IFabricPublicApiService>
{
    [Fact]
    public void ListWorkloadsCommand_HasCorrectProperties()
    {
        Assert.Equal("workloads", Command.Name);
        Assert.NotEmpty(Command.Description);
        Assert.Equal("Available Fabric Workloads", Command.Title);
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.ReadOnly);
    }

    [Fact]
    public void ListWorkloadsCommand_GetCommand_ReturnsValidCommand()
    {
        Assert.Equal("workloads", CommandDefinition.Name);
    }

    [Fact]
    public async Task ListWorkloadsCommand_ExecuteAsync_ReturnsWorkloads()
    {
        // Arrange
        var expectedWorkloads = new[] { "notebook", "report", "platform" };

        Service.ListWorkloadsAsync(Arg.Any<CancellationToken>()).Returns(expectedWorkloads);

        // Act
        var result = await ExecuteCommandAsync([]);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Results);
        await Service.Received(1).ListWorkloadsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListWorkloadsCommand_ExecuteAsync_HandlesException()
    {
        // Arrange
        Service.ListWorkloadsAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("Service error"));

        // Act
        var result = await ExecuteCommandAsync([]);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, result.Status);
        Assert.NotEmpty(result.Message);
    }
}
