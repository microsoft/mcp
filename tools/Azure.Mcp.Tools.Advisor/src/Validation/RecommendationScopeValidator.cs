// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Advisor.Options.Recommendation;
using Azure.Mcp.Tools.Advisor.Services;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Advisor.Validation;

internal static class RecommendationScopeValidator
{
    internal const int MaxServiceGroupIdLength = 250;

    internal static void PostBindOptions(
        IRecommendationScopeOptions options,
        ISubscriptionResolver subscriptionResolver)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(subscriptionResolver);

        var readOptions = options as IRecommendationReadScopeOptions;
        var serviceGroupWasProvided = options.ServiceGroup is not null;
        options.ServiceGroup = options.ServiceGroup?.Trim();
        options.Subscription = options.Subscription?.Trim('"', '\'');
        if (readOptions is not null)
        {
            readOptions.Subscriptions = NormalizeScopes(readOptions.Subscriptions);
        }

        if (!serviceGroupWasProvided)
        {
            if (readOptions?.Subscriptions is not { Length: > 0 })
            {
                options.Subscription = subscriptionResolver.ResolveSubscription(options.Subscription);
            }

            options.Subscription = options.Subscription?.Trim('"', '\'');
        }
    }

    internal static void ValidateOptions(
        IRecommendationScopeOptions options,
        ValidationResult validationResult)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(validationResult);

        var readOptions = options as IRecommendationReadScopeOptions;
        if (readOptions?.Subscriptions?.Any(string.IsNullOrWhiteSpace) == true)
        {
            validationResult.Errors.Add("Each explicit subscription scope must be non-empty.");
        }

        var subscriptionWasProvided =
            options.Subscription is not null ||
            readOptions?.Subscriptions is { Length: > 0 };
        var serviceGroupWasProvided = options.ServiceGroup is not null;
        if (subscriptionWasProvided && serviceGroupWasProvided)
        {
            validationResult.Errors.Add(readOptions?.Subscriptions is { Length: > 0 }
                ? "Specify either --subscription/--subscriptions or --service-group, not both."
                : "Specify either --subscription or --service-group, not both.");
        }
        else if (string.IsNullOrWhiteSpace(options.Subscription) &&
            readOptions?.Subscriptions is not { Length: > 0 } &&
            string.IsNullOrWhiteSpace(options.ServiceGroup))
        {
            validationResult.Errors.Add(readOptions is null
                ? "Missing Required options: --subscription or --service-group."
                : "Missing Required options: --subscription, --subscriptions, or --service-group.");
        }

        if (options.ServiceGroup is not null && !IsValidServiceGroupInput(options.ServiceGroup))
        {
            validationResult.Errors.Add(
                "Specify a Service Group name or ID of 1 to 250 characters, or its full ARM resource ID. Names cannot contain path separators or control characters.");
        }
    }

    internal static void ThrowIfInvalidServiceGroupInput(string serviceGroup)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceGroup);
        if (!IsValidServiceGroupInput(serviceGroup.Trim()))
        {
            throw new ArgumentException(
                "Specify a Service Group name or ID of 1 to 250 characters, or its full ARM resource ID. Names cannot contain path separators or control characters.",
                nameof(serviceGroup));
        }
    }

    private static bool IsValidServiceGroupInput(string serviceGroup) =>
        serviceGroup.StartsWith(RecommendationQueryBuilder.ServiceGroupResourceIdPrefix, StringComparison.OrdinalIgnoreCase)
            ? IsValidServiceGroupId(serviceGroup[RecommendationQueryBuilder.ServiceGroupResourceIdPrefix.Length..])
            : !string.IsNullOrWhiteSpace(serviceGroup) &&
                serviceGroup.Length <= MaxServiceGroupIdLength &&
                serviceGroup.All(static character => !char.IsControl(character) && character is not '/' and not '\\');

    internal static void ThrowIfInvalidServiceGroupId(string serviceGroup)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceGroup);
        if (!IsValidServiceGroupId(serviceGroup))
        {
            throw new ArgumentException(
                "The service group ID must be 1 to 250 characters and contain only letters, numbers, hyphens, underscores, periods, parentheses, or tildes.",
                nameof(serviceGroup));
        }
    }

    internal static bool IsValidServiceGroupId(string serviceGroup) =>
        serviceGroup.Length is >= 1 and <= MaxServiceGroupIdLength &&
        serviceGroup.All(IsValidServiceGroupIdCharacter);

    internal static string[] GetSubscriptions(IRecommendationScopeOptions options) =>
        NormalizeScopes(RecommendationFilterValidator.CombineValues(
            options.Subscription,
            (options as IRecommendationReadScopeOptions)?.Subscriptions));

    private static string[] NormalizeScopes(IEnumerable<string>? values) =>
        (values ?? [])
            .Select(static value => value?.Trim('"', '\'', ' ') ?? string.Empty)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static bool IsValidServiceGroupIdCharacter(char character) =>
        char.IsLetterOrDigit(character) || character is '-' or '_' or '.' or '(' or ')' or '~';
}
