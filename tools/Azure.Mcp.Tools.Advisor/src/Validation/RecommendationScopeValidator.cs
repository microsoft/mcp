// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Advisor.Options.Recommendation;
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

        var serviceGroupWasProvided = options.ServiceGroup is not null || options.ServiceGroups is { Length: > 0 };
        options.ServiceGroup = options.ServiceGroup?.Trim();
        options.ServiceGroups = options.ServiceGroups?
            .Where(static serviceGroup => !string.IsNullOrWhiteSpace(serviceGroup))
            .Select(static serviceGroup => serviceGroup.Trim('"', '\'', ' '))
            .Where(static serviceGroup => !string.IsNullOrWhiteSpace(serviceGroup))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        options.Subscription = options.Subscription?.Trim('"', '\'');
        options.Subscriptions = options.Subscriptions?
            .Where(static subscription => !string.IsNullOrWhiteSpace(subscription))
            .Select(static subscription => subscription.Trim('"', '\'', ' '))
            .Where(static subscription => !string.IsNullOrWhiteSpace(subscription))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (!serviceGroupWasProvided)
        {
            if (options.Subscriptions is not { Length: > 0 })
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

        var subscriptionWasProvided =
            options.Subscription is not null ||
            options.Subscriptions is { Length: > 0 };
        var serviceGroupWasProvided = options.ServiceGroup is not null || options.ServiceGroups is { Length: > 0 };
        if (subscriptionWasProvided && serviceGroupWasProvided)
        {
            validationResult.Errors.Add("Specify either --subscription or --service-group, not both. The plural scope options are mutually exclusive with the opposite scope family.");
        }
        else if (string.IsNullOrWhiteSpace(options.Subscription) &&
            options.Subscriptions is not { Length: > 0 } &&
            string.IsNullOrWhiteSpace(options.ServiceGroup) &&
            options.ServiceGroups is not { Length: > 0 })
        {
            validationResult.Errors.Add("Missing Required options: --subscription, --subscriptions, --service-group, or --service-groups.");
        }

        var serviceGroups = (options.ServiceGroups ?? [])
            .Concat(string.IsNullOrWhiteSpace(options.ServiceGroup) ? [] : [options.ServiceGroup]);
        foreach (var serviceGroup in serviceGroups)
        {
            if (!IsValidServiceGroupId(serviceGroup))
            {
                validationResult.Errors.Add(
                    "Each service group ID must be 1 to 250 characters and contain only letters, numbers, hyphens, underscores, periods, parentheses, or tildes.");
            }
        }
    }

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

    internal static bool HasMultipleScopes(IRecommendationScopeOptions options) =>
        CountDistinctValues(options.Subscription, options.Subscriptions) > 1 ||
        CountDistinctValues(options.ServiceGroup, options.ServiceGroups) > 1;

    private static int CountDistinctValues(string? singular, string[]? plural) =>
        (plural ?? [])
            .Append(singular)
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

    private static bool IsValidServiceGroupIdCharacter(char character) =>
        char.IsLetterOrDigit(character) || character is '-' or '_' or '.' or '(' or ')' or '~';
}
