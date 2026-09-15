// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.FunctionApp.Commands;
using Azure.Mcp.Tools.FunctionApp.Commands.FunctionApp;
using Azure.Mcp.Tools.FunctionApp.Models;
using Azure.Mcp.Tools.FunctionApp.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.FunctionApp.Tests.FunctionApp;

public sealed class FunctionAppCreateCommandTests : SubscriptionCommandUnitTestsBase<FunctionAppCreateCommand, IFunctionAppService>
{
    private const string RequiredArgs = "--subscription sub --resource-group rg --function-app myapp --location eastus";

    private static readonly FunctionAppInfo s_createdFunctionApp =
        new("myapp", "rg", "eastus", "myapp-plan", "Running", "myapp.azurewebsites.net", "windows", null);

    private void SetupCreate(FunctionAppInfo result) => Service
        .CreateFunctionApp(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
        .Returns(result);

    private void SetupCreateThrows(Exception exception) => Service
        .CreateFunctionApp(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
        .ThrowsAsync(exception);

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("create", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Theory]
    [InlineData(RequiredArgs, true)]
    [InlineData("--subscription sub --resource-group rg --function-app myapp", false)]
    [InlineData("--subscription sub --resource-group rg --location eastus", false)]
    [InlineData("--subscription sub --function-app myapp --location eastus", false)]
    [InlineData("--resource-group rg --function-app myapp --location eastus", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            SetupCreate(s_createdFunctionApp);
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
    public async Task ExecuteAsync_ReturnsCreatedFunctionApp()
    {
        // Arrange
        SetupCreate(s_createdFunctionApp);

        // Act
        var response = await ExecuteCommandAsync(RequiredArgs);

        // Assert
        var result = ValidateAndDeserializeResponse(response, FunctionAppJsonContext.Default.FunctionAppCreateCommandResult);

        Assert.Equal(s_createdFunctionApp.Name, result.FunctionApp.Name);
        Assert.Equal(s_createdFunctionApp.ResourceGroupName, result.FunctionApp.ResourceGroupName);
        Assert.Equal(s_createdFunctionApp.Location, result.FunctionApp.Location);
        Assert.Equal(s_createdFunctionApp.AppServicePlanName, result.FunctionApp.AppServicePlanName);
        Assert.Equal(s_createdFunctionApp.Status, result.FunctionApp.Status);
        Assert.Equal(s_createdFunctionApp.DefaultHostName, result.FunctionApp.DefaultHostName);
        Assert.Equal(s_createdFunctionApp.OperatingSystem, result.FunctionApp.OperatingSystem);
    }

    [Fact]
    public async Task ExecuteAsync_PassesAllOptionsToService()
    {
        // Arrange
        SetupCreate(s_createdFunctionApp);

        // Act
        var response = await ExecuteCommandAsync(
            $"{RequiredArgs} --app-service-plan existing-plan --plan-type premium --plan-sku EP2 --runtime node --runtime-version 22 " +
            "--os linux --storage-account mystorage --storage-auth-mode connection-string --tenant tenant1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateFunctionApp(
            "sub", "rg", "myapp", "eastus",
            "existing-plan", "premium", "EP2", "node", "22", "linux", "mystorage", "connection-string", "tenant1",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_PassesNullForOmittedOptionalOptions()
    {
        // Arrange
        SetupCreate(s_createdFunctionApp);

        // Act
        var response = await ExecuteCommandAsync(RequiredArgs);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateFunctionApp(
            "sub", "rg", "myapp", "eastus",
            null, null, null, null, null, null, null, null, null,
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("a")]
    [InlineData("this-function-app-name-is-way-too-long-for-azure-to-accept")]
    public async Task ExecuteAsync_RejectsInvalidFunctionAppNameLength(string functionAppName)
    {
        // Act
        var response = await ExecuteCommandAsync($"--subscription sub --resource-group rg --function-app {functionAppName} --location eastus");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("between 2 and 43 characters", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("containerapp")]
    [InlineData("containerapps")]
    public async Task ExecuteAsync_RejectsContainerAppPlanType(string planType)
    {
        // Act
        var response = await ExecuteCommandAsync($"{RequiredArgs} --plan-type {planType}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("functionapp containerapp create", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        SetupCreateThrows(new Exception("Create error"));

        // Act
        var response = await ExecuteCommandAsync(RequiredArgs);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Create error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_MapsServiceValidationErrorsToBadRequest()
    {
        // Arrange
        SetupCreateThrows(new ArgumentException("Runtime 'ruby' is not supported."));

        // Act
        var response = await ExecuteCommandAsync($"{RequiredArgs} --runtime ruby");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("not supported", response.Message);
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict, "already exists")]
    [InlineData(HttpStatusCode.NotFound, "not found")]
    [InlineData(HttpStatusCode.Forbidden, "Authorization failed")]
    public async Task ExecuteAsync_MapsRequestFailedExceptionsToFriendlyMessages(HttpStatusCode status, string expectedMessage)
    {
        // Arrange
        SetupCreateThrows(new RequestFailedException((int)status, "service error"));

        // Act
        var response = await ExecuteCommandAsync(RequiredArgs);

        // Assert
        Assert.Equal(status, response.Status);
        Assert.Contains(expectedMessage, response.Message);
    }
}
