// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Azure.Mcp.Tools.Advisor.Options.Recommendation;
using Azure.Mcp.Tools.Advisor.Services;
using Azure.Mcp.Tools.Advisor.Validation;

namespace Azure.Mcp.Tools.Advisor.Models;

public static class AdvisorTelemetryTags
{
    private static string AddPrefix(string tagName) => $"advisor/{tagName}";

    public const string Subscription = "subscription";
    public static readonly string SubscriptionCount = AddPrefix("SubscriptionCount");

    public static readonly string ServiceGroup = AddPrefix("ServiceGroup");
    public static readonly string RecommendationTypeId = AddPrefix("RecommendationTypeId");
    public static readonly string RecommendationTypeCount = AddPrefix("RecommendationTypeCount");
    public static readonly string Category = AddPrefix("Category");
    public static readonly string SubCategory = AddPrefix("SubCategory");
    public static readonly string Impact = AddPrefix("Impact");
    public static readonly string GroupBy = AddPrefix("GroupBy");
    public static readonly string Prioritized = AddPrefix("Prioritized");

    public static void AddScopeTags(Activity? activity, IRecommendationScopeOptions options)
    {
        var subscriptions = RecommendationScopeValidator.GetSubscriptions(options);
        if (subscriptions.Length > 1)
        {
            activity?.AddTag(SubscriptionCount, subscriptions.Length);
        }
        else
        {
            AddTagIfPresent(activity, Subscription, NormalizeSubscriptionId(subscriptions.SingleOrDefault()));
        }
        AddTagIfPresent(activity, ServiceGroup, options.ServiceGroup);
    }

    public static void AddRecommendationTags(
        Activity? activity,
        string? recommendationTypeId = null,
        string? category = null,
        string? subCategory = null,
        string? impact = null,
        IReadOnlyList<string>? recommendationTypeIds = null)
    {
        var typeIds = RecommendationFilterValidator.NormalizeRecommendationTypeIds(
            RecommendationFilterValidator.CombineValues(recommendationTypeId, recommendationTypeIds));
        if (typeIds.Count > 1)
        {
            activity?.AddTag(RecommendationTypeCount, typeIds.Count);
        }
        else
        {
            AddTagIfPresent(activity, RecommendationTypeId, typeIds.SingleOrDefault());
        }
        AddTagIfPresent(activity, Category, RecommendationFilterValidator.NormalizeAllowedValue(category, RecommendationFilterValidator.AllowedCategories));
        AddTagIfPresent(activity, SubCategory, subCategory?.Trim());
        AddTagIfPresent(activity, Impact, RecommendationFilterValidator.NormalizeAllowedValue(impact, RecommendationFilterValidator.AllowedImpacts));
    }

    public static void AddQueryTags(Activity? activity, bool? prioritized, string? groupBy = null)
    {
        AddTagIfPresent(activity, Prioritized, prioritized?.ToString());
        AddTagIfPresent(activity, GroupBy, RecommendationFilterValidator.NormalizeAllowedValue(groupBy, RecommendationSummaryService.AllowedGroupBy));
    }

    private static string? NormalizeSubscriptionId(string? subscription) =>
        Guid.TryParse(subscription?.Trim(), out var subscriptionId) ? subscriptionId.ToString("D") : null;

    private static void AddTagIfPresent(Activity? activity, string key, string? value)
    {
        if (activity is null || string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        activity.AddTag(key, value);
    }
}
