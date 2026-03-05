// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ComputeRecommender.Commands;
using Azure.Mcp.Tools.ComputeRecommender.Commands.PlacementScore;
using Azure.Mcp.Tools.ComputeRecommender.Models;
using Azure.Mcp.Tools.ComputeRecommender.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ComputeRecommender.UnitTests.PlacementScore;

public class SpotPlacementScoreCommandTests
{
    private readonly IComputeRecommenderService _service;
    private readonly ILogger<SpotPlacementScoreCommand> _logger;
    private readonly SpotPlacementScoreCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public SpotPlacementScoreCommandTests()
    {
        _service = Substitute.For<IComputeRecommenderService>();
        _logger = Substitute.For<ILogger<SpotPlacementScoreCommand>>();

        var collection = new ServiceCollection().AddSingleton(_service);

        var serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("generate", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsScores_WhenServiceSucceeds()
    {
        // Arrange
        var subscription = "sub123";
        var location = "eastus";
        var desiredLocations = new[] { "eastus", "westus2" };
        var desiredSizes = new[] { "Standard_D2_v2" };

        var expectedScores = new List<PlacementScoreInfo>
        {
            new("Standard_D2_v2", "eastus", "1", "High", true),
            new("Standard_D2_v2", "eastus", "2", "Medium", true),
            new("Standard_D2_v2", "westus2", "1", "High", true),
            new("Standard_D2_v2", "westus2", "2", "Low", false)
        };

        _service.GetSpotPlacementScoresAsync(
            Arg.Is(location),
            Arg.Is(subscription),
            Arg.Any<string[]>(),
            Arg.Any<string[]>(),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedScores));

        var args = _commandDefinition.Parse([
            "--subscription", subscription,
            "--location", location,
            "--desired-locations", "eastus",
            "--desired-locations", "westus2",
            "--desired-sizes", "Standard_D2_v2"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeRecommenderJsonContext.Default.SpotPlacementScoreCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Scores);
        Assert.Equal(4, result.Scores.Count);
        Assert.Equal("High", result.Scores[0].Score);
        Assert.Equal("Standard_D2_v2", result.Scores[0].Sku);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoScores()
    {
        var subscription = "sub123";
        var location = "eastus";

        _service.GetSpotPlacementScoresAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string[]>(),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<PlacementScoreInfo>()));

        var args = _commandDefinition.Parse([
            "--subscription", subscription,
            "--location", location,
            "--desired-locations", "eastus",
            "--desired-sizes", "Standard_D2_v2"]);

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeRecommenderJsonContext.Default.SpotPlacementScoreCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Scores);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var expectedError = "Rate limit exceeded";
        var subscription = "sub123";
        var location = "eastus";

        _service.GetSpotPlacementScoresAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string[]>(),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse([
            "--subscription", subscription,
            "--location", location,
            "--desired-locations", "eastus",
            "--desired-sizes", "Standard_D2_v2"]);

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Theory]
    [InlineData("--subscription sub123 --location eastus --desired-locations eastus --desired-sizes Standard_D2_v2", true)]
    [InlineData("--subscription sub123 --location eastus --desired-locations eastus", false)]
    [InlineData("--subscription sub123 --location eastus --desired-sizes Standard_D2_v2", false)]
    [InlineData("--subscription sub123 --desired-locations eastus --desired-sizes Standard_D2_v2", false)]
    [InlineData("--location eastus --desired-locations eastus --desired-sizes Standard_D2_v2", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            _service.GetSpotPlacementScoresAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string[]>(),
                Arg.Any<string[]>(),
                Arg.Any<int>(),
                Arg.Any<bool>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new List<PlacementScoreInfo>
                {
                    new("Standard_D2_v2", "eastus", "1", "High", true)
                }));
        }

        var parseResult = _commandDefinition.Parse(args);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        _service.GetSpotPlacementScoresAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string[]>(),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var parseResult = _commandDefinition.Parse([
            "--subscription", "sub123",
            "--location", "eastus",
            "--desired-locations", "eastus",
            "--desired-sizes", "Standard_D2_v2"]);

        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        _service.GetSpotPlacementScoresAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string[]>(),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        var parseResult = _commandDefinition.Parse([
            "--subscription", "sub123",
            "--location", "eastus",
            "--desired-locations", "eastus",
            "--desired-sizes", "Standard_D2_v2"]);

        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_PassesDesiredCountAndAvailabilityZones()
    {
        // Arrange
        var subscription = "sub123";
        var location = "eastus";

        _service.GetSpotPlacementScoresAsync(
            Arg.Is(location),
            Arg.Is(subscription),
            Arg.Any<string[]>(),
            Arg.Any<string[]>(),
            Arg.Is(5),
            Arg.Is(false),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<PlacementScoreInfo>
            {
                new("Standard_D2_v2", "eastus", null, "High", true)
            }));

        var args = _commandDefinition.Parse([
            "--subscription", subscription,
            "--location", location,
            "--desired-locations", "eastus",
            "--desired-sizes", "Standard_D2_v2",
            "--desired-count", "5",
            "--availability-zones", "false"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);

        await _service.Received(1).GetSpotPlacementScoresAsync(
            location,
            subscription,
            Arg.Any<string[]>(),
            Arg.Any<string[]>(),
            5,
            false,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }
}
