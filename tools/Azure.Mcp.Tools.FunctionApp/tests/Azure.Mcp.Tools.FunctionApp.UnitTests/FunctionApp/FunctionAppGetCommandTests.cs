// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.FunctionApp.Commands;
using Azure.Mcp.Tools.FunctionApp.Commands.FunctionApp;
using Azure.Mcp.Tools.FunctionApp.Models;
using Azure.Mcp.Tools.FunctionApp.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.FunctionApp.UnitTests.FunctionApp;

public sealed class FunctionAppGetCommandTests : CommandUnitTestsBase<FunctionAppGetCommand, IFunctionAppService>
{
    [Theory]
    [InlineData("--subscription sub123", true)]
    [InlineData("--subscription sub123 --tenant tenant123", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_Listing_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var testFunctionApps = new List<FunctionAppInfo>
            {
                new("functionApp1", null, "eastus", "plan1", "Running", "functionapp1.azurewebsites.net", null),
                new("functionApp2", null, "westus", "plan2", "Stopped", "functionapp2.azurewebsites.net", null)
            };
            Service.GetFunctionApp(
                Arg.Any<string>(),
                Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
                Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(testFunctionApps);
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsFunctionAppList()
    {
        // Arrange
        var expectedFunctionApps = new List<FunctionAppInfo>
        {
            new("functionApp1", "rg1", "eastus", "plan1", "Running", "functionapp1.azurewebsites.net", null),
            new("functionApp2", "rg2", "westus", "plan2", "Stopped", "functionapp2.azurewebsites.net", null)
        };
        Service.GetFunctionApp(
            Arg.Any<string>(),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedFunctionApps);

        // Act
        var response = await ExecuteCommandAsync("--subscription sub123");

        // Assert
        // Verify the mock was called
        await Service.Received(1).GetFunctionApp(
            Arg.Any<string>(),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());

        var result = ValidateAndDeserializeResponse(response, FunctionAppJsonContext.Default.FunctionAppGetCommandResult);

        Assert.NotNull(result);
        Assert.Equal(expectedFunctionApps.Count, result.FunctionApps.Count);
        Assert.Equal(expectedFunctionApps[0].Name, result.FunctionApps[0].Name);
        Assert.Equal(expectedFunctionApps[0].ResourceGroupName, result.FunctionApps[0].ResourceGroupName);
        Assert.Equal(expectedFunctionApps[0].AppServicePlanName, result.FunctionApps[0].AppServicePlanName);
        Assert.Equal(expectedFunctionApps[0].Location, result.FunctionApps[0].Location);
        Assert.Equal(expectedFunctionApps[0].Status, result.FunctionApps[0].Status);
        Assert.Equal(expectedFunctionApps[0].DefaultHostName, result.FunctionApps[0].DefaultHostName);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyWhenNoFunctionApp()
    {
        // Arrange
        Service.GetFunctionApp(
            Arg.Any<string>(),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync("--subscription sub123");

        // Assert
        var result = ValidateAndDeserializeResponse(response, FunctionAppJsonContext.Default.FunctionAppGetCommandResult);

        Assert.Empty(result.FunctionApps);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.GetFunctionApp(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync("--subscription sub123");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Theory]
    [InlineData("--subscription sub123 --resource-group rg1 --function-app app1", true)]
    [InlineData("--subscription sub123 --function-app app1", false)]
    [InlineData("--resource-group rg1 --function-app app1", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.GetFunctionApp(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns([new("app1", "rg1", "eastus", "plan1", "Running", "app1.azurewebsites.net", null)]);
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsFunctionApp()
    {
        var expected = new FunctionAppInfo("app1", "rg1", "eastus", "plan1", "Running", "app1.azurewebsites.net", null);
        Service.GetFunctionApp(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns([expected]);

        var response = await ExecuteCommandAsync("--subscription sub123 --resource-group rg1 --function-app app1");

        var result = ValidateAndDeserializeResponse(response, FunctionAppJsonContext.Default.FunctionAppGetCommandResult);

        Assert.Equal(expected.Name, result.FunctionApps[0].Name);
        Assert.Equal(expected.ResourceGroupName, result.FunctionApps[0].ResourceGroupName);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyWhenNotFound()
    {
        Service.GetFunctionApp(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns((List<FunctionAppInfo>?)null);

        var response = await ExecuteCommandAsync("--subscription sub123 --resource-group rg1 --function-app app1");

        var result = ValidateAndDeserializeResponse(response, FunctionAppJsonContext.Default.FunctionAppGetCommandResult);
        Assert.Empty(result.FunctionApps);
    }
}
