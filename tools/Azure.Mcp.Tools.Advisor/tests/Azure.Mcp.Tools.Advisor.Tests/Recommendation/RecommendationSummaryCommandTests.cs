// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Advisor.Commands;
using Azure.Mcp.Tools.Advisor.Commands.Recommendation;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Recommendation;

public class RecommendationSummaryCommandTests : CommandUnitTestsBase<RecommendationSummaryCommand, IAdvisorService>
{
    private static RecommendationSummary EmptySummary(string groupBy = "category", int top = 5) =>
        new(GroupBy: groupBy, Top: top, TotalRecommendations: 0, AreResultsTruncated: false, Groups: []);

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("summary", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub1 --group-by category", true)]
    [InlineData("--subscription sub1 --group-by impact --top 10", true)]
    [InlineData("--subscription sub1", false)]                            // missing --group-by
    [InlineData("", false)]                                                // missing everything
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.SummarizeRecommendationsAsync(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<RecommendationFilters?>(),
                Arg.Any<CancellationToken>())
                .Returns(EmptySummary());
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidGroupBy_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync("--subscription", "sub1", "--group-by", "nonsense");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("nonsense", response.Message);
        Assert.Contains("Allowed values", response.Message);
        await Service.DidNotReceive().SummarizeRecommendationsAsync(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string>(), Arg.Any<int>(), Arg.Any<RecommendationFilters?>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("category")]
    [InlineData("Category")]
    [InlineData("  category  ")]
    public async Task ExecuteAsync_GroupBy_NormalizedToLowercaseTrimmed(string raw)
    {
        string? captured = null;
        Service.SummarizeRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Do<string>(g => captured = g),
            Arg.Any<int>(),
            Arg.Any<RecommendationFilters?>(),
            Arg.Any<CancellationToken>())
            .Returns(EmptySummary());

        var response = await ExecuteCommandAsync("--subscription", "sub1", "--group-by", raw);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("category", captured);
    }

    [Theory]
    [InlineData(null, 5)]    // omitted -> default 5
    [InlineData(0, 1)]       // below min -> clamped to 1
    [InlineData(-3, 1)]      // negative -> clamped to 1
    [InlineData(7, 7)]       // in-range pass-through
    [InlineData(50, 50)]     // at max
    [InlineData(500, 50)]    // above max -> clamped to 50
    public async Task ExecuteAsync_Top_IsClampedCorrectly(int? input, int expected)
    {
        int? captured = null;
        Service.SummarizeRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string>(),
            Arg.Do<int>(t => captured = t),
            Arg.Any<RecommendationFilters?>(),
            Arg.Any<CancellationToken>())
            .Returns(EmptySummary());

        var args = input is null
            ? new[] { "--subscription", "sub1", "--group-by", "category" }
            : ["--subscription", "sub1", "--group-by", "category", "--top", input.Value.ToString()];

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal(expected, captured);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsFiltersToService()
    {
        RecommendationFilters? captured = null;
        Service.SummarizeRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Do<RecommendationFilters?>(f => captured = f),
            Arg.Any<CancellationToken>())
            .Returns(EmptySummary());

        await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--group-by", "category",
            "--category", "Security",
            "--impact", "High",
            "--resource-type", "Microsoft.Storage/storageAccounts",
            "--resource", "mystorage",
            "--search", "encryption");

        Assert.NotNull(captured);
        Assert.Equal("Security", captured!.Category);
        Assert.Equal("High", captured.Impact);
        Assert.Equal("Microsoft.Storage/storageAccounts", captured.ResourceType);
        Assert.Equal("mystorage", captured.Resource);
        Assert.Equal("encryption", captured.Search);
    }

    [Fact]
    public async Task ExecuteAsync_OmittedFilters_AreNull()
    {
        RecommendationFilters? captured = null;
        Service.SummarizeRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Do<RecommendationFilters?>(f => captured = f),
            Arg.Any<CancellationToken>())
            .Returns(EmptySummary());

        await ExecuteCommandAsync("--subscription", "sub1", "--group-by", "category");

        Assert.NotNull(captured);
        Assert.Null(captured!.Category);
        Assert.Null(captured.Impact);
        Assert.Null(captured.ResourceType);
        Assert.Null(captured.Resource);
        Assert.Null(captured.Search);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSummaryPayload()
    {
        var summary = new RecommendationSummary(
            GroupBy: "category",
            Top: 5,
            TotalRecommendations: 3,
            AreResultsTruncated: false,
            Groups:
            [
                new RecommendationGroup("Security", 2),
                new RecommendationGroup("Cost", 1),
            ]);

        Service.SummarizeRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<RecommendationFilters?>(),
            Arg.Any<CancellationToken>())
            .Returns(summary);

        var response = await ExecuteCommandAsync("--subscription", "sub1", "--group-by", "category");

        var result = ValidateAndDeserializeResponse(response, AdvisorJsonContext.Default.RecommendationSummaryResult);

        Assert.Equal("category", result.Summary.GroupBy);
        Assert.Equal(5, result.Summary.Top);
        Assert.Equal(3, result.Summary.TotalRecommendations);
        Assert.False(result.Summary.AreResultsTruncated);
        Assert.Equal(2, result.Summary.Groups.Count);
        Assert.Equal("Security", result.Summary.Groups[0].Key);
        Assert.Equal(2, result.Summary.Groups[0].Count);
    }

    [Fact]
    public async Task ExecuteAsync_PropagatesTruncationFlag()
    {
        Service.SummarizeRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<RecommendationFilters?>(),
            Arg.Any<CancellationToken>())
            .Returns(new RecommendationSummary("category", 5, 1000, true, []));

        var response = await ExecuteCommandAsync("--subscription", "sub1", "--group-by", "category");
        var result = ValidateAndDeserializeResponse(response, AdvisorJsonContext.Default.RecommendationSummaryResult);

        Assert.True(result.Summary.AreResultsTruncated);
        Assert.Equal(1000, result.Summary.TotalRecommendations);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_ReturnsErrorResponse()
    {
        Service.SummarizeRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<RecommendationFilters?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("boom"));

        var response = await ExecuteCommandAsync("--subscription", "sub1", "--group-by", "category");

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.Contains("boom", response.Message);
    }
}
