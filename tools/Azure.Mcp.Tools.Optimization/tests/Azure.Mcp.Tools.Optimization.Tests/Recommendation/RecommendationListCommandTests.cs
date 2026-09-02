// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// cSpell:ignore subcat

using System.Net;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Optimization.Commands;
using Azure.Mcp.Tools.Optimization.Commands.Recommendation;
using Azure.Mcp.Tools.Optimization.Models;
using Azure.Mcp.Tools.Optimization.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Optimization.Tests.Recommendation;

public class RecommendationListCommandTests
    : SubscriptionCommandUnitTestsBase<RecommendationListCommand, IOptimizationService>
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
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.ListCostSavingsAsync(
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
                .Returns(new ResourceQueryResults<CostSavingsRecommendation>([], false));
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (!shouldSucceed)
        {
            Assert.Contains("required", response.Message.ToLower());
        }
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
            .Returns(new ResourceQueryResults<CostSavingsRecommendation>(expected, false));

        var response = await ExecuteCommandAsync("--subscription", "sub123");

        var result = ValidateAndDeserializeResponse(response, OptimizationJsonContext.Default.RecommendationListResult);
        Assert.Single(result.Recommendations);
        Assert.Equal("name1", result.Recommendations[0].Name);
        Assert.False(result.AreResultsTruncated);
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
}
