// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Advisor.Commands;
using Azure.Mcp.Tools.Advisor.Commands.Recommendation;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Recommendation;

public class RecommendationSummaryCommandTests
    : SubscriptionCommandUnitTestsBase<RecommendationSummaryCommand, IRecommendationSummaryService>
{
    private static RecommendationSummary EmptySummary(string groupBy = "category") =>
        new(groupBy, 0, []);

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("summary", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.ReadOnly);
    }

    [Theory]
    [InlineData("--subscription sub1 --group-by category", true)]
    [InlineData("--subscription sub1 --group-by impact", true)]
    [InlineData("--subscription sub1", true)]
    [InlineData("--subscription sub1 --group-by recommendation-type", true)]
    [InlineData("--subscription sub1 --group-by resource-type", true)]
    [InlineData("--subscription sub1 --group-by status", true)]
    [InlineData("--subscription sub1 --group-by sub-category", true)]
    [InlineData("--subscription sub1 --group-by retirement-date", true)]
    [InlineData("--subscription sub1 --category Security --impact High", true)]
    [InlineData("--subscription sub1 --recommendation-type-id 1d70919c-1a4a-4f79-8300-bb576c291e9d", true)]
    [InlineData("--subscription sub1 --retirement-date le:2026-12-31", true)]
    [InlineData("--subscription sub1 --group-by nonsense", false)]
    [InlineData("--subscription sub1 --group-by 999", false)]
    [InlineData("--subscription sub1 --category nonsense", false)]
    [InlineData("--subscription sub1 --category Sustainability", false)]
    [InlineData("--subscription sub1 --category 999", false)]
    [InlineData("--subscription sub1 --category \" \"", false)]
    [InlineData("--subscription sub1 --impact critical", false)]
    [InlineData("--subscription sub1 --impact 999", false)]
    [InlineData("--subscription sub1 --recommendation-type-id not-a-guid", false)]
    [InlineData("--subscription sub1 --top 0", false)]
    [InlineData("--subscription sub1 --top 101", false)]
    [InlineData("--subscription sub1 --retirement-date \" \"", false)]
    [InlineData("--subscription sub1 --retirement-date before:2026-12-31", false)]
    [InlineData("--subscription sub1 --retirement-date le:12-31-2026", false)]
    [InlineData("--subscription sub1 --sub-category \" \"", false)]
    [InlineData("--subscription sub1 --retirement-date le:2026-12-31 --sub-category ZoneResiliency", false)]
    [InlineData("--subscription sub1 --group-by retirement-date --sub-category ZoneResiliency", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.SummarizeRecommendationsAsync(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<AdvisorRecommendationGroupBy>(),
                Arg.Any<RecommendationFilters?>(),
                Arg.Any<string?>(),
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
        Assert.Contains("Must be one of", response.Message);
        await Service.DidNotReceive().SummarizeRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<AdvisorRecommendationGroupBy>(),
            Arg.Any<RecommendationFilters?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_GroupByOmitted_DefaultsToCategory()
    {
        AdvisorRecommendationGroupBy? captured = null;
        Service.SummarizeRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Do<AdvisorRecommendationGroupBy>(value => captured = value),
            Arg.Any<RecommendationFilters?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(EmptySummary());

        var response = await ExecuteCommandAsync("--subscription", "sub1");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal(AdvisorRecommendationGroupBy.Category, captured);
    }

    [Theory]
    [InlineData("recommendation-type", AdvisorRecommendationGroupBy.RecommendationType)]
    [InlineData("category", AdvisorRecommendationGroupBy.Category)]
    [InlineData("Category", AdvisorRecommendationGroupBy.Category)]
    [InlineData("  category  ", AdvisorRecommendationGroupBy.Category)]
    [InlineData("impact", AdvisorRecommendationGroupBy.Impact)]
    [InlineData("resource-type", AdvisorRecommendationGroupBy.ResourceType)]
    [InlineData("status", AdvisorRecommendationGroupBy.Status)]
    [InlineData("sub-category", AdvisorRecommendationGroupBy.SubCategory)]
    [InlineData("retirement-date", AdvisorRecommendationGroupBy.RetirementDate)]
    [InlineData("  RETIREMENT-DATE  ", AdvisorRecommendationGroupBy.RetirementDate)]
    public async Task ExecuteAsync_GroupBy_BindsEnum(string raw, AdvisorRecommendationGroupBy expected)
    {
        AdvisorRecommendationGroupBy? captured = null;
        Service.SummarizeRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Do<AdvisorRecommendationGroupBy>(value => captured = value),
            Arg.Any<RecommendationFilters?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(EmptySummary());

        var response = await ExecuteCommandAsync("--subscription", "sub1", "--group-by", raw);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal(expected, captured);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsNormalizedFilters()
    {
        RecommendationFilters? captured = null;
        Service.SummarizeRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<AdvisorRecommendationGroupBy>(),
            Arg.Do<RecommendationFilters?>(value => captured = value),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(EmptySummary("impact"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--group-by", " Impact ",
            "--category", " security ",
            "--impact", " high ",
            "--recommendation-type-id", "1D70919C-1A4A-4F79-8300-BB576C291E9D",
            "--resource-type", " Microsoft.Web/sites ",
            "--resource", " webapp ",
            "--search", " encrypt ",
            "--sub-category", " ServiceUpgradeAndRetirement ",
            "--retirement-date", "ge:2026-03-31");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(captured);
        Assert.Equal(AdvisorRecommendationCategory.Security, captured!.Category);
        Assert.Equal(AdvisorRecommendationImpact.High, captured.Impact);
        Assert.Equal("1d70919c-1a4a-4f79-8300-bb576c291e9d", captured.RecommendationTypeId);
        Assert.Equal("Microsoft.Web/sites", captured.ResourceType);
        Assert.Equal("webapp", captured.Resource);
        Assert.Equal("encrypt", captured.Search);
        Assert.Equal("ServiceUpgradeAndRetirement", captured.SubCategory);
        Assert.Equal("ge", captured.RetirementDateOperator);
        Assert.Equal(new DateOnly(2026, 3, 31), captured.RetirementDate);
    }

    [Fact]
    public async Task ExecuteAsync_OmittedFilters_ForwardsNullValues()
    {
        RecommendationFilters? captured = null;
        Service.SummarizeRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<AdvisorRecommendationGroupBy>(),
            Arg.Do<RecommendationFilters?>(value => captured = value),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(EmptySummary());

        await ExecuteCommandAsync("--subscription", "sub1", "--group-by", "category");

        Assert.NotNull(captured);
        Assert.Null(captured!.Category);
        Assert.Null(captured.Impact);
        Assert.Null(captured.RecommendationTypeId);
        Assert.Null(captured.ResourceType);
        Assert.Null(captured.Resource);
        Assert.Null(captured.Search);
        Assert.Null(captured.SubCategory);
        Assert.Null(captured.RetirementDateOperator);
        Assert.Null(captured.RetirementDate);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsKeyLabelCountPayload()
    {
        Service.SummarizeRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<AdvisorRecommendationGroupBy>(),
            Arg.Any<RecommendationFilters?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new RecommendationSummary(
                "recommendation-type",
                3,
                [
                    new(
                        "42dbf883-9e4b-4f84-9da4-232b87c4b5e9",
                        "Enable Soft Delete",
                        3),
                ]));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--group-by", "recommendation-type");
        var result = ValidateAndDeserializeResponse(
            response,
            AdvisorJsonContext.Default.RecommendationSummaryResult);

        var group = Assert.Single(result.Summary.Groups);
        Assert.Equal("42dbf883-9e4b-4f84-9da4-232b87c4b5e9", group.Key);
        Assert.Equal("Enable Soft Delete", group.Label);
        Assert.Equal(3, group.Count);
        Assert.Equal(3, result.Summary.TotalRecommendations);
    }

    [Fact]
    public async Task ExecuteAsync_TopPreservesUnknownAndTotal()
    {
        Service.SummarizeRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<AdvisorRecommendationGroupBy>(),
            Arg.Any<RecommendationFilters?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new RecommendationSummary(
                "resource-type",
                100,
                [
                    new("microsoft.web/sites", "microsoft.web/sites", 50),
                    new("microsoft.storage/storageaccounts", "microsoft.storage/storageaccounts", 30),
                    new("microsoft.keyvault/vaults", "microsoft.keyvault/vaults", 15),
                    new("Unknown", "Unknown", 5),
                ]));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--group-by", "resource-type",
            "--top", "2");
        var result = ValidateAndDeserializeResponse(
            response,
            AdvisorJsonContext.Default.RecommendationSummaryResult);

        Assert.Equal(3, result.Summary.Groups.Count);
        Assert.Equal("microsoft.web/sites", result.Summary.Groups[0].Key);
        Assert.Equal("microsoft.storage/storageaccounts", result.Summary.Groups[1].Key);
        Assert.Equal("Unknown", result.Summary.Groups[2].Key);
        Assert.Equal(100, result.Summary.TotalRecommendations);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_ReturnsErrorResponse()
    {
        Service.SummarizeRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<AdvisorRecommendationGroupBy>(),
            Arg.Any<RecommendationFilters?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("boom"));

        var response = await ExecuteCommandAsync("--subscription", "sub1");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.Status);
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
                new RecommendationGroup("Security", "Security", 50),
                new RecommendationGroup("Cost", "Cost", 30),
                new RecommendationGroup("Performance", "Performance", 15),
                new RecommendationGroup("HighAvailability", "HighAvailability", 5),
            ]);

        Service.SummarizeRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<AdvisorRecommendationGroupBy>(),
            Arg.Any<RecommendationFilters?>(),
            Arg.Any<string?>(),
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
    public async Task ExecuteAsync_Top_LargerThanGroups_ReturnsAll()
    {
        var summary = new RecommendationSummary(
            GroupBy: "category",
            TotalRecommendations: 3,
            Groups:
            [
                new RecommendationGroup("Security", "Security", 2),
                new RecommendationGroup("Cost", "Cost", 1),
            ]);

        Service.SummarizeRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<AdvisorRecommendationGroupBy>(),
            Arg.Any<RecommendationFilters?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(summary);

        var response = await ExecuteCommandAsync("--subscription", "sub1", "--group-by", "category", "--top", "100");
        var result = ValidateAndDeserializeResponse(response, AdvisorJsonContext.Default.RecommendationSummaryResult);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal(2, result.Summary.Groups.Count);
        Assert.Equal(3, result.Summary.TotalRecommendations);
    }
}
