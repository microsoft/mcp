// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.AzureMigrate.Commands;
using Azure.Mcp.Tools.AzureMigrate.Commands.PlatformLandingZone;
using Azure.Mcp.Tools.AzureMigrate.Models;
using Azure.Mcp.Tools.AzureMigrate.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Tests.Client;
using NSubstitute.ExceptionExtensions;

namespace Azure.Mcp.Tools.AzureMigrate.UnitTests.PlatformLandingZone;

public class RequestCommandTests : CommandUnitTestsBase<RequestCommand, IPlatformLandingZoneService>
{
    public RequestCommandTests() : base(serviceCollection =>
    {
        serviceCollection.AddSingleton(Substitute.For<ISubscriptionService>());
        serviceCollection.AddSingleton(Substitute.For<ITenantService>());
    })
    {
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("request", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("update", command.Description);
        Assert.Contains("generate", command.Description);
        Assert.Contains("download", command.Description);
        Assert.Contains("status", command.Description);
    }

    [Theory]
    [InlineData("--action update --subscription sub123 --resource-group rg1 --migrate-project-name project1 --region-type multi", true)]
    [InlineData("--action download --subscription sub123 --resource-group rg1 --migrate-project-name project1", true)]
    [InlineData("--action generate --subscription sub123 --resource-group rg1 --migrate-project-name project1", true)]
    [InlineData("--action status --subscription sub123 --resource-group rg1 --migrate-project-name project1", true)]
    [InlineData("--subscription sub123 --resource-group rg1 --migrate-project-name project1", false)] // Missing action
    [InlineData("--action update --resource-group rg1 --migrate-project-name project1", false)] // Missing subscription
    [InlineData("--action update --subscription sub123 --migrate-project-name project1", false)] // Missing resource group
    [InlineData("--action update --subscription sub123 --resource-group rg1", false)] // Missing migrate project name
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var parameters = new PlatformLandingZoneParameters
            {
                RegionType = "multi",
                FireWallType = "azurefirewall",
                NetworkArchitecture = "hubspoke",
                IdentitySubscriptionId = "id-sub",
                ManagementSubscriptionId = "mgmt-sub",
                ConnectivitySubscriptionId = "conn-sub",
                Regions = "eastus,westus",
                EnvironmentName = "prod",
                VersionControlSystem = "github",
                OrganizationName = "myorg",
                CachedAt = DateTime.UtcNow
            };

            Service.UpdateParametersAsync(
                Arg.Any<PlatformLandingZoneContext>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
                .Returns(parameters);

            Service.DownloadAsync(
                Arg.Any<PlatformLandingZoneContext>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
                .Returns("/path/to/downloaded/file.zip");

            Service.GenerateAsync(
                Arg.Any<PlatformLandingZoneContext>(),
                Arg.Any<CancellationToken>())
                .Returns("https://download.url/file.zip");

            Service.GetParameterStatus(Arg.Any<PlatformLandingZoneContext>())
                .Returns("Status message");

            Service.GetMissingParameters(Arg.Any<PlatformLandingZoneContext>())
                .Returns(new List<string>());
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.True(response.Status == HttpStatusCode.BadRequest || response.Status == HttpStatusCode.InternalServerError);
        }
    }

    [Fact]
    public async Task ExecuteAsync_UpdateAction_UpdatesParameters()
    {
        // Arrange
        var subscription = "sub123";
        var resourceGroup = "rg1";
        var projectName = "project1";
        var regionType = "multi";
        var fireWallType = "azurefirewall";

        var updatedParameters = new PlatformLandingZoneParameters
        {
            RegionType = regionType,
            FireWallType = fireWallType,
            NetworkArchitecture = "hubspoke",
            IdentitySubscriptionId = subscription,
            ManagementSubscriptionId = subscription,
            ConnectivitySubscriptionId = subscription,
            Regions = "eastus",
            EnvironmentName = "prod",
            VersionControlSystem = "local",
            OrganizationName = "contoso",
            CachedAt = DateTime.UtcNow
        };

        Service.UpdateParametersAsync(
            Arg.Is<PlatformLandingZoneContext>(ctx =>
                ctx.SubscriptionId == subscription &&
                ctx.ResourceGroupName == resourceGroup &&
                ctx.MigrateProjectName == projectName),
            Arg.Is(regionType),
            Arg.Is(fireWallType),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(updatedParameters);

        Service.GetMissingParameters(Arg.Any<PlatformLandingZoneContext>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync(
            "--action", "update",
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--migrate-project-name", projectName,
            "--region-type", regionType,
            "--firewall-type", fireWallType);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var result = ConvertResponse(response, AzureMigrateJsonContext.Default.RequestCommandResult);

        Assert.NotNull(result);
        Assert.Contains("Parameters updated successfully", result.Message);
        Assert.Contains("Complete: True", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DownloadAction_DownloadsFiles()
    {
        // Arrange
        var subscription = "sub123";
        var resourceGroup = "rg1";
        var projectName = "project1";
        var downloadedPath = "/path/to/downloaded/file.zip";

        Service.DownloadAsync(
            Arg.Is<PlatformLandingZoneContext>(ctx =>
                ctx.SubscriptionId == subscription &&
                ctx.ResourceGroupName == resourceGroup &&
                ctx.MigrateProjectName == projectName),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(downloadedPath);

        // Act
        var response = await ExecuteCommandAsync(
            "--action", "download",
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--migrate-project-name", projectName);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureMigrateJsonContext.Default.RequestCommandResult);

        Assert.NotNull(result);
        Assert.Contains("downloaded successfully", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_GenerateAction_GeneratesLandingZone()
    {
        // Arrange
        var subscription = "sub123";
        var resourceGroup = "rg1";
        var projectName = "project1";
        var downloadUrl = "https://download.url/landingzone.zip";

        // Mock that all parameters are provided
        Service.GetMissingParameters(Arg.Any<PlatformLandingZoneContext>())
            .Returns([]);

        Service.GenerateAsync(
            Arg.Is<PlatformLandingZoneContext>(ctx =>
                ctx.SubscriptionId == subscription &&
                ctx.ResourceGroupName == resourceGroup &&
                ctx.MigrateProjectName == projectName),
            Arg.Any<CancellationToken>())
            .Returns(downloadUrl);

        // Act
        var response = await ExecuteCommandAsync(
            "--action", "generate",
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--migrate-project-name", projectName);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var result = ConvertResponse(response, AzureMigrateJsonContext.Default.RequestCommandResult);

        Assert.NotNull(result);
        Assert.Contains("Platform Landing zone generated successfully", result.Message);
        Assert.Contains(downloadUrl, result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_GenerateAction_WithDefaultParameters_Succeeds()
    {
        // Arrange
        var subscription = "sub123";
        var resourceGroup = "rg1";
        var projectName = "project1";

        // Mock that defaults are applied (no missing parameters)
        Service.GetMissingParameters(Arg.Any<PlatformLandingZoneContext>())
            .Returns([]);

        Service.GenerateAsync(
            Arg.Any<PlatformLandingZoneContext>(),
            Arg.Any<CancellationToken>())
            .Returns("https://download.url/landingzone.zip");

        // Act
        var response = await ExecuteCommandAsync(
            "--action", "generate",
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--migrate-project-name", projectName);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var result = ConvertResponse(response, AzureMigrateJsonContext.Default.RequestCommandResult);

        Assert.NotNull(result);
        Assert.Contains("Platform Landing zone generated successfully", result.Message);

        // Verify GenerateAsync was called
        await Service.Received(1).GenerateAsync(
            Arg.Any<PlatformLandingZoneContext>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_GenerateAction_HandlesTimeout()
    {
        // Arrange
        var subscription = "sub123";
        var resourceGroup = "rg1";
        var projectName = "project1";

        // Mock that all parameters are provided
        Service.GetMissingParameters(Arg.Any<PlatformLandingZoneContext>())
            .Returns([]);

        Service.GenerateAsync(
            Arg.Any<PlatformLandingZoneContext>(),
            Arg.Any<CancellationToken>())
            .Returns((string?)null);

        // Act
        var response = await ExecuteCommandAsync(
            "--action", "generate",
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--migrate-project-name", projectName);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var result = ConvertResponse(response, AzureMigrateJsonContext.Default.RequestCommandResult);

        Assert.NotNull(result);
        Assert.Contains("in progress", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_StatusAction_ReturnsParameterStatus()
    {
        // Arrange
        var subscription = "sub123";
        var resourceGroup = "rg1";
        var projectName = "project1";
        var statusMessage = "Parameters for sub123:rg1:project1:\n  Cached at: 2025-12-10\n  Complete: True";

        Service.GetParameterStatus(
            Arg.Is<PlatformLandingZoneContext>(ctx =>
                ctx.SubscriptionId == subscription &&
                ctx.ResourceGroupName == resourceGroup &&
                ctx.MigrateProjectName == projectName))
            .Returns(statusMessage);

        // Act
        var response = await ExecuteCommandAsync(
            "--action", "status",
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--migrate-project-name", projectName);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var result = ConvertResponse(response, AzureMigrateJsonContext.Default.RequestCommandResult);

        Assert.NotNull(result);
        Assert.Equal(statusMessage, result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidAction_ReturnsError()
    {
        // Arrange
        var subscription = "sub123";
        var resourceGroup = "rg1";
        var projectName = "project1";

        // Act
        var response = await ExecuteCommandAsync(
            "--action", "invalid-action",
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--migrate-project-name", projectName);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Status == HttpStatusCode.BadRequest || response.Status == HttpStatusCode.InternalServerError);
        Assert.Contains("Invalid action", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var subscription = "sub123";
        var resourceGroup = "rg1";
        var projectName = "project1";

        Service.DownloadAsync(
            Arg.Any<PlatformLandingZoneContext>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--action", "download",
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--migrate-project-name", projectName);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Service error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesHttpRequestException()
    {
        // Arrange
        var subscription = "sub123";
        var resourceGroup = "rg1";
        var projectName = "project1";

        Service.GetMissingParameters(Arg.Any<PlatformLandingZoneContext>())
            .Returns([]);

        Service.GenerateAsync(
            Arg.Any<PlatformLandingZoneContext>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("HTTP request failed"));

        // Act
        var response = await ExecuteCommandAsync(
            "--action", "generate",
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--migrate-project-name", projectName);

        // Assert
        Assert.True(response.Status == HttpStatusCode.BadGateway || response.Status == HttpStatusCode.ServiceUnavailable);
        Assert.Contains("HTTP request failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesInvalidOperationException()
    {
        // Arrange
        var subscription = "sub123";
        var resourceGroup = "rg1";
        var projectName = "project1";

        Service.GetMissingParameters(Arg.Any<PlatformLandingZoneContext>())
            .Returns([]);

        Service.GenerateAsync(
            Arg.Any<PlatformLandingZoneContext>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Missing required parameters"));

        // Act
        var response = await ExecuteCommandAsync(
            "--action", "generate",
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--migrate-project-name", projectName);

        // Assert
        Assert.True(response.Status == HttpStatusCode.BadRequest || response.Status == HttpStatusCode.InternalServerError);
        Assert.Contains("Missing required parameters", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesArgumentException()
    {
        // Arrange
        var subscription = "sub123";
        var resourceGroup = "rg1";
        var projectName = "project1";

        Service.UpdateParametersAsync(
            Arg.Any<PlatformLandingZoneContext>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException("regionType must be 'single' or 'multi'"));

        // Act
        var response = await ExecuteCommandAsync(
            "--action", "update",
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--migrate-project-name", projectName,
            "--region-type", "invalid");

        // Assert
        Assert.True(response.Status == HttpStatusCode.BadRequest || response.Status == HttpStatusCode.InternalServerError);
        Assert.Contains("regionType must be 'single' or 'multi'", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_UpdateAction_WithAllParameters_ReturnsComplete()
    {
        // Arrange
        var subscription = "sub123";
        var resourceGroup = "rg1";
        var projectName = "project1";

        var completeParameters = new PlatformLandingZoneParameters
        {
            RegionType = "multi",
            FireWallType = "azurefirewall",
            NetworkArchitecture = "hubspoke",
            IdentitySubscriptionId = "id-sub-123",
            ManagementSubscriptionId = "mgmt-sub-456",
            ConnectivitySubscriptionId = "conn-sub-789",
            Regions = "eastus,westus",
            EnvironmentName = "prod",
            VersionControlSystem = "github",
            OrganizationName = "myorg",
            CachedAt = DateTime.UtcNow
        };

        Service.UpdateParametersAsync(
            Arg.Any<PlatformLandingZoneContext>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(completeParameters);

        Service.GetMissingParameters(Arg.Any<PlatformLandingZoneContext>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync(
            "--action", "update",
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--migrate-project-name", projectName,
            "--region-type", "multi",
            "--firewall-type", "azurefirewall",
            "--network-architecture", "hubspoke",
            "--identity-subscription-id", "id-sub-123",
            "--management-subscription-id", "mgmt-sub-456",
            "--connectivity-subscription-id", "conn-sub-789",
            "--regions", "eastus,westus",
            "--environment-name", "prod",
            "--version-control-system", "github",
            "--organization-name", "myorg");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var result = ConvertResponse(response, AzureMigrateJsonContext.Default.RequestCommandResult);

        Assert.NotNull(result);
        Assert.Contains("Complete: True", result.Message);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange & Act
        var args = CommandDefinition.Parse([
            "--action", "update",
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--migrate-project-name", "project1",
            "--region-type", "multi",
            "--firewall-type", "azurefirewall",
            "--network-architecture", "hubspoke"
        ]);

        // Assert
        Assert.Empty(args.Errors);
    }
}
