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
    private static RecommendationSummary EmptySummary(string groupBy = "category") =>
        new(GroupBy: groupBy, TotalRecommendations: 0, Groups: []);

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
    [InlineData("--subscription sub1 --group-by impact", true)]
    [InlineData("--subscription sub1", true)]                              // --group-by optional, defaults to category
    [InlineData("", false)]                                                // missing subscription
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.SummarizeRecommendationsAsync(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<string>(),
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
            Arg.Any<string>(), Arg.Any<RecommendationFilters?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_GroupByOmitted_DefaultsToCategory()
    {
        string? captured = null;
        Service.SummarizeRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Do<string>(g => captured = g),
            Arg.Any<RecommendationFilters?>(),
            Arg.Any<CancellationToken>())
            .Returns(EmptySummary());

        var response = await ExecuteCommandAsync("--subscription", "sub1");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("category", captured);
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
            Arg.Any<RecommendationFilters?>(),
            Arg.Any<CancellationToken>())
            .Returns(EmptySummary());

        var response = await ExecuteCommandAsync("--subscription", "sub1", "--group-by", raw);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("category", captured);
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
            TotalRecommendations: 3,
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
            Arg.Any<RecommendationFilters?>(),
            Arg.Any<CancellationToken>())
            .Returns(summary);

        var response = await ExecuteCommandAsync("--subscription", "sub1", "--group-by", "category");

        var result = ValidateAndDeserializeResponse(response, AdvisorJsonContext.Default.RecommendationSummaryResult);

        Assert.Equal("category", result.Summary.GroupBy);
        Assert.Equal(3, result.Summary.TotalRecommendations);
        Assert.Equal(2, result.Summary.Groups.Count);
        Assert.Equal("Security", result.Summary.Groups[0].Key);
        Assert.Equal(2, result.Summary.Groups[0].Count);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_ReturnsErrorResponse()
    {
        Service.SummarizeRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string>(),
            Arg.Any<RecommendationFilters?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("boom"));

        var response = await ExecuteCommandAsync("--subscription", "sub1", "--group-by", "category");

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.Contains("boom", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Top_SlicesGroupsButPreservesTotal()
    {
        var summary = new RecommendationSummary(
            GroupBy: "category",
            TotalRecommendations: 100,
            Groups:
            [
                new RecommendationGroup("Security", 50),
                new RecommendationGroup("Cost", 30),
                new RecommendationGroup("Performance", 15),
                new RecommendationGroup("HighAvailability", 5),
            ]);

        Service.SummarizeRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string>(),
            Arg.Any<RecommendationFilters?>(),
            Arg.Any<CancellationToken>())
            .Returns(summary);

        var response = await ExecuteCommandAsync("--subscription", "sub1", "--group-by", "category", "--top", "2");
        var result = ValidateAndDeserializeResponse(response, AdvisorJsonContext.Default.RecommendationSummaryResult);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal(2, result.Summary.Groups.Count);
        Assert.Equal("Security", result.Summary.Groups[0].Key);
        Assert.Equal("Cost", result.Summary.Groups[1].Key);
        // Total reflects the full filtered population, not the displayed slice.
        Assert.Equal(100, result.Summary.TotalRecommendations);
    }

    [Fact]
    public async Task ExecuteAsync_Top_AlwaysIncludesUnknownAtTail()
    {
        // Service returns Unknown last (KQL ordering). When --top would clip it, the
        // command must still surface it after the top-N real buckets.
        var summary = new RecommendationSummary(
            GroupBy: "resource-type",
            TotalRecommendations: 800,
            Groups:
            [
                new RecommendationGroup("microsoft.storage/storageaccounts", 291),
                new RecommendationGroup("microsoft.web/serverfarms", 114),
                new RecommendationGroup("microsoft.keyvault/vaults", 46),
                new RecommendationGroup("microsoft.machinelearningservices/workspaces", 32),
                new RecommendationGroup("microsoft.web/sites", 25),
                new RecommendationGroup("microsoft.compute/virtualmachines", 10),
                new RecommendationGroup("Unknown", 325),
            ]);

        Service.SummarizeRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string>(),
            Arg.Any<RecommendationFilters?>(),
            Arg.Any<CancellationToken>())
            .Returns(summary);

        var response = await ExecuteCommandAsync("--subscription", "sub1", "--group-by", "resource-type", "--top", "5");
        var result = ValidateAndDeserializeResponse(response, AdvisorJsonContext.Default.RecommendationSummaryResult);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        // 5 real buckets + Unknown appended at the tail = 6 entries.
        Assert.Equal(6, result.Summary.Groups.Count);
        Assert.Equal("Unknown", result.Summary.Groups[^1].Key);
        Assert.Equal(325, result.Summary.Groups[^1].Count);
        Assert.Equal("microsoft.storage/storageaccounts", result.Summary.Groups[0].Key);
        Assert.Equal(800, result.Summary.TotalRecommendations);
    }

    [Fact]
    public async Task ExecuteAsync_Top_LargerThanGroups_ReturnsAll()
    {
        var summary = new RecommendationSummary(
            GroupBy: "category",
            TotalRecommendations: 3,
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
            Arg.Any<RecommendationFilters?>(),
            Arg.Any<CancellationToken>())
            .Returns(summary);

        var response = await ExecuteCommandAsync("--subscription", "sub1", "--group-by", "category", "--top", "100");
        var result = ValidateAndDeserializeResponse(response, AdvisorJsonContext.Default.RecommendationSummaryResult);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal(2, result.Summary.Groups.Count);
        Assert.Equal(3, result.Summary.TotalRecommendations);
    }
}
