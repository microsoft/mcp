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

public class SpotPlacementMetadataCommandTests
{
    private readonly IComputeRecommenderService _service;
    private readonly ILogger<SpotPlacementMetadataCommand> _logger;
    private readonly SpotPlacementMetadataCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public SpotPlacementMetadataCommandTests()
    {
        _service = Substitute.For<IComputeRecommenderService>();
        _logger = Substitute.For<ILogger<SpotPlacementMetadataCommand>>();

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
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsMetadata_WhenServiceSucceeds()
    {
        // Arrange
        var subscription = "sub123";
        var location = "eastus";
        var expectedMetadata = new SpotPlacementMetadataInfo(
            Id: "/subscriptions/sub123/providers/Microsoft.Compute/locations/eastus/placementScores/spot",
            Name: "spot",
            ResourceType: "Microsoft.Compute/locations/placementScores",
            SupportedResourceTypes: ["Microsoft.Compute/virtualMachines", "Microsoft.Compute/virtualMachineScaleSets"]);

        _service.GetSpotPlacementMetadataAsync(
            Arg.Is(location),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedMetadata));

        var args = _commandDefinition.Parse(["--subscription", subscription, "--location", location]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeRecommenderJsonContext.Default.SpotPlacementMetadataCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Metadata);
        Assert.Equal("spot", result.Metadata.Name);
        Assert.Equal(2, result.Metadata.SupportedResourceTypes.Count);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var subscription = "sub123";
        var location = "eastus";
        var expectedError = "Service unavailable";

        _service.GetSpotPlacementMetadataAsync(
            Arg.Is(location),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse(["--subscription", subscription, "--location", location]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Theory]
    [InlineData("--subscription sub123 --location eastus", true)]
    [InlineData("--subscription sub123", false)]
    [InlineData("--location eastus", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var expectedMetadata = new SpotPlacementMetadataInfo(
                Id: "/subscriptions/sub123/providers/Microsoft.Compute/locations/eastus/placementScores/spot",
                Name: "spot",
                ResourceType: "Microsoft.Compute/locations/placementScores",
                SupportedResourceTypes: ["Microsoft.Compute/virtualMachines"]);

            _service.GetSpotPlacementMetadataAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(expectedMetadata));
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
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        var subscription = "sub123";
        var location = "eastus";

        _service.GetSpotPlacementMetadataAsync(
            Arg.Is(location),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        var parseResult = _commandDefinition.Parse(["--subscription", subscription, "--location", location]);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        var subscription = "sub123";
        var location = "invalidlocation";

        _service.GetSpotPlacementMetadataAsync(
            Arg.Is(location),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Resource not found"));

        var parseResult = _commandDefinition.Parse(["--subscription", subscription, "--location", location]);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("Resource not found", response.Message);
    }
}
