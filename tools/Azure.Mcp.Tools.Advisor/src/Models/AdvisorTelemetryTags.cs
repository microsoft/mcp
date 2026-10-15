// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Azure.Mcp.Tools.Advisor.Options.Recommendation;

namespace Azure.Mcp.Tools.Advisor.Models;

public static class AdvisorTelemetryTags
{
    private static string AddPrefix(string tagName) => $"advisor/{tagName}";

    public const string Subscription = "subscription";

    public static readonly string ServiceGroup = AddPrefix("ServiceGroup");
    public static readonly string RecommendationTypeId = AddPrefix("RecommendationTypeId");
    public static readonly string Category = AddPrefix("Category");
    public static readonly string SubCategory = AddPrefix("SubCategory");
    public static readonly string Impact = AddPrefix("Impact");
    public static readonly string GroupBy = AddPrefix("GroupBy");
    public static readonly string Prioritized = AddPrefix("Prioritized");

    /// <summary>
    /// Adds the subscription and service group scope tags. Null or blank values are skipped.
    /// </summary>
    public static void AddScopeTags(Activity? activity, IRecommendationScopeOptions options)
    {
        AddTagIfPresent(activity, Subscription, options.Subscription);
        AddTagIfPresent(activity, ServiceGroup, options.ServiceGroup);
    }

    /// <summary>
    /// Adds recommendation filter tags. Null or blank values are skipped.
    /// </summary>
    public static void AddRecommendationTags(
        Activity? activity,
        string? recommendationTypeId = null,
        string? category = null,
        string? subCategory = null,
        string? impact = null)
    {
        AddTagIfPresent(activity, RecommendationTypeId, recommendationTypeId);
        AddTagIfPresent(activity, Category, category);
        AddTagIfPresent(activity, SubCategory, subCategory);
        AddTagIfPresent(activity, Impact, impact);
    }

    /// <summary>
    /// Adds query shape tags for prioritized mode and summary grouping. Null or blank values are skipped.
    /// </summary>
    public static void AddQueryTags(Activity? activity, bool? prioritized, string? groupBy = null)
    {
        AddTagIfPresent(activity, Prioritized, prioritized?.ToString());
        AddTagIfPresent(activity, GroupBy, groupBy);
    }

    private static void AddTagIfPresent(Activity? activity, string key, string? value)
    {
        if (activity is null || string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        activity.AddTag(key, value);
    }
}
