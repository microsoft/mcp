// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Microsoft.Mcp.Core.Options;
using Azure.Mcp.Tools.Compute.Commands.PlacementScore;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Compute.UnitTests.PlacementScore;

public class SpotPlacementScoreCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IComputePlacementService _placementService;
    private readonly ILogger<SpotPlacementScoreCommand> _logger;
    private readonly SpotPlacementScoreCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;
    private readonly string _knownSubscription = "sub123";
    private readonly string _knownLocation = "eastus";

    public SpotPlacementScoreCommandTests()
    {
        _placementService = Substitute.For<IComputePlacementService>();
        _logger = Substitute.For<ILogger<SpotPlacementScoreCommand>>();

        var collection = new ServiceCollection().AddSingleton(_placementService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
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

    [Theory]
    [InlineData("--subscription sub123 --location eastus --desired-locations eastus --desired-sizes Standard_D2_v2", true)]
    [InlineData("--subscription sub123 --location eastus --desired-locations eastus --desired-sizes Standard_D2_v2 --desired-count 5 --availability-zones false", true)]
    [InlineData("--subscription sub123 --location eastus --desired-sizes Standard_D2_v2", false)] // Missing desired-locations
    [InlineData("--subscription sub123 --location eastus --desired-locations eastus", false)] // Missing desired-sizes
    [InlineData("--subscription sub123 --desired-locations eastus --desired-sizes Standard_D2_v2", false)] // Missing location
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var scores = new List<PlacementScoreInfo>
            {
                new(Sku: "Standard_D2_v2", Region: "eastus", AvailabilityZone: "1", Score: "High", IsQuotaAvailable: true)
            };

            _placementService.GetSpotPlacementScoresAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string[]>(),
                Arg.Any<string[]>(),
                Arg.Any<int>(),
                Arg.Any<bool>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(scores);
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
            Assert.False(string.IsNullOrEmpty(response.Message));
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsScores_WhenValidRequest()
    {
        var expectedScores = new List<PlacementScoreInfo>
        {
            new(Sku: "Standard_D2_v2", Region: "eastus", AvailabilityZone: "1", Score: "High", IsQuotaAvailable: true),
            new(Sku: "Standard_D2_v2", Region: "eastus", AvailabilityZone: "2", Score: "Medium", IsQuotaAvailable: true),
            new(Sku: "Standard_D4s_v3", Region: "eastus", AvailabilityZone: null, Score: "Low", IsQuotaAvailable: false)
        };

        _placementService.GetSpotPlacementScoresAsync(
            Arg.Is(_knownLocation),
            Arg.Is(_knownSubscription),
            Arg.Any<string[]>(),
            Arg.Any<string[]>(),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedScores);

        var parseResult = _commandDefinition.Parse([
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--desired-locations", "eastus",
            "--desired-sizes", "Standard_D2_v2",
            "--desired-count", "3",
            "--availability-zones", "true"
        ]);

        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.True(root.TryGetProperty("scores", out var scoresElement));
        Assert.Equal(3, scoresElement.GetArrayLength());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        _placementService.GetSpotPlacementScoresAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string[]>(),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("rate limit exceeded"));

        var parseResult = _commandDefinition.Parse([
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--desired-locations", "eastus",
            "--desired-sizes", "Standard_D2_v2"
        ]);

        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.Contains("rate limit exceeded", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_PassesBoundOptionsToService()
    {
        var scores = new List<PlacementScoreInfo>
        {
            new(Sku: "Standard_D2_v2", Region: "eastus", AvailabilityZone: "1", Score: "High", IsQuotaAvailable: true)
        };

        _placementService.GetSpotPlacementScoresAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string[]>(),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(scores);

        var parseResult = _commandDefinition.Parse([
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--desired-locations", "eastus",
            "--desired-sizes", "Standard_D2_v2",
            "--desired-count", "7",
            "--availability-zones", "false"
        ]);

        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _placementService.Received(1).GetSpotPlacementScoresAsync(
            Arg.Is(_knownLocation),
            Arg.Is(_knownSubscription),
            Arg.Is<string[]>(x => x.Length == 1 && x[0] == "eastus"),
            Arg.Is<string[]>(x => x.Length == 1 && x[0] == "Standard_D2_v2"),
            Arg.Is(7),
            Arg.Is(false),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }
}
