// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Options.Recommendation;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests;

public sealed class AdvisorTelemetryTagsTests : IDisposable
{
    private readonly ActivityListener _listener;
    private readonly ActivitySource _source;

    public AdvisorTelemetryTagsTests()
    {
        _listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
        };
        ActivitySource.AddActivityListener(_listener);
        _source = new ActivitySource("test");
    }

    public void Dispose()
    {
        _source.Dispose();
        _listener.Dispose();
    }

    [Fact]
    public void TagNames_UseExpectedKeys()
    {
        Assert.Equal("subscription", AdvisorTelemetryTags.Subscription);
        Assert.Equal("advisor/ServiceGroup", AdvisorTelemetryTags.ServiceGroup);
        Assert.Equal("advisor/RecommendationTypeId", AdvisorTelemetryTags.RecommendationTypeId);
        Assert.Equal("advisor/Category", AdvisorTelemetryTags.Category);
        Assert.Equal("advisor/SubCategory", AdvisorTelemetryTags.SubCategory);
    }

    [Fact]
    public void AddScopeTags_NullActivity_DoesNotThrow()
    {
        AdvisorTelemetryTags.AddScopeTags(null, new RecommendationListOptions { Subscription = "sub" });
    }

    [Fact]
    public void AddScopeTags_WithSubscription_SetsSubscriptionTagOnly()
    {
        using var activity = StartActivity();

        AdvisorTelemetryTags.AddScopeTags(activity, new RecommendationListOptions { Subscription = "00000000-0000-0000-0000-000000000001" });

        Assert.Equal("00000000-0000-0000-0000-000000000001", activity.GetTagItem(AdvisorTelemetryTags.Subscription));
        Assert.Null(activity.GetTagItem(AdvisorTelemetryTags.ServiceGroup));
    }

    [Fact]
    public void AddScopeTags_WithServiceGroup_SetsServiceGroupTagOnly()
    {
        using var activity = StartActivity();

        AdvisorTelemetryTags.AddScopeTags(activity, new RecommendationSummaryOptions { ServiceGroup = "sg-contoso" });

        Assert.Equal("sg-contoso", activity.GetTagItem(AdvisorTelemetryTags.ServiceGroup));
        Assert.Null(activity.GetTagItem(AdvisorTelemetryTags.Subscription));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddScopeTags_NullOrBlankValues_SkipsTags(string? value)
    {
        using var activity = StartActivity();

        AdvisorTelemetryTags.AddScopeTags(activity, new RecommendationListOptions { Subscription = value, ServiceGroup = value });

        Assert.Empty(activity.TagObjects);
    }

    [Fact]
    public void AddRecommendationTags_NullActivity_DoesNotThrow()
    {
        AdvisorTelemetryTags.AddRecommendationTags(null, "type-id", "Cost", "ServiceUpgradeAndRetirement");
    }

    [Fact]
    public void AddRecommendationTags_WithValues_SetsAllTags()
    {
        using var activity = StartActivity();

        AdvisorTelemetryTags.AddRecommendationTags(
            activity,
            "262c35d4-83fe-457f-afa7-cac774c371d8",
            "HighAvailability",
            "ServiceUpgradeAndRetirement");

        Assert.Equal("262c35d4-83fe-457f-afa7-cac774c371d8", activity.GetTagItem(AdvisorTelemetryTags.RecommendationTypeId));
        Assert.Equal("HighAvailability", activity.GetTagItem(AdvisorTelemetryTags.Category));
        Assert.Equal("ServiceUpgradeAndRetirement", activity.GetTagItem(AdvisorTelemetryTags.SubCategory));
    }

    [Fact]
    public void AddRecommendationTags_OnlySomeValues_SetsOnlyThoseTags()
    {
        using var activity = StartActivity();

        AdvisorTelemetryTags.AddRecommendationTags(activity, category: "Cost");

        Assert.Equal("Cost", activity.GetTagItem(AdvisorTelemetryTags.Category));
        Assert.Null(activity.GetTagItem(AdvisorTelemetryTags.RecommendationTypeId));
        Assert.Null(activity.GetTagItem(AdvisorTelemetryTags.SubCategory));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddRecommendationTags_NullOrBlankValues_SkipsTags(string? value)
    {
        using var activity = StartActivity();

        AdvisorTelemetryTags.AddRecommendationTags(activity, value, value, value);

        Assert.Empty(activity.TagObjects);
    }

    private Activity StartActivity()
    {
        var activity = _source.StartActivity("test-op");
        Assert.NotNull(activity);
        return activity;
    }
}
