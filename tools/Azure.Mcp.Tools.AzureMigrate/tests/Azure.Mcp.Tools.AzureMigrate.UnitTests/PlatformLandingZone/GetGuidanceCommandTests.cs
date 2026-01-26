// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.AzureMigrate.Commands.PlatformLandingZone;
using Azure.Mcp.Tools.AzureMigrate.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using static Azure.Mcp.Tools.AzureMigrate.Services.PlatformLandingZoneGuidanceService;

namespace Azure.Mcp.Tools.AzureMigrate.UnitTests.PlatformLandingZone;

public class GetGuidanceCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GetGuidanceCommand> _logger;
    private readonly IPlatformLandingZoneGuidanceService _guidanceService;
    private readonly GetGuidanceCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public GetGuidanceCommandTests()
    {
        _logger = Substitute.For<ILogger<GetGuidanceCommand>>();
        _guidanceService = Substitute.For<IPlatformLandingZoneGuidanceService>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_guidanceService);
        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
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
        _guidanceService.GetGuidanceAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("Sample guidance response");

        // Act
        var parseResult = _commandDefinition.Parse(args);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsGuidance_ForValidScenario()
    {
        // Arrange
        _guidanceService.GetGuidanceAsync("bastion", Arg.Any<CancellationToken>())
            .Returns("Bastion guidance: To enable Bastion, configure...");

        var parseResult = _commandDefinition.Parse(["--scenario", "bastion"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, Commands.AzureMigrateJsonContext.Default.GetGuidanceCommandResult);

        Assert.NotNull(result);
        Assert.Contains("Bastion guidance", result.Guidance);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        _guidanceService.GetGuidanceAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("DDoS guidance: To enable DDoS protection...");

        var parseResult = _commandDefinition.Parse(["--scenario", "ddos"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, Commands.AzureMigrateJsonContext.Default.GetGuidanceCommandResult);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Guidance);
    }

    [Fact]
    public async Task ExecuteAsync_WithPolicyName_SearchesForPolicies()
    {
        // Arrange
        _guidanceService.GetGuidanceAsync("policy-enforcement", Arg.Any<CancellationToken>())
            .Returns("Policy enforcement guidance...");

        _guidanceService.SearchPoliciesAsync("ddos", Arg.Any<CancellationToken>())
            .Returns([
                new PolicyLocationResult("Enable-DDoS-VNET", ["corp", "connectivity"])
            ]);

        var parseResult = _commandDefinition.Parse([
            "--scenario", "policy-enforcement",
            "--policy-name", "ddos"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, Commands.AzureMigrateJsonContext.Default.GetGuidanceCommandResult);

        Assert.NotNull(result);
        Assert.Contains("Enable-DDoS-VNET", result.Guidance);
        Assert.Contains("corp", result.Guidance);
        Assert.Contains("connectivity", result.Guidance);
    }

    [Fact]
    public async Task ExecuteAsync_WithListPolicies_ReturnsAllPolicies()
    {
        // Arrange
        _guidanceService.GetGuidanceAsync("policy-assignment", Arg.Any<CancellationToken>())
            .Returns("Policy assignment guidance...");

        _guidanceService.GetAllPoliciesAsync(Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, List<string>>
            {
                ["corp"] = ["Enable-DDoS-VNET", "Deny-Public-IP"],
                ["connectivity"] = ["Deploy-ASC-Monitoring"]
            });

        var parseResult = _commandDefinition.Parse([
            "--scenario", "policy-assignment",
            "--list-policies", "true"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, Commands.AzureMigrateJsonContext.Default.GetGuidanceCommandResult);

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
        _guidanceService.GetGuidanceAsync("policy-enforcement", Arg.Any<CancellationToken>())
            .Returns("Policy enforcement guidance...");

        _guidanceService.SearchPoliciesAsync("nonexistent", Arg.Any<CancellationToken>())
            .Returns([]);

        var parseResult = _commandDefinition.Parse([
            "--scenario", "policy-enforcement",
            "--policy-name", "nonexistent"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, Commands.AzureMigrateJsonContext.Default.GetGuidanceCommandResult);

        Assert.NotNull(result);
        Assert.Contains("No policies matching 'nonexistent' found", result.Guidance);
        Assert.Contains("list-policies", result.Guidance);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Service error occurred");
        _guidanceService.GetGuidanceAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<string>(expectedException));

        var parseResult = _commandDefinition.Parse(["--scenario", "bastion"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.Contains("error", response.Message, StringComparison.OrdinalIgnoreCase);

        // Verify logging
        _logger.Received(1).Log(
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
        _guidanceService.GetGuidanceAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<string>(httpException));

        var parseResult = _commandDefinition.Parse(["--scenario", "ddos"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Message);

        // Verify the exception was logged
        _logger.Received(1).Log(
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
        _guidanceService.GetGuidanceAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<string>(argumentException));

        var parseResult = _commandDefinition.Parse(["--scenario", "invalid-scenario"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);

        // Verify error was logged
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            argumentException,
            Arg.Any<Func<object, Exception?, string>>());
    }
}
