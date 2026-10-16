// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Azure.Mcp.Tools.Advisor.Options.Recommendation;
using Azure.Mcp.Tools.Advisor.Services;
using Azure.Mcp.Tools.Advisor.Validation;

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

    public static void AddScopeTags(Activity? activity, IRecommendationScopeOptions options)
    {
        AddTagIfPresent(activity, Subscription, NormalizeSubscriptionId(options.Subscription));
        AddTagIfPresent(activity, ServiceGroup, HashServiceGroup(options.ServiceGroup));
    }

    public static void AddRecommendationTags(
        Activity? activity,
        string? recommendationTypeId = null,
        string? category = null,
        string? subCategory = null,
        string? impact = null)
    {
        AddTagIfPresent(activity, RecommendationTypeId, RecommendationFilterValidator.NormalizeRecommendationTypeId(recommendationTypeId));
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

    private static string? HashServiceGroup(string? serviceGroup) =>
        string.IsNullOrWhiteSpace(serviceGroup)
            ? null
            : Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(serviceGroup.Trim().ToLowerInvariant())));

    private static void AddTagIfPresent(Activity? activity, string key, string? value)
    {
        if (activity is null || string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        activity.AddTag(key, value);
    }
}
