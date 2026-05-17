// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.ContainerApps.Commands;
using Azure.Mcp.Tools.ContainerApps.Commands.ContainerApp;
using Azure.Mcp.Tools.ContainerApps.Models;
using Azure.Mcp.Tools.ContainerApps.Services;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Helpers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ContainerApps.UnitTests.ContainerApp;

public class ContainerAppListCommandTests : CommandUnitTestsBase<ContainerAppListCommand, IContainerAppsService>
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
        var originalSubscriptionId = EnvironmentHelpers.GetAzureSubscriptionId();
        try
        {
            // Ensure environment variable fallback does not interfere with validation tests
            TestEnvironment.ClearAzureSubscriptionId();
            // Arrange
            if (shouldSucceed)
            {
                Service.ListContainerApps(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                    .Returns(new ResourceQueryResults<ContainerAppInfo>(
                    [
                        new("app1", "eastus", "rg1", "/subscriptions/sub/resourceGroups/rg1/providers/Microsoft.App/managedEnvironments/env1", "Succeeded"),
                        new("app2", "eastus2", "rg2", "/subscriptions/sub/resourceGroups/rg2/providers/Microsoft.App/managedEnvironments/env2", "Succeeded")
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
        finally
        {
            if (originalSubscriptionId != null)
            {
                TestEnvironment.SetAzureSubscriptionId(originalSubscriptionId);
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.ListContainerApps(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_FiltersByResourceGroup_ReturnsFilteredContainerApps()
    {
        // Arrange
        var expectedApps = new ResourceQueryResults<ContainerAppInfo>([new("app1", null, null, null, null)], false);
        Service.ListContainerApps("sub", "rg", Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedApps);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub", "--resource-group", "rg");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        await Service.Received(1).ListContainerApps("sub", "rg", Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_EmptyList_ReturnsEmptyResults()
    {
        // Arrange
        Service.ListContainerApps("sub", null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<ContainerAppInfo>([], false));

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub");

        // Assert
        var result = ValidateAndDeserializeResponse(response, ContainerAppsJsonContext.Default.ContainerAppListCommandResult);
        Assert.Empty(result.ContainerApps);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsExpectedContainerAppProperties()
    {
        // Arrange
        var containerApp = new ContainerAppInfo("myapp", "eastus", "myrg", "/subscriptions/sub/resourceGroups/myrg/providers/Microsoft.App/managedEnvironments/myenv", "Succeeded");
        Service.ListContainerApps("sub", null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<ContainerAppInfo>([containerApp], false));

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }
}
