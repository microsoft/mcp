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
    [InlineData("--subscription sub1", true)]
    [InlineData("--subscription sub1 --group-by recommendation-type", true)]
    [InlineData("--subscription sub1 --group-by status", true)]
    [InlineData("--subscription sub1 --group-by sub-category", true)]
    [InlineData("--subscription sub1 --group-by retirement-date", true)]
    [InlineData("--subscription sub1 --category Security --impact High", true)]
    [InlineData("--subscription sub1 --recommendation-type-id 1d70919c-1a4a-4f79-8300-bb576c291e9d", true)]
    [InlineData("--subscription sub1 --retirement-date le:2026-12-31", true)]
    [InlineData("--subscription sub1 --group-by nonsense", false)]
    [InlineData("--subscription sub1 --category nonsense", false)]
    [InlineData("--subscription sub1 --category \" \"", false)]
    [InlineData("--subscription sub1 --impact critical", false)]
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
                Arg.Any<string>(),
                Arg.Any<RecommendationFilters?>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
                .Returns(EmptySummary());
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_GroupByOmitted_DefaultsToCategory()
    {
        string? captured = null;
        Service.SummarizeRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Do<string>(value => captured = value),
            Arg.Any<RecommendationFilters?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(EmptySummary());

        var response = await ExecuteCommandAsync("--subscription", "sub1");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("category", captured);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsNormalizedFilters()
    {
        RecommendationFilters? captured = null;
        Service.SummarizeRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
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
        Assert.Equal("Security", captured!.Category);
        Assert.Equal("High", captured.Impact);
        Assert.Equal("1d70919c-1a4a-4f79-8300-bb576c291e9d", captured.RecommendationTypeId);
        Assert.Equal("Microsoft.Web/sites", captured.ResourceType);
        Assert.Equal("webapp", captured.Resource);
        Assert.Equal("encrypt", captured.Search);
        Assert.Equal("ServiceUpgradeAndRetirement", captured.SubCategory);
        Assert.Equal("ge", captured.RetirementDateOperator);
        Assert.Equal(new DateOnly(2026, 3, 31), captured.RetirementDate);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsKeyLabelCountPayload()
    {
        Service.SummarizeRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
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
            Arg.Any<string>(),
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
            Arg.Any<string>(),
            Arg.Any<RecommendationFilters?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("boom"));

        var response = await ExecuteCommandAsync("--subscription", "sub1");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.Status);
        Assert.Contains("boom", response.Message);
    }
}
