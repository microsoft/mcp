// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// cSpell:ignore subcat

using System.Net;
using Azure.Mcp.Tools.Optimization.Commands;
using Azure.Mcp.Tools.Optimization.Commands.Recommendation;
using Azure.Mcp.Tools.Optimization.Models;
using Azure.Mcp.Tools.Optimization.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Optimization.Tests.Recommendation;

public class RecommendationListCommandTests
    : CommandUnitTestsBase<RecommendationListCommand, IOptimizationService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("list", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub123", true)]
    [InlineData("--subscription sub123 --top 10", true)]
    [InlineData("", true)]
    [InlineData("--top 10", true)]
    [InlineData("--top abc", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.ListCostSavingsAsync(
                Arg.Any<string?>(),
                Arg.Any<int>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
                .Returns(new CostSavingsResult([], false));
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutSubscription_QueriesAllAccessibleSubscriptions()
    {
        Service.ListCostSavingsAsync(
            Arg.Any<string?>(),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new CostSavingsResult([], false));

        var response = await ExecuteCommandAsync("--top", "5");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).ListCostSavingsAsync(
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            5,
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsRecommendations()
    {
        var expected = new List<CostSavingsRecommendation>
        {
            new("id1", "name1", "tenant1", "rg1", "sub1", "type1", "USD", 1200, 100, 5.5,
                "Right-size the VM", "detail", "subcat", "solution", "virtualmachines", "vm1", "high",
                "/subscriptions/sub1/resourcegroups/rg1/providers/microsoft.compute/virtualmachines/vm1"),
        };
        Service.ListCostSavingsAsync(
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new CostSavingsResult(expected, false));

        var response = await ExecuteCommandAsync("--subscription", "sub123");

        var result = ValidateAndDeserializeResponse(response, OptimizationJsonContext.Default.RecommendationListResult);
        Assert.Single(result.Recommendations);
        Assert.Equal("name1", result.Recommendations[0].Name);
        Assert.False(result.AreResultsTruncated);
        Assert.Null(result.DiskRecommendationSummary);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsDiskRecommendationSummary()
    {
        var summary = new DiskRecommendationSummary(
            "Review disks that are not attached to a VM and evaluate if you still need the disks: 2 found.",
            2,
            ["sub1"],
            "https://ms.portal.azure.com/#view/Microsoft_Azure_Expert/RecommendationList.ReactView");
        Service.ListCostSavingsAsync(
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new CostSavingsResult([], false, DiskRecommendationSummary: summary));

        var response = await ExecuteCommandAsync("--subscription", "sub123");

        var result = ValidateAndDeserializeResponse(response, OptimizationJsonContext.Default.RecommendationListResult);
        Assert.NotNull(result.DiskRecommendationSummary);
        Assert.Equal(2, result.DiskRecommendationSummary!.UnattachedDiskCount);
        Assert.Equal(summary.ActionUrl, result.DiskRecommendationSummary.ActionUrl);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMultipleSubscriptionsMatch_AsksUserToSelect()
    {
        var candidates = new List<SubscriptionOption>
        {
            new("sub1", "contoso-dev", "tenant1"),
            new("sub2", "contoso-prod", "tenant1"),
        };
        Service.ListCostSavingsAsync(
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new CostSavingsResult([], false, candidates));

        var response = await ExecuteCommandAsync("--subscription", "contoso");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(response, OptimizationJsonContext.Default.RecommendationListResult);
        Assert.Empty(result.Recommendations);
        Assert.NotNull(result.SubscriptionOptions);
        Assert.Equal(2, result.SubscriptionOptions!.Count);
        Assert.NotNull(result.Message);
        Assert.Contains("select", result.Message!.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.ListCostSavingsAsync(
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--subscription", "sub123");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutSubscriptionAndMultipleTenants_ReturnsBadRequest()
    {
        Service.ListCostSavingsAsync(
            Arg.Any<string?>(),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException("Multiple tenants are accessible, so the tenant to query cannot be inferred. Specify --tenant."));

        var response = await ExecuteCommandAsync("--top", "5");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--tenant", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_PassesTenantToService()
    {
        Service.ListCostSavingsAsync(
            Arg.Any<string?>(),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new CostSavingsResult([], false));

        var response = await ExecuteCommandAsync("--tenant", "Contoso");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).ListCostSavingsAsync(
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Any<int>(),
            "Contoso",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_UnknownTenantOrSubscription_ReturnsNotFound()
    {
        Service.ListCostSavingsAsync(
            Arg.Any<string?>(),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new KeyNotFoundException("Could not find tenant with name Contoso"));

        var response = await ExecuteCommandAsync("--tenant", "Contoso");

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
    }
}
