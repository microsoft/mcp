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

public class SpotPlacementScoreCommandTests
    : CommandUnitTestsBase<SpotPlacementScoreCommand, IComputePlacementService>
{
    private const string KnownSubscription = "sub123";
    private const string KnownLocation = "eastus";

    [Fact]
    public void Constructor_ThrowsForNullDependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new SpotPlacementScoreCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new SpotPlacementScoreCommand(Substitute.For<ILogger<SpotPlacementScoreCommand>>(), null!));
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("generate", CommandDefinition.Name);
        Assert.False(string.IsNullOrWhiteSpace(CommandDefinition.Description));
    }

    [Theory]
    [InlineData("--subscription sub123 --location eastus --desired-locations eastus --desired-sizes Standard_D2_v2", true)]
    [InlineData("--subscription sub123 --location eastus --desired-locations eastus --desired-sizes Standard_D2_v2 --desired-count 5 --availability-zones false", true)]
    [InlineData("--subscription sub123 --location eastus --desired-sizes Standard_D2_v2", false)] // missing --desired-locations
    [InlineData("--subscription sub123 --location eastus --desired-locations eastus", false)] // missing --desired-sizes
    [InlineData("--subscription sub123 --desired-locations eastus --desired-sizes Standard_D2_v2", false)] // missing --location
    [InlineData("--subscription sub123 --location eastus --desired-locations eastus --desired-sizes Standard_D2_v2 --desired-count 0", false)] // count < 1
    [InlineData("--subscription sub123 --location eastus --desired-locations eastus --desired-sizes Standard_D2_v2 --desired-count 1001", false)] // count > 1000
    [InlineData("--subscription sub123 --location eastus --desired-locations eastus --desired-locations westus --desired-locations centralus --desired-locations northeurope --desired-sizes Standard_D2_v2", false)] // > 3 desired-locations
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.GetSpotPlacementScoresAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string[]>(),
                Arg.Any<string[]>(),
                Arg.Any<int>(),
                Arg.Any<bool>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns([
                    new PlacementScoreInfo(Sku: "Standard_D2_v2", Region: "eastus", AvailabilityZone: "1", Score: "High", IsQuotaAvailable: true)
                ]);
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsScores_WhenValidRequest()
    {
        var expected = new List<PlacementScoreInfo>
        {
            new(Sku: "Standard_D2_v2", Region: "eastus", AvailabilityZone: "1", Score: "High", IsQuotaAvailable: true),
            new(Sku: "Standard_D2_v2", Region: "eastus", AvailabilityZone: "2", Score: "Medium", IsQuotaAvailable: true),
            new(Sku: "Standard_D4s_v3", Region: "eastus", AvailabilityZone: null, Score: "Low", IsQuotaAvailable: false),
        };

        Service.GetSpotPlacementScoresAsync(
            Arg.Is(KnownLocation),
            Arg.Is(KnownSubscription),
            Arg.Any<string[]>(),
            Arg.Any<string[]>(),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync(
            "--subscription", KnownSubscription,
            "--location", KnownLocation,
            "--desired-locations", "eastus",
            "--desired-sizes", "Standard_D2_v2",
            "--desired-count", "3",
            "--availability-zones", "true");

        var result = ValidateAndDeserializeResponse(
            response,
            ComputePlacementJsonContext.Default.SpotPlacementScoreCommandResult);

        Assert.Equal(3, result.Scores.Count);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.GetSpotPlacementScoresAsync(
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

        var response = await ExecuteCommandAsync(
            "--subscription", KnownSubscription,
            "--location", KnownLocation,
            "--desired-locations", "eastus",
            "--desired-sizes", "Standard_D2_v2");

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.Contains("rate limit exceeded", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_PassesBoundOptionsToService()
    {
        Service.GetSpotPlacementScoresAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string[]>(),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns([
                new PlacementScoreInfo(Sku: "Standard_D2_v2", Region: "eastus", AvailabilityZone: "1", Score: "High", IsQuotaAvailable: true)
            ]);

        var response = await ExecuteCommandAsync(
            "--subscription", KnownSubscription,
            "--location", KnownLocation,
            "--desired-locations", "eastus",
            "--desired-sizes", "Standard_D2_v2",
            "--desired-count", "7",
            "--availability-zones", "false");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).GetSpotPlacementScoresAsync(
            Arg.Is(KnownLocation),
            Arg.Is(KnownSubscription),
            Arg.Is<string[]>(x => x.Length == 1 && x[0] == "eastus"),
            Arg.Is<string[]>(x => x.Length == 1 && x[0] == "Standard_D2_v2"),
            Arg.Is(7),
            Arg.Is(false),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }
}
