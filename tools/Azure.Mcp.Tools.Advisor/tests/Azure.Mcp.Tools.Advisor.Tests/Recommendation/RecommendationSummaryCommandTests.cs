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
        Assert.Contains("--service-group", command.Description);
        Assert.Contains("--category Cost --sub-category IdleResources", command.Description);
        Assert.DoesNotContain("Preserve the returned Retail pricing basis", command.Description);
        Assert.Equal(typeof(string), Assert.Single(command.Options, static option => option.Name == "--service-group").ValueType);
        Assert.Equal(typeof(string[]), Assert.Single(command.Options, static option => option.Name == "--subscriptions").ValueType);
        Assert.DoesNotContain(command.Options, static option => option.Name == "--service-groups");
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
    [InlineData("--service-group sg1", true)]
    [InlineData("--service-group sg1 --tenant tenant1 --group-by status", true)]
    [InlineData("--subscription sub1 --prioritized true", true)]
    [InlineData("--subscription sub1 --prioritized true --group-by recommendation-type --top 5", true)]
    [InlineData("--subscription sub1 --prioritized true --category Cost", true)]
    [InlineData("--subscription sub1 --prioritized true --product-family Compute", true)]
    [InlineData("--service-group sg1 --prioritized true --product-family Storage --status Completed", true)]
    [InlineData("--subscriptions sub1 sub2 --prioritized true --product-family Networking", true)]
    [InlineData("--subscription sub1 --product-family Analytics", true)]
    [InlineData("--service-group sg1 --product-family \"AI + Machine Learning\"", true)]
    [InlineData("--subscription sub1 --product-family \"\"", false)]
    [InlineData("--service-group sg1 --product-family \" \"", false)]
    [InlineData("--service-group sg1 --prioritized true", true)]
    [InlineData("--subscriptions sub1 sub2 --prioritized true", true)]
    [InlineData("--subscription sub1 --prioritized false --group-by impact", true)]
    [InlineData("--subscription sub1 --category Cost --from 2020-09-10T00:00:00Z --as-of 2020-09-01T00:00:00Z", false)]
    [InlineData("--subscription sub1 --category Cost --from 2020-09-01T00:00:00Z --as-of 2020-09-01T00:00:00Z", true)]
    [InlineData("--subscription sub1 --category Cost --from 2020-09-01T00:00:00Z --as-of 2020-09-10T00:00:00Z", true)]
    [InlineData("--subscription sub1 --category Cost --from 2020-09-10T00:00:00Z --to 2020-09-30T00:00:00Z --as-of 2020-09-01T00:00:00Z", true)]
    [InlineData("--subscription sub1 --category Cost --from 2020-09-10T00:00:00Z --to 2020-09-01T00:00:00Z --as-of 2020-09-30T00:00:00Z", false)]
    [InlineData("--subscription sub1 --category Cost --from 2020-09-01T00:00:00Z", true)]
    [InlineData("--subscription sub1 --category Cost --to 2020-09-30T00:00:00Z", true)]
    [InlineData("--subscription sub1 --prioritized true --group-by category", false)]
    [InlineData("--subscription sub1 --prioritized true --group-by status", false)]
    [InlineData("--subscription sub1 --prioritized true --top 101", false)]
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
    [InlineData("--subscription sub1 --service-group sg1", false)]
    [InlineData("--subscriptions sub1 sub2 --service-group sg1", false)]
    [InlineData("--service-group sg1 --resource-group rg1", false)]
    [InlineData("--service-group bad/name", false)]
    [InlineData("--service-group \" \"", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.SummarizeServiceGroupRecommendationsAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RecommendationFilters?>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
                .Returns(EmptySummary());
            Service.SummarizeRecommendationsAsync(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string>(),
                Arg.Any<RecommendationFilters?>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
                .Returns(EmptySummary());
            Service.SummarizeRecommendationsAsync(
                Arg.Any<IReadOnlyCollection<string>>(),
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
    public async Task ExecuteAsync_WithoutScope_UsesDefaultSubscription()
    {
        SubscriptionResolver.ResolveSubscription(null).Returns("default-subscription");
        Service.SummarizeRecommendationsAsync(
            "default-subscription",
            null,
            "category",
            Arg.Any<RecommendationFilters?>(),
            null,
            Arg.Any<CancellationToken>())
            .Returns(EmptySummary());

        var response = await ExecuteCommandAsync("");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).SummarizeRecommendationsAsync(
            "default-subscription",
            null,
            "category",
            Arg.Any<RecommendationFilters?>(),
            null,
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("service-group-test")]
    [InlineData("Trey Research Shared Services")]
    [InlineData("/providers/Microsoft.Management/serviceGroups/service-group-test")]
    public async Task ExecuteAsync_ServiceGroup_ForwardsScopeWithoutResolvingSubscription(string serviceGroup)
    {
        Service.SummarizeServiceGroupRecommendationsAsync(
            serviceGroup,
            "impact",
            Arg.Any<RecommendationFilters?>(),
            "tenant1",
            Arg.Any<CancellationToken>())
            .Returns(EmptySummary("impact"));

        var response = await ExecuteCommandAsync(
            "--service-group", $" {serviceGroup} ",
            "--tenant", "tenant1",
            "--group-by", "Impact");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).SummarizeServiceGroupRecommendationsAsync(
            serviceGroup,
            "impact",
            Arg.Any<RecommendationFilters?>(),
            "tenant1",
            Arg.Any<CancellationToken>());
        SubscriptionResolver.DidNotReceive().ResolveSubscription(Arg.Any<string?>());
        await Service.DidNotReceive().SummarizeRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<RecommendationFilters?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_BlankServiceGroup_DoesNotFallBackToDefaultSubscription()
    {
        SubscriptionResolver.ResolveSubscription(null).Returns("default-subscription");

        var response = await ExecuteCommandAsync("--service-group", " ");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        SubscriptionResolver.DidNotReceive().ResolveSubscription(Arg.Any<string?>());
        await Service.DidNotReceive().SummarizeRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<RecommendationFilters?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
        await Service.DidNotReceive().SummarizeServiceGroupRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RecommendationFilters?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("sub1", "")]
    [InlineData("sub1", " ")]
    [InlineData("", "sg1")]
    [InlineData(" ", "sg1")]
    public async Task ExecuteAsync_BothScopesExplicitlyProvided_RejectsBlankValues(
        string subscription,
        string serviceGroup)
    {
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--service-group", serviceGroup);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Specify either --subscription or --service-group, not both", response.Message);
        SubscriptionResolver.DidNotReceive().ResolveSubscription(Arg.Any<string?>());
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
    public async Task ExecuteAsync_CombinesScalarAndPluralFiltersAndPreservesStatus()
    {
        RecommendationFilters? captured = null;
        Service.SummarizeRecommendationsAsync(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(),
            Arg.Do<RecommendationFilters?>(value => captured = value),
            Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(EmptySummary());

        var response = await ExecuteCommandAsync(
            "--subscription", "sub1", "--status", "Completed",
            "--recommendation-type-id", "1D70919C-1A4A-4F79-8300-BB576C291E9D",
            "--recommendation-type-ids", "2D70919C-1A4A-4F79-8300-BB576C291E9D", "1d70919c-1a4a-4f79-8300-bb576c291e9d",
            "--resource-type", "Microsoft.Compute/virtualMachines",
            "--resource-types", "Microsoft.Web/sites", "microsoft.compute/virtualmachines");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(captured);
        Assert.Equal(RecommendationStatus.Completed, captured.Status);
        Assert.Null(captured.RecommendationTypeId);
        Assert.Equal(
            ["1d70919c-1a4a-4f79-8300-bb576c291e9d", "2d70919c-1a4a-4f79-8300-bb576c291e9d"],
            captured.RecommendationTypeIds);
        Assert.Null(captured.ResourceType);
        Assert.Equal(["Microsoft.Compute/virtualMachines", "Microsoft.Web/sites"], captured.ResourceTypes);
        Assert.Equal(typeof(string), Assert.Single(Command.GetCommand().Options,
            static option => option.Name == "--recommendation-type-id").ValueType);
        Assert.Equal(typeof(string), Assert.Single(Command.GetCommand().Options,
            static option => option.Name == "--resource-type").ValueType);
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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExecuteAsync_TopDoesNotTrimCostTotalsOrCalculationMetadata(bool prioritized)
    {
        var from = new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 9, 30, 23, 59, 59, TimeSpan.Zero);
        var cost = new RecommendationCostSummary(
            "Retail", 12, 12, 0, 0,
            [
                new("USD", 100, 20, null, 10, 10, 10, 0),
                new("EUR", 50, null, null, 2, 2, 0, 0),
            ],
            AsOfUtc: to, FromUtc: from, ToUtc: to,
            StatusTotals: [new("New", 12)],
            TypeTotals:
            [
                new("type-a", "A", "USD", "Retail", 60, 12, null, 6, 6, 6, 0),
                new("type-b", "B", "USD", "Retail", 40, 8, null, 4, 4, 4, 0),
                new("type-c", "C", "EUR", "Retail", 50, null, null, 2, 2, 0, 0),
            ]);
        Service.SummarizeRecommendationsAsync(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(),
            Arg.Any<RecommendationFilters?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new RecommendationSummary(
                "recommendation-type", 12,
                [new("type-a", "A", 6), new("type-b", "B", 4), new("type-c", "C", 2)],
                Cost: cost));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub1", "--category", "Cost", "--group-by", "recommendation-type",
            "--prioritized", prioritized.ToString(), "--top", "1",
            "--as-of", "2026-09-30T23:59:59Z", "--from", "2026-09-01T00:00:00Z", "--to", "2026-09-30T23:59:59Z");
        var result = ValidateAndDeserializeResponse(response, AdvisorJsonContext.Default.RecommendationSummaryResult);

        Assert.Single(result.Summary.Groups);
        Assert.Equal(12, result.Summary.TotalRecommendations);
        Assert.Equal(3, result.Summary.TotalRecommendationGroupCount);
        Assert.Equal(2, result.Summary.RemainingRecommendationGroupCount);
        Assert.Equal(
            System.Text.Json.JsonSerializer.Serialize(cost, AdvisorJsonContext.Default.RecommendationCostSummary),
            System.Text.Json.JsonSerializer.Serialize(result.Summary.Cost, AdvisorJsonContext.Default.RecommendationCostSummary));
    }

    [Theory]
    [InlineData(false, RecommendationStatus.New)]
    [InlineData(false, RecommendationStatus.Postponed)]
    [InlineData(false, RecommendationStatus.Dismissed)]
    [InlineData(false, RecommendationStatus.Completed)]
    [InlineData(true, RecommendationStatus.New)]
    [InlineData(true, RecommendationStatus.Postponed)]
    [InlineData(true, RecommendationStatus.Dismissed)]
    [InlineData(true, RecommendationStatus.Completed)]
    public async Task ExecuteAsync_IdleResourcesTopPreservesSavingsOrderAndFullCounts(
        bool prioritized, RecommendationStatus status)
    {
        Service.SummarizeRecommendationsAsync(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(),
            Arg.Any<RecommendationFilters?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new RecommendationSummary(
                "recommendation-type", 101,
                [new("highest-savings", "Highest savings", 1), new("most-records", "Most records", 100)]));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub1", "--category", "Cost", "--sub-category", "IdleResources",
            "--group-by", "recommendation-type", "--status", status.ToString(),
            "--prioritized", prioritized.ToString(), "--top", "1");
        var result = ValidateAndDeserializeResponse(response, AdvisorJsonContext.Default.RecommendationSummaryResult);

        Assert.Equal("highest-savings", Assert.Single(result.Summary.Groups).Key);
        Assert.Equal(101, result.Summary.TotalRecommendations);
        Assert.Equal(2, result.Summary.TotalRecommendationGroupCount);
        Assert.Equal(1, result.Summary.RemainingRecommendationGroupCount);
    }

    [Fact]
    public async Task ExecuteAsync_Prioritized_ImpliesRecommendationTypeAndDefaultTopTen()
    {
        string? capturedGroupBy = null;
        RecommendationFilters? capturedFilters = null;
        var groups = Enumerable.Range(1, 12)
            .Select(index => new RecommendationGroup($"type-{index:D2}", $"Type {index}", 1))
            .ToList();
        Service.SummarizeRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Do<string>(value => capturedGroupBy = value),
            Arg.Do<RecommendationFilters?>(value => capturedFilters = value),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new RecommendationSummary("recommendation-type", 12, groups));

        var response = await ExecuteCommandAsync("--subscription", "sub1", "--prioritized", "true");
        var result = ValidateAndDeserializeResponse(
            response,
            AdvisorJsonContext.Default.RecommendationSummaryResult);

        Assert.Equal("recommendation-type", capturedGroupBy);
        Assert.True(capturedFilters?.Prioritized);
        Assert.Equal(10, result.Summary.Groups.Count);
        Assert.Equal(12, result.Summary.TotalRecommendations);
        Assert.Equal("type-01", result.Summary.Groups[0].Key);
        Assert.Equal("type-10", result.Summary.Groups[^1].Key);
    }

    [Fact]
    public async Task ExecuteAsync_PrioritizedFalse_KeepsExistingDefaults()
    {
        string? capturedGroupBy = null;
        RecommendationFilters? capturedFilters = null;
        Service.SummarizeRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Do<string>(value => capturedGroupBy = value),
            Arg.Do<RecommendationFilters?>(value => capturedFilters = value),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(EmptySummary());

        var response = await ExecuteCommandAsync("--subscription", "sub1", "--prioritized", "false");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("category", capturedGroupBy);
        Assert.Null(capturedFilters?.Prioritized);
    }

    [Theory]
    [InlineData("--subscription sub1", " compute ")]
    [InlineData("--subscriptions sub1 sub2", " Analytics ")]
    [InlineData("--service-group sg1", " AI + Machine Learning ")]
    public async Task ExecuteAsync_PrioritizedProductFamilyPreservesStateAndDateFilters(string scope, string family)
    {
        RecommendationFilters? captured = null;
        Service.SummarizeRecommendationsAsync(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(),
            Arg.Do<RecommendationFilters?>(filters => captured = filters),
            Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(EmptySummary("recommendation-type"));
        Service.SummarizeRecommendationsAsync(
            Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<string?>(), Arg.Any<string>(),
            Arg.Do<RecommendationFilters?>(filters => captured = filters),
            Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(EmptySummary("recommendation-type"));
        Service.SummarizeServiceGroupRecommendationsAsync(
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Do<RecommendationFilters?>(filters => captured = filters),
            Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(EmptySummary("recommendation-type"));

        var response = await ExecuteCommandAsync(
            $"{scope} --category Cost --sub-category IdleResources " +
            $"--product-family \"{family}\" " +
            "--prioritized true --status Postponed --as-of 2026-09-30T00:00:00Z --top 1");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(captured);
        Assert.Equal(family.Trim(), captured.ProductFamily);
        Assert.True(captured.Prioritized);
        Assert.Equal(RecommendationStatus.Postponed, captured.Status);
        Assert.Equal(new DateTimeOffset(2026, 9, 30, 0, 0, 0, TimeSpan.Zero), captured.SavingsAsOf);
    }

    [Fact]
    public async Task ExecuteAsync_PrioritizedTop_KeepsRankedOrderWithoutMovingUnknown()
    {
        Service.SummarizeServiceGroupRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RecommendationFilters?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new RecommendationSummary(
                "recommendation-type",
                9,
                [
                    new("type-a", "A", 4),
                    new("Unknown", "Unknown", 3),
                    new("type-b", "B", 2),
                ]));

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--prioritized", "true",
            "--top", "2");
        var result = ValidateAndDeserializeResponse(
            response,
            AdvisorJsonContext.Default.RecommendationSummaryResult);

        Assert.Equal(["type-a", "Unknown"], result.Summary.Groups.Select(group => group.Key));
        Assert.Equal(9, result.Summary.TotalRecommendations);
    }

    [Fact]
    public async Task ExecuteAsync_Prioritized_SerializesSignalsAndOmitsAbsentFields()
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
                5,
                [
                    new(
                        "type-a",
                        "Retiring feature",
                        3,
                        new RecommendationPrioritizationSignals
                        {
                            Category = "HighAvailability",
                            Impact = "High",
                            SubCategory = "ServiceUpgradeAndRetirement",
                            RetirementDate = "2025-02-28",
                            RetirementWindow = "retirementPastDue",
                            CriticalService = "qcs",
                        }),
                    new(
                        "type-b",
                        "Idle plan",
                        2,
                        new RecommendationPrioritizationSignals
                        {
                            Category = "Cost",
                            Impact = "Medium",
                            AnnualSavings = 1200.5,
                            AnnualSavingsCurrency = "USD",
                        }),
                ]));

        var response = await ExecuteCommandAsync("--subscription", "sub1", "--prioritized", "true");
        var json = System.Text.Json.JsonSerializer.Serialize(response.Results);
        var result = ValidateAndDeserializeResponse(
            response,
            AdvisorJsonContext.Default.RecommendationSummaryResult);

        Assert.Contains("\"prioritizationSignals\"", json);
        Assert.Contains("\"retirementWindow\":\"retirementPastDue\"", json);
        Assert.DoesNotContain("\"effort\"", json);
        Assert.DoesNotContain("priorityScore", json, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("qcs", result.Summary.Groups[0].PrioritizationSignals?.CriticalService);
        Assert.Null(result.Summary.Groups[0].PrioritizationSignals?.AnnualSavings);
        Assert.Equal(1200.5, result.Summary.Groups[1].PrioritizationSignals?.AnnualSavings);
        Assert.Equal("USD", result.Summary.Groups[1].PrioritizationSignals?.AnnualSavingsCurrency);
        Assert.Null(result.Summary.Groups[1].PrioritizationSignals?.SubCategory);
    }

    [Fact]
    public async Task ExecuteAsync_NotPrioritized_OmitsPrioritizationSignals()
    {
        Service.SummarizeRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<RecommendationFilters?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new RecommendationSummary("category", 2, [new("Cost", "Cost", 2)]));

        var response = await ExecuteCommandAsync("--subscription", "sub1");
        var json = System.Text.Json.JsonSerializer.Serialize(response.Results);

        Assert.DoesNotContain("prioritizationSignals", json);
    }
}
