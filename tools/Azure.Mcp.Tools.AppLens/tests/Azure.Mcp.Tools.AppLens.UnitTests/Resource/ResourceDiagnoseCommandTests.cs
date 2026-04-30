// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AppLens.Commands.Resource;
using Azure.Mcp.Tools.AppLens.Models;
using Azure.Mcp.Tools.AppLens.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AppLens.UnitTests.Resource;

public class ResourceDiagnoseCommandTests : CommandUnitTestsBase<ResourceDiagnoseCommand, IAppLensService>
{
    [Fact]
    public async Task ExecuteAsync_ReturnsDiagnosticResult_WhenAllParametersProvided()
    {
        // Arrange
        var expectedResult = new DiagnosticResult(
            ["Insight 1", "Insight 2"],
            ["Solution 1", "Solution 2"],
            "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Web/sites/myapp",
            "Microsoft.Web/sites");

        Service.DiagnoseResourceAsync(
            "Why is my app slow?",
            "myapp",
            "sub123",
            "rg1",
            "Microsoft.Web/sites",
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var response = await ExecuteCommandAsync(
            "--question", "Why is my app slow?",
            "--resource", "myapp",
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--resource-type", "Microsoft.Web/sites");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AppLensJsonContext.Default.ResourceDiagnoseCommandResult);

        Assert.NotNull(result.Result);
        Assert.Equal(2, result.Result.Insights.Count);
        Assert.Equal(2, result.Result.Solutions.Count);
        Assert.Equal("Insight 1", result.Result.Insights[0]);
        Assert.Equal("Solution 1", result.Result.Solutions[0]);
        Assert.Equal("/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Web/sites/myapp", result.Result.ResourceId);
        Assert.Equal("Microsoft.Web/sites", result.Result.ResourceType);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsDiagnosticResult_WhenOnlyRequiredParametersProvided()
    {
        // Arrange - only question and resource are required now
        var expectedResult = new DiagnosticResult(
            ["Insight 1"],
            ["Solution 1"],
            "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Web/sites/myapp",
            "Microsoft.Web/sites");

        Service.DiagnoseResourceAsync(
            "Why is my app slow?",
            "myapp",
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var response = await ExecuteCommandAsync(
            "--question", "Why is my app slow?",
            "--resource", "myapp");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenQuestionIsMissing()
    {
        // Arrange && Act
        var response = await ExecuteCommandAsync(
            "--resource", "myapp",
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--resource-type", "Microsoft.Web/sites");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenResourceIsMissing()
    {
        // Arrange && Act
        var response = await ExecuteCommandAsync(
            "--question", "Why is my app slow?",
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--resource-type", "Microsoft.Web/sites");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_SucceedsWithoutSubscription()
    {
        // Arrange - subscription is now optional
        var expectedResult = new DiagnosticResult(
            ["Insight 1"],
            ["Solution 1"],
            "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Web/sites/myapp",
            "Microsoft.Web/sites");

        Service.DiagnoseResourceAsync(
            "Why is my app slow?",
            "myapp",
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var response = await ExecuteCommandAsync(
            "--question", "Why is my app slow?",
            "--resource", "myapp",
            "--resource-group", "rg1",
            "--resource-type", "Microsoft.Web/sites");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_SucceedsWithoutResourceGroup()
    {
        // Arrange - resource group is now optional
        var expectedResult = new DiagnosticResult(
            ["Insight 1"],
            ["Solution 1"],
            "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Web/sites/myapp",
            "Microsoft.Web/sites");

        Service.DiagnoseResourceAsync(
            "Why is my app slow?",
            "myapp",
            "sub123",
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var response = await ExecuteCommandAsync(
            "--question", "Why is my app slow?",
            "--resource", "myapp",
            "--subscription", "sub123",
            "--resource-type", "Microsoft.Web/sites");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_SucceedsWithoutResourceType()
    {
        // Arrange - resource type is now optional
        var expectedResult = new DiagnosticResult(
            ["Insight 1"],
            ["Solution 1"],
            "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Web/sites/myapp",
            "Microsoft.Web/sites");

        Service.DiagnoseResourceAsync(
            "Why is my app slow?",
            "myapp",
            "sub123",
            "rg1",
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var response = await ExecuteCommandAsync(
            "--question", "Why is my app slow?",
            "--resource", "myapp",
            "--subscription", "sub123",
            "--resource-group", "rg1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_WhenServiceThrowsGenericException()
    {
        // Arrange
        Service.DiagnoseResourceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--question", "Why is my app slow?",
            "--resource", "myapp",
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--resource-type", "Microsoft.Web/sites");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Service error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenServiceThrowsInvalidOperationException()
    {
        // Arrange
        Service.DiagnoseResourceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Resource not found"));

        // Act
        var response = await ExecuteCommandAsync(
            "--question", "Why is my app slow?",
            "--resource", "myapp",
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--resource-type", "Microsoft.Web/sites");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Resource not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Returns503_WhenServiceIsUnavailable()
    {
        // Arrange
        Service.DiagnoseResourceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Service Unavailable", null, HttpStatusCode.ServiceUnavailable));

        // Act
        var response = await ExecuteCommandAsync(
            "--question", "Why is my app slow?",
            "--resource", "myapp",
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--resource-type", "Microsoft.Web/sites");

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.Status);
        Assert.Contains("Service Unavailable", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesEmptyDiagnosticResult()
    {
        // Arrange
        var expectedResult = new DiagnosticResult(
            [],
            [],
            "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Web/sites/myapp",
            "Microsoft.Web/sites");

        Service.DiagnoseResourceAsync(
            "Why is my app slow?",
            "myapp",
            "sub123",
            "rg1",
            "Microsoft.Web/sites",
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var response = await ExecuteCommandAsync(
            "--question", "Why is my app slow?",
            "--resource", "myapp",
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--resource-type", "Microsoft.Web/sites");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AppLensJsonContext.Default.ResourceDiagnoseCommandResult);

        Assert.NotNull(result.Result);
        Assert.Empty(result.Result.Insights);
        Assert.Empty(result.Result.Solutions);
    }

    [Theory]
    [InlineData("", "myapp")]
    [InlineData("Why is my app slow?", "")]
    public async Task ExecuteAsync_Returns400_WhenRequiredParameterIsEmpty(string question, string resource)
    {
        // Arrange
        var args = new List<string>();
        if (!string.IsNullOrEmpty(question))
        { args.AddRange(["--question", question]); }
        if (!string.IsNullOrEmpty(resource))
        { args.AddRange(["--resource", resource]); }

        // Act
        var response = await ExecuteCommandAsync(args.ToArray());

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_LogsInformationOnSuccess()
    {
        // Arrange
        var expectedResult = new DiagnosticResult(
            ["Insight 1"],
            ["Solution 1"],
            "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Web/sites/myapp",
            "Microsoft.Web/sites");

        Service.DiagnoseResourceAsync(
            "Why is my app slow?",
            "myapp",
            "sub123",
            "rg1",
            "Microsoft.Web/sites",
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        await ExecuteCommandAsync(
            "--question", "Why is my app slow?",
            "--resource", "myapp",
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--resource-type", "Microsoft.Web/sites");

        // Assert
        Logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Diagnosing resource")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task ExecuteAsync_LogsErrorOnException()
    {
        // Arrange
        Service.DiagnoseResourceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        await ExecuteCommandAsync(
            "--question", "Why is my app slow?",
            "--resource", "myapp",
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--resource-type", "Microsoft.Web/sites");

        // Assert
        Logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Error in diagnose")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Theory]
    [InlineData("microsoft.web/sites", "app", true)]
    [InlineData("microsoft.web/sites", "linux", true)]
    [InlineData("microsoft.web/sites", "functionapp", true)]
    [InlineData("Microsoft.Web/Sites", "App", true)]
    [InlineData("MICROSOFT.WEB/SITES", "APP", true)]
    [InlineData("microsoft.containerservice/managedclusters", "", true)]
    [InlineData("Microsoft.ContainerService/managedClusters", "", true)]
    [InlineData("microsoft.apimanagement/service", "", true)]
    [InlineData("Microsoft.ApiManagement/service", "", true)]
    [InlineData("microsoft.web/sites", "container", false)]
    [InlineData("microsoft.web/sites", "", false)]
    [InlineData("microsoft.compute/virtualmachines", "", false)]
    [InlineData("microsoft.storage/storageaccounts", "", false)]
    [InlineData("microsoft.sql/servers", "", false)]
    public void IsResourceTypeSupported_ReturnsCorrectResult(string resourceType, string resourceKind, bool expected)
    {
        // Act
        var result = AppLensService.IsResourceTypeSupported(resourceType, resourceKind);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void SupportedResourceTypes_ReturnsExpectedTypes()
    {
        // Act
        var types = AppLensService.SupportedResourceTypes().ToList();

        // Assert
        Assert.Equal(3, types.Count);
        Assert.Contains("microsoft.web/sites", types);
        Assert.Contains("microsoft.containerservice/managedclusters", types);
        Assert.Contains("microsoft.apimanagement/service", types);
    }
}
