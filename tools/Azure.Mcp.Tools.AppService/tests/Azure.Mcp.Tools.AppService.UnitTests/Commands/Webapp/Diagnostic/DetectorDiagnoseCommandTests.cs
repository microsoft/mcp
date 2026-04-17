// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AppService.Commands;
using Azure.Mcp.Tools.AppService.Commands.Webapp.Diagnostic;
using Azure.Mcp.Tools.AppService.Models;
using Azure.Mcp.Tools.AppService.Services;
using Azure.ResourceManager.AppService.Models;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AppService.UnitTests.Commands.Webapp.Diagnostic;

[Trait("Command", "DetectorDiagnose")]
public class DetectorDiagnoseCommandTests : CommandUnitTestsBase<DetectorDiagnoseCommand, IAppServiceService>
{
    [Theory]
    [InlineData(null, null, null)]
    [InlineData(null, null, "PT1H")]
    [InlineData("2023-01-01T00:00:00Z", null, null)]
    [InlineData(null, "2023-01-02T00:00:00Z", null)]
    [InlineData("2023-01-01T00:00:00Z", "2023-01-02T00:00:00Z", null)]
    [InlineData("2023-01-01T00:00:00Z", "2023-01-02T00:00:00Z", "PT1H")]
    public async Task ExecuteAsync_WithValidParameters_CallsServiceWithCorrectArguments(string? startDateTimeString, string? endDateTimeString, string? interval)
    {
        var dataset = new DiagnosticDataset()
        {
            Table = new DataTableResponseObject(),
            RenderingProperties = new DiagnosticDataRendering()
        };
        var expectedValue = new DiagnosisResults([dataset], new DetectorDetails("name", "type", "description", "category", ["analysisType1", "analysisType2"]));

        var startTime = startDateTimeString != null ? DateTimeOffset.Parse(startDateTimeString).ToUniversalTime() : (DateTimeOffset?)null;
        var endTime = endDateTimeString != null ? DateTimeOffset.Parse(endDateTimeString).ToUniversalTime() : (DateTimeOffset?)null;

        // Arrange
        // Set up the mock to return success for any arguments
        Service.DiagnoseDetectorAsync("sub123", "rg1", "test-app", "detector-name", startTime, endTime,
            interval, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expectedValue);

        List<string> unparsedArgs = [
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--app", "test-app",
            "--detector-name", "detector-name"
        ];
        if (startDateTimeString != null)
        {
            unparsedArgs.AddRange(["--start-time", startDateTimeString]);
        }
        if (endDateTimeString != null)
        {
            unparsedArgs.AddRange(["--end-time", endDateTimeString]);
        }
        if (interval != null)
        {
            unparsedArgs.AddRange(["--interval", interval]);
        }

        // Act
        var response = await ExecuteCommandAsync(unparsedArgs.ToArray());

        // Assert
        // Verify that the mock was called with the expected parameters
        await Service.Received(1).DiagnoseDetectorAsync("sub123", "rg1", "test-app", "detector-name",
            startTime, endTime, interval, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());

        var result = ValidateAndDeserializeResponse(response, AppServiceJsonContext.Default.DetectorDiagnoseResult);

        Assert.NotNull(result);
        Assert.Single(result.Diagnoses.Datasets);
        Assert.NotNull(result.Diagnoses.Datasets[0]);
        Assert.Equal(expectedValue.Detector.Name, result.Diagnoses.Detector.Name);
        Assert.Equal(expectedValue.Detector.Type, result.Diagnoses.Detector.Type);
        Assert.Equal(expectedValue.Detector.Description, result.Diagnoses.Detector.Description);
        Assert.Equal(expectedValue.Detector.Category, result.Diagnoses.Detector.Category);
        Assert.Equal(expectedValue.Detector.AnalysisTypes, result.Diagnoses.Detector.AnalysisTypes);
    }

    [Theory]
    [InlineData()] // Missing all parameters
    [InlineData("--subscription", "sub123")] // Missing resource group, app name, and detector name
    [InlineData("--resource-group", "rg1")] // Missing subscription, app name, and detector name
    [InlineData("--app", "app")] // Missing subscription, resource group, and detector name
    [InlineData("--detector-name", "detector")] // Missing subscription, resource group, and app name
    [InlineData("--subscription", "sub123", "--resource-group", "rg1")] // Missing app name and detector name
    [InlineData("--subscription", "sub123", "--app", "test-app")] // Missing resource group and detector name
    [InlineData("--subscription", "sub123", "--detector-name", "detector-name")] // Missing resource group and app name
    [InlineData("--resource-group", "rg1", "--app", "test-app")] // Missing subscription and detector name
    [InlineData("--resource-group", "rg1", "--detector-name", "detector-name")] // Missing subscription and app name
    [InlineData("--app", "test-app", "--detector-name", "detector-name")] // Missing subscription and resource group
    [InlineData("--subscription", "sub123", "--resource-group", "rg1", "--app", "test-app")] // Missing detector name
    [InlineData("--subscription", "sub123", "--resource-group", "rg1", "--detector-name", "detector-name")] // Missing app name
    [InlineData("--subscription", "sub123", "--app", "test-app", "--detector-name", "detector-name")] // Missing resource group
    [InlineData("--resource-group", "rg1", "--app", "test-app", "--detector-name", "detector-name")] // Missing subscription
    public async Task ExecuteAsync_MissingRequiredParameter_ReturnsErrorResponse(params string[] commandArgs)
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(commandArgs);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);

        await Service.DidNotReceive().DiagnoseDetectorAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<DateTimeOffset?>(),
            Arg.Any<DateTimeOffset?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(null, null, null)]
    [InlineData(null, null, "PT1H")]
    [InlineData("2023-01-01T00:00:00Z", null, null)]
    [InlineData(null, "2023-01-02T00:00:00Z", null)]
    [InlineData("2023-01-01T00:00:00Z", "2023-01-02T00:00:00Z", null)]
    [InlineData("2023-01-01T00:00:00Z", "2023-01-02T00:00:00Z", "PT1H")]
    public async Task ExecuteAsync_ServiceThrowsException_ReturnsErrorResponse(string? startDateTimeString, string? endDateTimeString, string? interval)
    {
        var startTime = startDateTimeString != null ? DateTimeOffset.Parse(startDateTimeString).ToUniversalTime() : (DateTimeOffset?)null;
        var endTime = endDateTimeString != null ? DateTimeOffset.Parse(endDateTimeString).ToUniversalTime() : (DateTimeOffset?)null;

        // Arrange
        // Set up the mock to return success for any arguments
        Service.DiagnoseDetectorAsync("sub123", "rg1", "test-app", "detector-name", startTime, endTime,
            interval, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Service error"));

        List<string> unparsedArgs = [
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--app", "test-app",
            "--detector-name", "detector-name"
        ];
        if (startDateTimeString != null)
        {
            unparsedArgs.AddRange(["--start-time", startDateTimeString]);
        }
        if (endDateTimeString != null)
        {
            unparsedArgs.AddRange(["--end-time", endDateTimeString]);
        }
        if (interval != null)
        {
            unparsedArgs.AddRange(["--interval", interval]);
        }

        // Act
        var response = await ExecuteCommandAsync(unparsedArgs.ToArray());

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);

        await Service.Received(1).DiagnoseDetectorAsync("sub123", "rg1", "test-app", "detector-name",
            startTime, endTime, interval, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }
}
