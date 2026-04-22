// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AppService.Commands;
using Azure.Mcp.Tools.AppService.Commands.Webapp.Diagnostic;
using Azure.Mcp.Tools.AppService.Models;
using Azure.Mcp.Tools.AppService.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AppService.UnitTests.Commands.Webapp.Diagnostic;

[Trait("Command", "DetectorList")]
public class DetectorListCommandTests : CommandUnitTestsBase<DetectorListCommand, IAppServiceService>
{
    [Fact]
    public async Task ExecuteAsync_WithValidParameters_CallsServiceWithCorrectArguments()
    {
        List<DetectorDetails> expectedValue = [new("name", "type", "description", "category", ["analysisType1", "analysisType2"])];
        // Arrange
        // Set up the mock to return success for any arguments
        Service.ListDetectorsAsync("sub123", "rg1", "test-app", Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expectedValue);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--app", "test-app");

        // Assert
        // Verify that the mock was called with the expected parameters
        await Service.Received(1).ListDetectorsAsync("sub123", "rg1", "test-app", Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());

        var result = ValidateAndDeserializeResponse(response, AppServiceJsonContext.Default.DetectorListResult);

        Assert.Single(result.Detectors);
        Assert.Equal(expectedValue[0].Name, result.Detectors[0].Name);
        Assert.Equal(expectedValue[0].Type, result.Detectors[0].Type);
        Assert.Equal(expectedValue[0].Description, result.Detectors[0].Description);
        Assert.Equal(expectedValue[0].Category, result.Detectors[0].Category);
        Assert.Equal(expectedValue[0].AnalysisTypes, result.Detectors[0].AnalysisTypes);
    }

    [Theory]
    [InlineData()] // Missing all parameters
    [InlineData("--subscription", "sub123")] // Missing resource group and app name,
    [InlineData("--resource-group", "rg1")] // Missing subscription and app name
    [InlineData("--app", "app")] // Missing subscription and resource group
    [InlineData("--subscription", "sub123", "--resource-group", "rg1")] // Missing app name
    [InlineData("--subscription", "sub123", "--app", "test-app")] // Missing resource group
    [InlineData("--resource-group", "rg1", "--app", "test-app")] // Missing subscription
    public async Task ExecuteAsync_MissingRequiredParameter_ReturnsErrorResponse(params string[] commandArgs)
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(commandArgs);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);

        await Service.DidNotReceive().ListDetectorsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsException_ReturnsErrorResponse()
    {
        // Arrange
        // Set up the mock to return success for any arguments
        Service.ListDetectorsAsync("sub123", "rg1", "test-app", Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Service error"));

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--app", "test-app");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);

        await Service.Received(1).ListDetectorsAsync("sub123", "rg1", "test-app",
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }
}
