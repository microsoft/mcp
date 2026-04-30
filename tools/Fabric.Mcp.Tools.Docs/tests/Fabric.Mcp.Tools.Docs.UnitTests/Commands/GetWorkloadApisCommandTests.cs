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

public class GetWorkloadApisCommandTests : CommandUnitTestsBase<GetWorkloadApisCommand, IFabricPublicApiService>
{
    [Fact]
    public void GetApiSpecCommand_HasCorrectProperties()
    {
        Assert.Equal("workload-api-spec", Command.Name);
        Assert.NotEmpty(Command.Description);
        Assert.Equal("Workload API Specification", Command.Title);
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.ReadOnly);
    }

    [Fact]
    public void GetApiSpecCommand_GetCommand_ReturnsValidCommand()
    {
        Assert.Equal("workload-api-spec", CommandDefinition.Name);
    }

    [Fact]
    public async Task GetApiSpecCommand_ExecuteAsync_WithValidWorkloadType_ReturnsApis()
    {
        // Arrange
        var expectedApi = new FabricWorkloadPublicApi("api-spec", new Dictionary<string, string> { { "model1", "definition1" } });

        Service.GetWorkloadPublicApis("notebook", Arg.Any<CancellationToken>()).Returns(expectedApi);

        // Act
        var result = await ExecuteCommandAsync("--workload-type", "notebook");

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Results);
        await Service.Received(1).GetWorkloadPublicApis("notebook", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetApiSpecCommand_ExecuteAsync_WithEmptyWorkloadType_ReturnsBadRequest()
    {
        // Arrange & Act
        var result = await ExecuteCommandAsync([]);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
        Assert.Equal("Missing Required options: --workload-type", result.Message);
        await Service.DidNotReceive().GetWorkloadPublicApis(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetApiSpecCommand_ExecuteAsync_WithCommonWorkloadType_ReturnsNotFound()
    {
        // Arrange & Act
        var result = await ExecuteCommandAsync("--workload-type", "common");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, result.Status);
        Assert.Contains("No workload of type 'common' exists", result.Message);
        Assert.Contains("Did you mean 'platform'?", result.Message);
        await Service.DidNotReceive().GetWorkloadPublicApis(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetApiSpecCommand_ExecuteAsync_WithHttpNotFoundError_ReturnsNotFound()
    {
        // Arrange
        var httpException = new HttpRequestException("Not found", null, HttpStatusCode.NotFound);
        Service.GetWorkloadPublicApis("invalid-workload", Arg.Any<CancellationToken>()).ThrowsAsync(httpException);

        // Act
        var result = await ExecuteCommandAsync("--workload-type", "invalid-workload");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, result.Status);
        Assert.Contains("No workload of type 'invalid-workload' exists", result.Message);
        Assert.Contains("workloads command", result.Message);
    }

    [Fact]
    public async Task GetApiSpecCommand_ExecuteAsync_WithHttpError_ReturnsMappedStatusCode()
    {
        // Arrange
        var httpException = new HttpRequestException("Service unavailable", null, HttpStatusCode.ServiceUnavailable);
        Service.GetWorkloadPublicApis("notebook", Arg.Any<CancellationToken>()).ThrowsAsync(httpException);

        // Act
        var result = await ExecuteCommandAsync("--workload-type", "notebook");

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, result.Status);
        Assert.Equal("Service unavailable", result.Message);
    }

    [Fact]
    public async Task GetApiSpecCommand_ExecuteAsync_WithGeneralException_ReturnsInternalServerError()
    {
        // Arrange
        Service.GetWorkloadPublicApis("notebook", Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("Service error"));

        // Act
        var result = await ExecuteCommandAsync("--workload-type", "notebook");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, result.Status);
        Assert.NotEmpty(result.Message);
    }
}
