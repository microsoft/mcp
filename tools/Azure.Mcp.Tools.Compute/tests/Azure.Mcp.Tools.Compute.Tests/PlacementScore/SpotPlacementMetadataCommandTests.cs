// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Compute.Commands;
using Azure.Mcp.Tools.Compute.Commands.PlacementScore;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Compute.UnitTests.PlacementScore;

public class SpotPlacementMetadataCommandTests
    : CommandUnitTestsBase<SpotPlacementMetadataCommand, IComputePlacementService>
{
    private const string KnownSubscription = "sub123";
    private const string KnownLocation = "eastus";

    [Fact]
    public void Constructor_ThrowsForNullDependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new SpotPlacementMetadataCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new SpotPlacementMetadataCommand(Substitute.For<ILogger<SpotPlacementMetadataCommand>>(), null!));
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", CommandDefinition.Name);
        Assert.False(string.IsNullOrWhiteSpace(CommandDefinition.Description));
    }

    [Theory]
    [InlineData("--subscription sub123 --location eastus", true)]
    [InlineData("--subscription sub123", false)] // missing --location
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.GetSpotPlacementMetadataAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(new SpotPlacementMetadataInfo(
                    Id: "id",
                    Name: "default",
                    ResourceType: "Microsoft.Compute/locations/spotPlacementScores",
                    SupportedResourceTypes: ["virtualMachines"]));
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsMetadata_WhenValidRequest()
    {
        var expected = new SpotPlacementMetadataInfo(
            Id: $"/subscriptions/{KnownSubscription}/providers/Microsoft.Compute/locations/{KnownLocation}/spotPlacementScores/default",
            Name: "default",
            ResourceType: "Microsoft.Compute/locations/spotPlacementScores",
            SupportedResourceTypes: ["virtualMachines", "virtualMachineScaleSets"]);

        Service.GetSpotPlacementMetadataAsync(
            Arg.Is(KnownLocation),
            Arg.Is(KnownSubscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync(
            "--subscription", KnownSubscription,
            "--location", KnownLocation);

        var result = ValidateAndDeserializeResponse(
            response,
            ComputePlacementJsonContext.Default.SpotPlacementMetadataCommandResult);

        Assert.NotNull(result.Metadata);
        Assert.Equal("default", result.Metadata.Name);
        Assert.Equal(2, result.Metadata.SupportedResourceTypes.Count);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.GetSpotPlacementMetadataAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("service failure"));

        var response = await ExecuteCommandAsync(
            "--subscription", KnownSubscription,
            "--location", KnownLocation);

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.Contains("service failure", response.Message);
    }
}
