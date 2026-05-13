// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.Docs.Commands.PublicApis;
using Fabric.Mcp.Tools.Docs.Models;
using Fabric.Mcp.Tools.Docs.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Fabric.Mcp.Tools.Docs.UnitTests.Commands;

public class GetPlatformApisCommandTests : CommandUnitTestsBase<GetPlatformApisCommand, IFabricPublicApiService>
{
    [Fact]
    public void GetPlatformApiSpecCommand_HasCorrectProperties()
    {
        Assert.Equal("platform-api-spec", Command.Name);
        Assert.NotEmpty(Command.Description);
        Assert.Equal("Platform API Specification", Command.Title);
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.ReadOnly);
    }

    [Fact]
    public void GetPlatformApiSpecCommand_GetCommand_ReturnsValidCommand()
    {
        Assert.Equal("platform-api-spec", CommandDefinition.Name);
    }

    [Fact]
    public async Task GetPlatformApiSpecCommand_ExecuteAsync_ReturnsPlatformApis()
    {
        // Arrange
        var expectedApi = new FabricWorkloadPublicApi("api-spec", new Dictionary<string, string> { { "model1", "definition1" } });

        Service.GetWorkloadPublicApis("platform", Arg.Any<CancellationToken>()).Returns(expectedApi);

        // Act
        var result = await ExecuteCommandAsync([]);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Results);
        await Service.Received(1).GetWorkloadPublicApis("platform", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPlatformApiSpecCommand_ExecuteAsync_HandlesException()
    {
        // Arrange
        Service.GetWorkloadPublicApis("platform", Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("Service error"));

        // Act
        var result = await ExecuteCommandAsync([]);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, result.Status);
        Assert.NotEmpty(result.Message);
    }
}
