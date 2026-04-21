// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Acr.Commands;
using Azure.Mcp.Tools.Acr.Commands.Registry;
using Azure.Mcp.Tools.Acr.Models;
using Azure.Mcp.Tools.Acr.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Helpers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Acr.UnitTests.Registry;

public class RegistryListCommandTests : CommandUnitTestsBase<RegistryListCommand, IAcrService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("list", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub", true)]
    [InlineData("--subscription sub --resource-group rg", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Ensure environment variable fallback does not interfere with validation tests
        TestEnvironment.ClearAzureSubscriptionId();
        // Arrange
        if (shouldSucceed)
        {
            Service.ListRegistries(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(new ResourceQueryResults<AcrRegistryInfo>(
                [
                    new("registry1", "eastus", "registry1.azurecr.io", "Basic", "Basic"),
                    new("registry2", "eastus2", "registry2.azurecr.io", "Standard", "Standard")
                ], false));
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.ListRegistries(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_FiltersById_ReturnsFilteredRegistries()
    {
        // Arrange
        var expectedRegistries = new ResourceQueryResults<AcrRegistryInfo>([new("registry1", null, null, null, null)], false);
        Service.ListRegistries("sub", "rg", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedRegistries);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub", "--resource-group", "rg");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        await Service.Received(1).ListRegistries("sub", "rg", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_EmptyList_ReturnsEmptyResults()
    {
        // Arrange
        Service.ListRegistries("sub", null, Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<AcrRegistryInfo>([], false));

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var result = DeserializeResponse(response, AcrJsonContext.Default.RegistryListCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Registries);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsExpectedRegistryProperties()
    {
        // Arrange
        var registry = new AcrRegistryInfo("myregistry", "eastus", "myregistry.azurecr.io", "Basic", "Basic");
        Service.ListRegistries("sub", null, Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<AcrRegistryInfo>([registry], false));

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }
}
