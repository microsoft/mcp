// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureMigrate.Commands;
using Azure.Mcp.Tools.AzureMigrate.Commands.PlatformLandingZone;
using Azure.Mcp.Tools.AzureMigrate.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Tests.Client;
using NSubstitute.ExceptionExtensions;

namespace Azure.Mcp.Tools.AzureMigrate.UnitTests.PlatformLandingZone;

public class GetGuidanceCommandTests : CommandUnitTestsBase<GetGuidanceCommand, IPlatformLandingZoneGuidanceService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("getguidance", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("scenario", command.Description);
    }

    [Theory]
    [InlineData("--scenario bastion")]
    [InlineData("--scenario ddos")]
    [InlineData("--scenario policy-enforcement")]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args)
    {
        // Arrange
        Service.GetGuidanceAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("Sample guidance response");

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsGuidance_ForValidScenario()
    {
        // Arrange
        Service.GetGuidanceAsync("bastion", Arg.Any<CancellationToken>())
            .Returns("Bastion guidance: To enable Bastion, configure...");

        // Act
        var response = await ExecuteCommandAsync("--scenario", "bastion");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var result = ConvertResponse(response, AzureMigrateJsonContext.Default.GetGuidanceCommandResult);

        Assert.NotNull(result);
        Assert.Contains("Bastion guidance", result.Guidance);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        Service.GetGuidanceAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("DDoS guidance: To enable DDoS protection...");

        // Act
        var response = await ExecuteCommandAsync("--scenario", "ddos");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var result = ConvertResponse(response, AzureMigrateJsonContext.Default.GetGuidanceCommandResult);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Guidance);
    }

    [Fact]
    public async Task ExecuteAsync_WithPolicyName_SearchesForPolicies()
    {
        // Arrange
        Service.GetGuidanceAsync("policy-enforcement", Arg.Any<CancellationToken>())
            .Returns("Policy enforcement guidance...");

        Service.SearchPoliciesAsync("ddos", Arg.Any<CancellationToken>())
            .Returns([new("Enable-DDoS-VNET", ["corp", "connectivity"])]);

        // Act
        var response = await ExecuteCommandAsync("--scenario", "policy-enforcement", "--policy-name", "ddos");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var result = ConvertResponse(response, AzureMigrateJsonContext.Default.GetGuidanceCommandResult);

        Assert.NotNull(result);
        Assert.Contains("Enable-DDoS-VNET", result.Guidance);
        Assert.Contains("corp", result.Guidance);
        Assert.Contains("connectivity", result.Guidance);
    }

    [Fact]
    public async Task ExecuteAsync_WithListPolicies_ReturnsAllPolicies()
    {
        // Arrange
        Service.GetGuidanceAsync("policy-assignment", Arg.Any<CancellationToken>())
            .Returns("Policy assignment guidance...");

        Service.GetAllPoliciesAsync(Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, List<string>>
            {
                ["corp"] = ["Enable-DDoS-VNET", "Deny-Public-IP"],
                ["connectivity"] = ["Deploy-ASC-Monitoring"]
            });

        // Act
        var response = await ExecuteCommandAsync("--scenario", "policy-assignment", "--list-policies", "true");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var result = ConvertResponse(response, AzureMigrateJsonContext.Default.GetGuidanceCommandResult);

        Assert.NotNull(result);
        Assert.Contains("All Policies by Archetype", result.Guidance);
        Assert.Contains("Enable-DDoS-VNET", result.Guidance);
        Assert.Contains("Deny-Public-IP", result.Guidance);
        Assert.Contains("Deploy-ASC-Monitoring", result.Guidance);
    }

    [Fact]
    public async Task ExecuteAsync_PolicyNotFound_SuggestsListPolicies()
    {
        // Arrange
        Service.GetGuidanceAsync("policy-enforcement", Arg.Any<CancellationToken>())
            .Returns("Policy enforcement guidance...");

        Service.SearchPoliciesAsync("nonexistent", Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync("--scenario", "policy-enforcement", "--policy-name", "nonexistent");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var result = ConvertResponse(response, AzureMigrateJsonContext.Default.GetGuidanceCommandResult);

        Assert.NotNull(result);
        Assert.Contains("No policies matching 'nonexistent' found", result.Guidance);
        Assert.Contains("list-policies", result.Guidance);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Service error occurred");
        Service.GetGuidanceAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(expectedException);

        // Act
        var response = await ExecuteCommandAsync("--scenario", "bastion");

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.Contains("error", response.Message, StringComparison.OrdinalIgnoreCase);

        // Verify logging
        Logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Error fetching guidance for scenario")),
            expectedException,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesHttpRequestException()
    {
        // Arrange
        var httpException = new HttpRequestException("Network error", null, HttpStatusCode.ServiceUnavailable);
        Service.GetGuidanceAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(httpException);

        // Act
        var response = await ExecuteCommandAsync("--scenario", "ddos");

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Message);

        // Verify the exception was logged
        Logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            httpException,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesArgumentException()
    {
        // Arrange
        var argumentException = new ArgumentException("Invalid scenario", "scenario");
        Service.GetGuidanceAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(argumentException);

        // Act
        var response = await ExecuteCommandAsync("--scenario", "invalid-scenario");

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);

        // Verify error was logged
        Logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            argumentException,
            Arg.Any<Func<object, Exception?, string>>());
    }
}
