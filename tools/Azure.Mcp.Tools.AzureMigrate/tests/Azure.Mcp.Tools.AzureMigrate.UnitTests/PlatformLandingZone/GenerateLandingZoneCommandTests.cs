// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.AzureMigrate.Commands;
using Azure.Mcp.Tools.AzureMigrate.Commands.PlatformLandingZone;
using Azure.Mcp.Tools.AzureMigrate.Models;
using Azure.Mcp.Tools.AzureMigrate.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureMigrate.UnitTests.PlatformLandingZone;

public class GenerateLandingZoneCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPlatformLandingZoneService _platformLandingZoneService;
    private readonly ILogger<GenerateLandingZoneCommand> _logger;
    private readonly GenerateLandingZoneCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public GenerateLandingZoneCommandTests()
    {
        _platformLandingZoneService = Substitute.For<IPlatformLandingZoneService>();
        _logger = Substitute.For<ILogger<GenerateLandingZoneCommand>>();

        var collection = new ServiceCollection().AddSingleton(_platformLandingZoneService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("generatelandingzone", command.Name);
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

            _platformLandingZoneService.UpdateParametersAsync(
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
                .Returns(Task.FromResult(parameters));

            _platformLandingZoneService.DownloadLandingZoneAsync(
                Arg.Any<PlatformLandingZoneContext>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
                .Returns(Task.FromResult("/path/to/downloaded/file.zip"));

            _platformLandingZoneService.GenerateLandingZoneAsync(
                Arg.Any<PlatformLandingZoneContext>(),
                Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<string?>("https://download.url/file.zip"));

            _platformLandingZoneService.GetParameterStatus(Arg.Any<PlatformLandingZoneContext>())
                .Returns("Status message");

            _platformLandingZoneService.GetMissingParameters(Arg.Any<PlatformLandingZoneContext>())
                .Returns(new List<string>());
        }

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

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
            NetworkArchitecture = null,
            IdentitySubscriptionId = null,
            ManagementSubscriptionId = null,
            ConnectivitySubscriptionId = null,
            Regions = null,
            EnvironmentName = null,
            VersionControlSystem = null,
            CachedAt = DateTime.UtcNow
        };

        _platformLandingZoneService.UpdateParametersAsync(
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
            .Returns(Task.FromResult(updatedParameters));

        _platformLandingZoneService.GetMissingParameters(Arg.Any<PlatformLandingZoneContext>())
            .Returns(new List<string> { "networkArchitecture", "identitySubscriptionId" });

        var args = _commandDefinition.Parse([
            "--action", "update",
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--migrate-project-name", projectName,
            "--region-type", regionType,
            "--firewall-type", fireWallType
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureMigrateJsonContext.Default.GenerateLandingZoneCommandResult);

        Assert.NotNull(result);
        Assert.Contains("Parameters updated successfully", result.Message);
        Assert.Contains("Complete: False", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DownloadAction_DownloadsFiles()
    {
        // Arrange
        var subscription = "sub123";
        var resourceGroup = "rg1";
        var projectName = "project1";
        var downloadedPath = "/path/to/downloaded/file.zip";

        _platformLandingZoneService.DownloadLandingZoneAsync(
            Arg.Is<PlatformLandingZoneContext>(ctx =>
                ctx.SubscriptionId == subscription &&
                ctx.ResourceGroupName == resourceGroup &&
                ctx.MigrateProjectName == projectName),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(downloadedPath));

        var args = _commandDefinition.Parse([
            "--action", "download",
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--migrate-project-name", projectName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureMigrateJsonContext.Default.GenerateLandingZoneCommandResult);

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
        _platformLandingZoneService.GetMissingParameters(Arg.Any<PlatformLandingZoneContext>())
            .Returns(new List<string>());

        _platformLandingZoneService.GenerateLandingZoneAsync(
            Arg.Is<PlatformLandingZoneContext>(ctx =>
                ctx.SubscriptionId == subscription &&
                ctx.ResourceGroupName == resourceGroup &&
                ctx.MigrateProjectName == projectName),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<string?>(downloadUrl));

        var args = _commandDefinition.Parse([
            "--action", "generate",
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--migrate-project-name", projectName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureMigrateJsonContext.Default.GenerateLandingZoneCommandResult);

        Assert.NotNull(result);
        Assert.Contains("Landing zone generated successfully", result.Message);
        Assert.Contains(downloadUrl, result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_GenerateAction_WithMissingParameters_ReturnsGuidanceMessage()
    {
        // Arrange
        var subscription = "sub123";
        var resourceGroup = "rg1";
        var projectName = "project1";

        // Mock that parameters are missing
        _platformLandingZoneService.GetMissingParameters(Arg.Any<PlatformLandingZoneContext>())
            .Returns(new List<string> { "regionType", "fireWallType", "networkArchitecture" });

        var args = _commandDefinition.Parse([
            "--action", "generate",
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--migrate-project-name", projectName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureMigrateJsonContext.Default.GenerateLandingZoneCommandResult);

        Assert.NotNull(result);
        Assert.Contains("Cannot generate landing zone", result.Message);
        Assert.Contains("provide the following required parameters", result.Message);
        Assert.Contains("regionType", result.Message);
        Assert.Contains("fireWallType", result.Message);
        Assert.Contains("networkArchitecture", result.Message);
        Assert.Contains("action='update'", result.Message);

        // Verify GenerateLandingZoneAsync was never called
        await _platformLandingZoneService.DidNotReceive().GenerateLandingZoneAsync(
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
        _platformLandingZoneService.GetMissingParameters(Arg.Any<PlatformLandingZoneContext>())
            .Returns(new List<string>());

        _platformLandingZoneService.GenerateLandingZoneAsync(
            Arg.Any<PlatformLandingZoneContext>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<string?>(null));

        var args = _commandDefinition.Parse([
            "--action", "generate",
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--migrate-project-name", projectName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureMigrateJsonContext.Default.GenerateLandingZoneCommandResult);

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

        _platformLandingZoneService.GetParameterStatus(
            Arg.Is<PlatformLandingZoneContext>(ctx =>
                ctx.SubscriptionId == subscription &&
                ctx.ResourceGroupName == resourceGroup &&
                ctx.MigrateProjectName == projectName))
            .Returns(statusMessage);

        var args = _commandDefinition.Parse([
            "--action", "status",
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--migrate-project-name", projectName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureMigrateJsonContext.Default.GenerateLandingZoneCommandResult);

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

        var args = _commandDefinition.Parse([
            "--action", "invalid-action",
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--migrate-project-name", projectName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid action", response.Message);
        Assert.Contains("update", response.Message);
        Assert.Contains("download", response.Message);
        Assert.Contains("generate", response.Message);
        Assert.Contains("status", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var subscription = "sub123";
        var resourceGroup = "rg1";
        var projectName = "project1";

        _platformLandingZoneService.DownloadLandingZoneAsync(
            Arg.Any<PlatformLandingZoneContext>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Service error"));

        var args = _commandDefinition.Parse([
            "--action", "download",
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--migrate-project-name", projectName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

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

        _platformLandingZoneService.GetMissingParameters(Arg.Any<PlatformLandingZoneContext>())
            .Returns(new List<string>());

        _platformLandingZoneService.GenerateLandingZoneAsync(
            Arg.Any<PlatformLandingZoneContext>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("HTTP request failed"));

        var args = _commandDefinition.Parse([
            "--action", "generate",
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--migrate-project-name", projectName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadGateway, response.Status);
        Assert.Contains("HTTP request failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesInvalidOperationException()
    {
        // Arrange
        var subscription = "sub123";
        var resourceGroup = "rg1";
        var projectName = "project1";

        _platformLandingZoneService.GetMissingParameters(Arg.Any<PlatformLandingZoneContext>())
            .Returns(new List<string>());

        _platformLandingZoneService.GenerateLandingZoneAsync(
            Arg.Any<PlatformLandingZoneContext>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Missing required parameters"));

        var args = _commandDefinition.Parse([
            "--action", "generate",
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--migrate-project-name", projectName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Missing required parameters", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesArgumentException()
    {
        // Arrange
        var subscription = "sub123";
        var resourceGroup = "rg1";
        var projectName = "project1";

        _platformLandingZoneService.UpdateParametersAsync(
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

        var args = _commandDefinition.Parse([
            "--action", "update",
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--migrate-project-name", projectName,
            "--region-type", "invalid"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
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

        _platformLandingZoneService.UpdateParametersAsync(
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
            .Returns(Task.FromResult(completeParameters));

        _platformLandingZoneService.GetMissingParameters(Arg.Any<PlatformLandingZoneContext>())
            .Returns(new List<string>());

        var args = _commandDefinition.Parse([
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
            "--organization-name", "myorg"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureMigrateJsonContext.Default.GenerateLandingZoneCommandResult);

        Assert.NotNull(result);
        Assert.Contains("Complete: True", result.Message);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange & Act
        var args = _commandDefinition.Parse([
            "--action", "update",
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--migrate-project-name", "project1",
            "--region-type", "multi",
            "--firewall-type", "azurefirewall",
            "--network-architecture", "hubspoke"
        ]);

        // Assert - verify command parses correctly
        Assert.Empty(args.Errors);
    }
}
