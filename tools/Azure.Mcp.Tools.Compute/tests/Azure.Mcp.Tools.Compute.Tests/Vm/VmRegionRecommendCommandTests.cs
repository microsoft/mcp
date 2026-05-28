// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Compute.Commands;
using Azure.Mcp.Tools.Compute.Commands.Vm;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Compute.UnitTests.Vm;

public class VmRegionRecommendCommandTests : CommandUnitTestsBase<VmRegionRecommendCommand, IComputeService>
{
    private readonly string _knownSubscription = "sub123";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("recommend-region", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub123", true)]
    [InlineData("--subscription sub123 --workload-hint gpu --geography-preference us --require-availability-zones --top 5", true)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.RecommendVmRegionsAsync(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<bool>(),
                Arg.Any<int?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(new List<VmRegionRecommendation>());
        }

        var response = await ExecuteCommandAsync(args);
        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsRecommendations()
    {
        var regions = new List<VmRegionRecommendation>
        {
            new("eastus", "East US", "United States", "Virginia", true, 100, "Popular tier-1 region with multi-AZ support."),
            new("westus3", "West US 3", "United States", "Phoenix", true, 85, "Tier-1 region with multi-AZ support."),
            new("eastus2", "East US 2", "United States", "Virginia", true, 90, "Popular tier-1 region with multi-AZ support."),
        };

        Service.RecommendVmRegionsAsync(
            Arg.Is(_knownSubscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<bool>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(regions);

        var response = await ExecuteCommandAsync(
            "--subscription", _knownSubscription);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.VmRegionRecommendCommandResult);
        Assert.Equal(3, result.Regions.Count);
        Assert.Equal("eastus", result.Regions[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_PassesHintsToService()
    {
        Service.RecommendVmRegionsAsync(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<int?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new List<VmRegionRecommendation>());

        await ExecuteCommandAsync(
            "--subscription", _knownSubscription,
            "--workload-hint", "gpu training",
            "--geography-preference", "us",
            "--require-availability-zones",
            "--top", "3");

        await Service.Received(1).RecommendVmRegionsAsync(
            _knownSubscription,
            "gpu training",
            "us",
            true,
            3,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesForbiddenError()
    {
        Service.RecommendVmRegionsAsync(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<int?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Forbidden"));

        var response = await ExecuteCommandAsync(
            "--subscription", _knownSubscription);

        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_AzRichRegionsOutrankNonAzForVmssHint()
    {
        // For the unified VMSS Flex recommendation path, the service now applies a stronger weight to
        // AZ-rich regions when the workload hint mentions scale/HA/production/vmss AND the caller
        // requires availability zones. We verify two things at the command boundary:
        //   (1) --workload-hint "vmss production" and --require-availability-zones flow through to
        //       the service (so the scoring heuristic actually fires).
        //   (2) The command preserves the service's ordering: an AZ-rich region with a higher score
        //       lands ahead of a non-AZ region in the deserialized response.
        var regions = new List<VmRegionRecommendation>
        {
            new("eastus", "East US", "United States", "Virginia", true, 140,
                "Tier-1 region with 3 AZs — strongly preferred for VMSS Flex production workloads."),
            new("westus3", "West US 3", "United States", "Phoenix", true, 130,
                "Multi-AZ region — strong VMSS Flex fit."),
            new("northcentralus", "North Central US", "United States", "Illinois", false, 60,
                "No availability zones — not recommended for production VMSS Flex."),
        };

        Service.RecommendVmRegionsAsync(
            Arg.Is(_knownSubscription),
            Arg.Is<string?>("vmss production"),
            Arg.Any<string?>(),
            Arg.Is(true),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(regions);

        var response = await ExecuteCommandAsync(
            "--subscription", _knownSubscription,
            "--workload-hint", "vmss production",
            "--require-availability-zones");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.VmRegionRecommendCommandResult);
        Assert.Equal(3, result.Regions.Count);

        // The first two results should be AZ-rich and outrank the non-AZ region.
        Assert.True(result.Regions[0].AvailabilityZones);
        Assert.True(result.Regions[1].AvailabilityZones);
        Assert.False(result.Regions[2].AvailabilityZones);
        Assert.True(result.Regions[0].Score > result.Regions[2].Score);
        Assert.True(result.Regions[1].Score > result.Regions[2].Score);

        await Service.Received(1).RecommendVmRegionsAsync(
            _knownSubscription,
            "vmss production",
            Arg.Any<string?>(),
            true,
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }
}
