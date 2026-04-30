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

public class SpotPlacementMetadataCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IComputePlacementService _placementService;
    private readonly ILogger<SpotPlacementMetadataCommand> _logger;
    private readonly SpotPlacementMetadataCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;
    private readonly string _knownSubscription = "sub123";
    private readonly string _knownLocation = "eastus";

    public SpotPlacementMetadataCommandTests()
    {
        _placementService = Substitute.For<IComputePlacementService>();
        _logger = Substitute.For<ILogger<SpotPlacementMetadataCommand>>();

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
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub123 --location eastus", true)]
    [InlineData("--subscription sub123", false)] // Missing location
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var metadata = new SpotPlacementMetadataInfo(
                Id: "/subscriptions/sub123/providers/Microsoft.Compute/locations/eastus/spotPlacementScores/default",
                Name: "default",
                ResourceType: "Microsoft.Compute/locations/spotPlacementScores",
                SupportedResourceTypes: new List<string> { "virtualMachines", "virtualMachineScaleSets" });

            _placementService.GetSpotPlacementMetadataAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(metadata);
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
    public async Task ExecuteAsync_ReturnsMetadata_WhenValidRequest()
    {
        var expectedMetadata = new SpotPlacementMetadataInfo(
            Id: $"/subscriptions/{_knownSubscription}/providers/Microsoft.Compute/locations/{_knownLocation}/spotPlacementScores/default",
            Name: "default",
            ResourceType: "Microsoft.Compute/locations/spotPlacementScores",
            SupportedResourceTypes: new List<string> { "virtualMachines", "virtualMachineScaleSets" });

        _placementService.GetSpotPlacementMetadataAsync(
            Arg.Is(_knownLocation),
            Arg.Is(_knownSubscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedMetadata);

        var parseResult = _commandDefinition.Parse([
            "--subscription", _knownSubscription,
            "--location", _knownLocation
        ]);

        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.True(root.TryGetProperty("metadata", out var metadataElement));
        Assert.Equal("default", metadataElement.GetProperty("name").GetString());
        Assert.Equal(2, metadataElement.GetProperty("supportedResourceTypes").GetArrayLength());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        _placementService.GetSpotPlacementMetadataAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("service failure"));

        var parseResult = _commandDefinition.Parse([
            "--subscription", _knownSubscription,
            "--location", _knownLocation
        ]);

        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.Contains("service failure", response.Message);
    }
}
