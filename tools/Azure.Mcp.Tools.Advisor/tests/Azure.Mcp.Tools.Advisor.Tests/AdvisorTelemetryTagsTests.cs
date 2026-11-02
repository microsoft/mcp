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
        Assert.Equal("advisor/Impact", AdvisorTelemetryTags.Impact);
        Assert.Equal("advisor/GroupBy", AdvisorTelemetryTags.GroupBy);
        Assert.Equal("advisor/Prioritized", AdvisorTelemetryTags.Prioritized);
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
            "ServiceUpgradeAndRetirement",
            "High");

        Assert.Equal("262c35d4-83fe-457f-afa7-cac774c371d8", activity.GetTagItem(AdvisorTelemetryTags.RecommendationTypeId));
        Assert.Equal("HighAvailability", activity.GetTagItem(AdvisorTelemetryTags.Category));
        Assert.Equal("ServiceUpgradeAndRetirement", activity.GetTagItem(AdvisorTelemetryTags.SubCategory));
        Assert.Equal("High", activity.GetTagItem(AdvisorTelemetryTags.Impact));
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

        AdvisorTelemetryTags.AddRecommendationTags(activity, value, value, value, value);

        Assert.Empty(activity.TagObjects);
    }

    [Fact]
    public void AddQueryTags_NullActivity_DoesNotThrow()
    {
        AdvisorTelemetryTags.AddQueryTags(null, true, "category");
    }

    [Theory]
    [InlineData(true, "True")]
    [InlineData(false, "False")]
    public void AddQueryTags_WithPrioritized_SetsPrioritizedTag(bool prioritized, string expected)
    {
        using var activity = StartActivity();

        AdvisorTelemetryTags.AddQueryTags(activity, prioritized);

        Assert.Equal(expected, activity.GetTagItem(AdvisorTelemetryTags.Prioritized));
        Assert.Null(activity.GetTagItem(AdvisorTelemetryTags.GroupBy));
    }

    [Fact]
    public void AddQueryTags_WithGroupBy_SetsGroupByTag()
    {
        using var activity = StartActivity();

        AdvisorTelemetryTags.AddQueryTags(activity, null, "impact");

        Assert.Equal("impact", activity.GetTagItem(AdvisorTelemetryTags.GroupBy));
        Assert.Null(activity.GetTagItem(AdvisorTelemetryTags.Prioritized));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddQueryTags_NullPrioritizedAndBlankGroupBy_SkipsTags(string? groupBy)
    {
        using var activity = StartActivity();

        AdvisorTelemetryTags.AddQueryTags(activity, null, groupBy);

        Assert.Empty(activity.TagObjects);
    }

    [Theory]
    [InlineData("53B36492-771D-40CB-8527-DC4C191366A7", "53b36492-771d-40cb-8527-dc4c191366a7")]
    [InlineData(" 53b36492-771d-40cb-8527-dc4c191366a7 ", "53b36492-771d-40cb-8527-dc4c191366a7")]
    public void AddScopeTags_SubscriptionGuid_SetsNormalizedGuid(string subscription, string expected)
    {
        using var activity = StartActivity();

        AdvisorTelemetryTags.AddScopeTags(activity, new RecommendationListOptions { Subscription = subscription });

        Assert.Equal(expected, activity.GetTagItem(AdvisorTelemetryTags.Subscription));
    }

    [Fact]
    public void AddScopeTags_SubscriptionName_SkipsSubscriptionTag()
    {
        using var activity = StartActivity();

        AdvisorTelemetryTags.AddScopeTags(activity, new RecommendationListOptions { Subscription = "Contoso Production" });

        Assert.Null(activity.GetTagItem(AdvisorTelemetryTags.Subscription));
    }

    [Fact]
    public void AddRecommendationTags_KnownValuesInAnyCase_SetsCanonicalValues()
    {
        using var activity = StartActivity();

        AdvisorTelemetryTags.AddRecommendationTags(
            activity,
            "262C35D4-83FE-457F-AFA7-CAC774C371D8",
            " highavailability ",
            " ServiceUpgradeAndRetirement ",
            "HIGH");

        Assert.Equal("262c35d4-83fe-457f-afa7-cac774c371d8", activity.GetTagItem(AdvisorTelemetryTags.RecommendationTypeId));
        Assert.Equal("HighAvailability", activity.GetTagItem(AdvisorTelemetryTags.Category));
        Assert.Equal("ServiceUpgradeAndRetirement", activity.GetTagItem(AdvisorTelemetryTags.SubCategory));
        Assert.Equal("High", activity.GetTagItem(AdvisorTelemetryTags.Impact));
    }

    [Fact]
    public void AddRecommendationTags_UnknownValues_SkipsTags()
    {
        using var activity = StartActivity();

        AdvisorTelemetryTags.AddRecommendationTags(
            activity,
            recommendationTypeId: "not-a-guid",
            category: "user@contoso.com",
            impact: "Critical");

        Assert.Empty(activity.TagObjects);
    }

    [Theory]
    [InlineData("IMPACT", "impact")]
    [InlineData(" resource-type ", "resource-type")]
    public void AddQueryTags_KnownGroupByInAnyCase_SetsCanonicalValue(string groupBy, string expected)
    {
        using var activity = StartActivity();

        AdvisorTelemetryTags.AddQueryTags(activity, null, groupBy);

        Assert.Equal(expected, activity.GetTagItem(AdvisorTelemetryTags.GroupBy));
    }

    [Fact]
    public void AddQueryTags_UnknownGroupBy_SkipsGroupByTag()
    {
        using var activity = StartActivity();

        AdvisorTelemetryTags.AddQueryTags(activity, null, "customer-name");

        Assert.Empty(activity.TagObjects);
    }

    private Activity StartActivity()
    {
        var activity = _source.StartActivity("test-op");
        Assert.NotNull(activity);
        return activity;
    }
}
