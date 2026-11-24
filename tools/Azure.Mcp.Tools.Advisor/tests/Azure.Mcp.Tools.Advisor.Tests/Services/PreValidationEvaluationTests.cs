// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Advisor.Commands;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Azure.Mcp.Tools.Monitor.Models;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Services;

public sealed class PreValidationEvaluationTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 30, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void BindResourceIds_ReusesEscapingWithoutChangingLiteralQuotes()
    {
        const string query = "resources | where id in~ (<RESOURCE_IDS>)";
        var bound = PreValidationService.BindResourceIds(query, ["Owner's\\disk", "DISK2"]);

        Assert.Equal("resources | where id in~ ('owner\\'s\\\\disk', 'disk2')", bound);
    }

    [Theory]
    [InlineData("rows", "block", 0, "failed")]
    [InlineData("rows", "block", 1, "passed")]
    [InlineData("noRows", "block", 0, "passed")]
    [InlineData("noRows", "block", 1, "failed")]
    [InlineData("noRows", "warn", 1, "warning")]
    [InlineData("info", "info", 0, "info")]
    public void EvaluateArgCheck_UsesInsightExpectations(string expect, string severity, int count, string outcome)
    {
        var check = new ResourceInsightCheck("check", "Check", "risk", "arg", severity, "Evidence", Expect: expect);
        var row = JsonDocument.Parse("""{"key":"resource"}""").RootElement.Clone();

        var result = PreValidationService.EvaluateArgCheck(check, Enumerable.Repeat(row, count).ToArray());

        Assert.Equal(outcome, result.Outcome);
        Assert.Equal(count, result.Data?.Count);
    }

    [Theory]
    [InlineData("""{"state":"Unattached"}""", "passed")]
    [InlineData("""{"state":"Attached"}""", "failed")]
    [InlineData("""{"state":null}""", "unverified")]
    [InlineData("""{"state":true}""", "unverified")]
    [InlineData("""{}""", "unverified")]
    public void EvaluateArgCheck_MissingOrWrongTypedFieldsAreUnverified(string json, string outcome)
    {
        var check = new ResourceInsightCheck("state", "State", "risk", "arg", "block", "State evidence",
            Expect: "field", Field: "state", ExpectedKind: JsonValueKind.String, ExpectedValue: "Unattached");
        using var document = JsonDocument.Parse(json);

        Assert.Equal(outcome, PreValidationService.EvaluateArgCheck(check, [document.RootElement]).Outcome);
        Assert.Equal("unverified", PreValidationService.EvaluateArgCheck(check, []).Outcome);
    }

    [Theory]
    [InlineData("""{"safe":true}""", "passed")]
    [InlineData("""{"safe":false}""", "failed")]
    [InlineData("""{"safe":1}""", "passed")]
    [InlineData("""{"safe":0}""", "failed")]
    [InlineData("""{"safe":2}""", "unverified")]
    [InlineData("""{"safe":-1}""", "unverified")]
    [InlineData("""{"safe":0.5}""", "unverified")]
    [InlineData("""{"safe":"true"}""", "unverified")]
    [InlineData("""{"safe":"1"}""", "unverified")]
    public void EvaluateArgCheck_BooleanComparisonsRetainTheirType(string json, string outcome)
    {
        var check = new ResourceInsightCheck("state", "State", "risk", "arg", "block", "State evidence",
            Expect: "field", Field: "safe", ExpectedKind: JsonValueKind.True, ExpectedValue: "true");
        using var document = JsonDocument.Parse(json);

        Assert.Equal(outcome, PreValidationService.EvaluateArgCheck(check, [document.RootElement]).Outcome);
    }

    [Theory]
    [InlineData("""{"safe":false}""", "passed")]
    [InlineData("""{"safe":0}""", "passed")]
    [InlineData("""{"safe":1}""", "failed")]
    [InlineData("""{"safe":2}""", "unverified")]
    public void EvaluateArgCheck_NumericBooleansAlsoSupportFalseExpectations(string json, string outcome)
    {
        var check = new ResourceInsightCheck("state", "State", "risk", "arg", "block", "State evidence",
            Expect: "field", Field: "safe", ExpectedKind: JsonValueKind.False, ExpectedValue: "false");
        using var document = JsonDocument.Parse(json);

        Assert.Equal(outcome, PreValidationService.EvaluateArgCheck(check, [document.RootElement]).Outcome);
    }

    [Fact]
    public void EvaluateMetricActivity_RequiresEveryRequestedMetric()
    {
        var signal = new UnusedAgeSignal("metrics", MetricNames: ["BytesIn", "BytesOut"]);
        var observation = PreValidationService.EvaluateMetricActivity(signal, [Metric("BytesIn")], Now.AddDays(-31), Now);

        Assert.True(observation.IsUnknown);
        var complete = PreValidationService.EvaluateMetricActivity(signal, [Metric("BytesIn"), Metric("BytesOut")], Now.AddDays(-31), Now);
        Assert.False(complete.IsUnknown);
        Assert.Equal(31, complete.QuietWindowDays);
    }

    [Theory]
    [InlineData("Maximum")]
    [InlineData("Average")]
    public void EvaluateMetricActivity_UsesTheDeclaredAggregation(string aggregation)
    {
        var metric = Metric("Usage");
        var values = new double[32];
        values[12] = 10;
        if (aggregation == "Maximum")
        {
            metric.TimeSeries[0].MaxBuckets = values;
        }
        else
        {
            metric.TimeSeries[0].AvgBuckets = values;
        }

        var observation = PreValidationService.EvaluateMetricActivity(
            new("metrics", MetricNames: ["Usage"], Aggregation: aggregation), [metric], Now.AddDays(-31), Now);

        Assert.False(observation.IsUnknown);
        Assert.Equal(Now.AddDays(-18), observation.LastActivityAt);
    }

    [Fact]
    public void EvaluateMetricActivity_RequiresEverySeriesAndDoesNotInventEmptySignals()
    {
        var metric = Metric("Usage");
        metric.TimeSeries.Add(new() { Start = Now.AddDays(-31).UtcDateTime, End = Now.UtcDateTime, Interval = "P1D" });

        Assert.True(PreValidationService.EvaluateMetricActivity(
            new("metrics", MetricNames: ["Usage"]), [metric], Now.AddDays(-31), Now).IsUnknown);
        Assert.Throws<InvalidOperationException>(() => PreValidationService.EvaluateMetricActivity(
            new("metrics"), [], Now.AddDays(-31), Now));
    }

    [Theory]
    [InlineData("empty")]
    [InlineData("short")]
    [InlineData("gap")]
    [InlineData("wrong-aggregation")]
    [InlineData("wrong-interval")]
    [InlineData("non-finite")]
    [InlineData("negative")]
    public void EvaluateMetricActivity_IncompleteTelemetryIsNotZeroUsage(string scenario)
    {
        var metric = Metric("BytesIn");
        var series = metric.TimeSeries[0];
        switch (scenario)
        {
            case "empty":
                series.TotalBuckets = [];
                break;
            case "short":
                series.Start = Now.AddDays(-14).UtcDateTime;
                series.TotalBuckets = new double[15];
                break;
            case "gap":
                series.TotalBuckets = new double[30];
                break;
            case "wrong-aggregation":
                series.TotalBuckets = null;
                series.AvgBuckets = new double[32];
                break;
            case "wrong-interval":
                series.Interval = "PT1H";
                break;
            case "non-finite":
                series.TotalBuckets![0] = double.NaN;
                break;
            case "negative":
                series.TotalBuckets![0] = -1;
                break;
        }
        var observation = PreValidationService.EvaluateMetricActivity(
            new("metrics", MetricNames: ["BytesIn"]), [metric], Now.AddDays(-31), Now);

        Assert.True(observation.IsUnknown);
    }

    [Fact]
    public void EvaluateMetricActivity_UsesTheEndOfTheLatestActiveDailyBucket()
    {
        var metric = Metric("BytesIn");
        metric.TimeSeries[0].TotalBuckets![12] = 10;
        var observation = PreValidationService.EvaluateMetricActivity(
            new("metrics", MetricNames: ["BytesIn"]), [metric], Now.AddDays(-31), Now);

        Assert.False(observation.IsUnknown);
        Assert.Equal(Now.AddDays(-18), observation.LastActivityAt);
    }

    [Fact]
    public void GetVerdict_AgeAndUnknownChecksNeverOverrideAConfirmedBlock()
    {
        var blocked = new PreValidationCheckResult("locks", "Locks", "risk", "block", "failed", "Locked");
        var age = UnusedAgePolicy.Evaluate([default], Now, 30);

        Assert.Equal("blocked", PreValidationService.GetVerdict([age, blocked]));
        Assert.Equal("blocked", PreValidationService.GetVerdict([blocked, age]));
        Assert.Equal("needsReview", PreValidationService.GetVerdict([age]));
        Assert.Equal("needsReview", PreValidationService.GetVerdict([]));
    }

    [Fact]
    public void GetVerdict_InformationalAndManualChecksDoNotClaimAutomatedCompletion()
    {
        var passed = new PreValidationCheckResult("detection", "Detection", "scope", "block", "passed", "Idle");
        var manual = new PreValidationCheckResult("backup", "Backup", "risk", "info", "manual", "Requires owner confirmation");
        var info = new PreValidationCheckResult("savings", "Savings", "savings", "info", "unverified", "No pricing");

        Assert.Equal("safe", PreValidationService.GetVerdict([passed, manual, info]));
        Assert.Equal("manual", manual.Outcome);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void BuildResult_EmptyOrIncompleteSelectionsCannotClaimSafe(bool truncated, bool unmatched)
    {
        var result = PreValidationService.BuildResult("type", [], Now, 30, truncated, unmatched ? ["missing"] : []);

        Assert.Equal("needsReview", result.Verdict);
        Assert.Empty(result.Safe);
        Assert.Contains("No deletion safety", result.Notice);
    }

    [Fact]
    public void BuildResult_UsesRetailLabelsAndPreservesMissingPricing()
    {
        var resource = new PreValidationResourceResult("resource", "disk", "microsoft.compute/disks", ["rec"],
            "safe", [], [], 100, null);
        var result = PreValidationService.BuildResult("type", [resource], Now, 30, false, []);

        Assert.Equal("Retail", result.PricingBasis);
        Assert.Equal("USD", result.Currency);
        var summary = Assert.Single(result.Summary, static bucket => bucket.Verdict == "safe");
        Assert.Equal(100, summary.YearlyPotentialSavings);
        Assert.Null(summary.Cost);
        Assert.Equal(1, summary.MissingCostCount);
        var json = JsonSerializer.Serialize(result, AdvisorJsonContext.Default.PreValidationResult);
        var restored = JsonSerializer.Deserialize(json, AdvisorJsonContext.Default.PreValidationResult);
        Assert.NotNull(restored);
        Assert.Single(restored.Safe);
        Assert.Contains("no deletion or state update is authorized", restored.Notice);
    }

    [Theory]
    [InlineData("safe")]
    [InlineData("needsReview")]
    [InlineData("blocked")]
    public void BuildResult_RoundsOutputAfterSummingFullPrecisionAmounts(string verdict)
    {
        var first = new PreValidationResourceResult("first", "disk", "microsoft.compute/disks", ["rec1"],
            verdict, [], [], 1.005m, -1.005m);
        var second = first with { ResourceId = "second", Cost = 0m };
        var missing = first with { ResourceId = "missing", YearlyPotentialSavings = null, Cost = null };
        var result = PreValidationService.BuildResult("type", [first, second, missing], Now, 30, false, []);

        var json = JsonSerializer.Serialize(result, AdvisorJsonContext.Default.PreValidationResult);
        var restored = JsonSerializer.Deserialize(json, AdvisorJsonContext.Default.PreValidationResult);
        Assert.NotNull(restored);
        var resources = verdict switch
        {
            "safe" => restored.Safe,
            "needsReview" => restored.NeedsReview,
            _ => restored.Blocked,
        };
        Assert.Equal(["first", "second", "missing"], resources.Select(static resource => resource.ResourceId));
        Assert.Equal(1.01m, resources[0].YearlyPotentialSavings);
        Assert.Equal(-1.01m, resources[0].Cost);
        Assert.Equal(1.01m, resources[1].YearlyPotentialSavings);
        Assert.Equal(0m, resources[1].Cost);
        Assert.Null(resources[2].YearlyPotentialSavings);
        Assert.Null(resources[2].Cost);
        var summary = Assert.Single(restored.Summary, bucket => bucket.Verdict == verdict);
        Assert.Equal(2.01m, summary.YearlyPotentialSavings);
        Assert.Equal(-1.01m, summary.Cost);
        Assert.Equal(1, summary.MissingSavingsCount);
        Assert.Equal(1, summary.MissingCostCount);
        Assert.All(restored.Summary.Where(bucket => bucket.Verdict != verdict), static bucket =>
        {
            Assert.Null(bucket.YearlyPotentialSavings);
            Assert.Null(bucket.Cost);
        });
        Assert.Equal(1.005m, first.YearlyPotentialSavings);
        Assert.Equal(-1.005m, first.Cost);
    }

    [Fact]
    public void BuildResult_ConfirmedDeletedStateAndCountsRoundTripWithTheExistingJsonContext()
    {
        var deleted = new PreValidationResourceResult("deleted-resource", "disk", "microsoft.compute/disks", ["rec"],
            "blocked", [], [], 100, null, Now.AddDays(-2), "Already deleted. No action needed.", false);
        var locked = new PreValidationResourceResult("locked-resource", "disk", "microsoft.compute/disks", ["rec2"],
            "blocked", [], [], null, null);
        var result = PreValidationService.BuildResult("type", [deleted, locked], Now, 30, false, []);

        var json = JsonSerializer.Serialize(result, AdvisorJsonContext.Default.PreValidationResult);
        var restored = JsonSerializer.Deserialize(json, AdvisorJsonContext.Default.PreValidationResult);

        Assert.NotNull(restored);
        Assert.Empty(restored.Safe);
        var summary = Assert.Single(restored.Summary, static bucket => bucket.Verdict == "blocked");
        Assert.Equal(2, summary.Count);
        Assert.Equal(1, summary.DeletedCount);
        Assert.Equal(100, summary.YearlyPotentialSavings);
        Assert.Equal(1, summary.MissingSavingsCount);
        var resource = Assert.Single(restored.Blocked, static resource => resource.ResourceId == "deleted-resource");
        Assert.Equal(Now.AddDays(-2), resource.ResourceDeletedOn);
        Assert.Equal(deleted.Reason, resource.Reason);
        Assert.False(resource.CanPostponeDismiss);
        Assert.Null(Assert.Single(restored.Blocked, static resource => resource.ResourceId == "locked-resource").CanPostponeDismiss);
        Assert.All(restored.Summary.Where(static bucket => bucket.Verdict != "blocked"),
            static bucket => Assert.Equal(0, bucket.DeletedCount));
    }

    private static MetricResult Metric(string name) => new()
    {
        Name = name,
        TimeSeries = [new()
        {
            Start = Now.AddDays(-31).UtcDateTime, End = Now.UtcDateTime,
            Interval = "P1D", TotalBuckets = new double[32],
        }],
    };
}
