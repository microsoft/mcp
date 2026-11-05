// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Services;

public sealed class UnusedAgePolicyTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 30, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(1, "warning")]
    [InlineData(29, "warning")]
    [InlineData(30, "passed")]
    [InlineData(45, "passed")]
    public void Evaluate_EnforcesTheThirtyDayBoundary(int days, string outcome)
    {
        var check = UnusedAgePolicy.Evaluate([new(Now.AddDays(-days))], Now, 30);

        Assert.Equal(outcome, check.Outcome);
        Assert.Equal(days, check.DaysUnused);
        Assert.Equal("warn", check.Severity);
    }

    [Fact]
    public void Evaluate_UsesTheMostRecentSignal()
    {
        var check = UnusedAgePolicy.Evaluate([new(Now.AddDays(-60)), new(Now.AddDays(-18))], Now, 30);

        Assert.Equal("warning", check.Outcome);
        Assert.Equal(18, check.DaysUnused);
        Assert.Contains("migration", check.Evidence);
    }

    [Fact]
    public void Evaluate_UnknownSignalNeverPasses()
    {
        Assert.Equal("unverified", UnusedAgePolicy.Evaluate([], Now, 30).Outcome);
        Assert.Equal("unverified", UnusedAgePolicy.Evaluate([default], Now, 30).Outcome);
        Assert.Equal("unverified", UnusedAgePolicy.Evaluate([new(Now.AddDays(-60)), default], Now, 30).Outcome);
    }

    [Fact]
    public void Evaluate_ShortMetricHistoryDoesNotBecomeAThirtyDayProof()
    {
        var check = UnusedAgePolicy.Evaluate([new(Now.AddDays(-60)), new(QuietWindowDays: 14)], Now, 30);

        Assert.Equal("unverified", check.Outcome);
        Assert.Equal("passed", UnusedAgePolicy.Evaluate([new(QuietWindowDays: 30)], Now, 30).Outcome);
    }

    [Fact]
    public void Evaluate_FutureActivityRequiresReview()
    {
        var check = UnusedAgePolicy.Evaluate([new(Now.AddDays(1))], Now, 30);

        Assert.Equal("warning", check.Outcome);
        Assert.Equal(0, check.DaysUnused);
    }

    [Fact]
    public void Evaluate_SupportsAnExplicitThreshold()
    {
        Assert.Equal("passed", UnusedAgePolicy.Evaluate([new(Now.AddDays(-15))], Now, 15).Outcome);
        Assert.Equal("warning", UnusedAgePolicy.Evaluate([new(Now.AddDays(-15).AddTicks(1))], Now, 15).Outcome);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(366)]
    public void Evaluate_RejectsInvalidThresholds(int days) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => UnusedAgePolicy.Evaluate([], Now, days));
}
