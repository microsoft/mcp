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

        var serviceGroupWasProvided = options.ServiceGroup is not null;
        options.ServiceGroup = options.ServiceGroup?.Trim();
        options.Subscription = options.Subscription?.Trim('"', '\'');

        if (!serviceGroupWasProvided)
        {
            options.Subscription = subscriptionResolver.ResolveSubscription(options.Subscription);
            options.Subscription = options.Subscription?.Trim('"', '\'');
        }
    }

    internal static void ValidateOptions(
        IRecommendationScopeOptions options,
        ValidationResult validationResult)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(validationResult);

        var subscriptionWasProvided = options.Subscription is not null;
        var serviceGroupWasProvided = options.ServiceGroup is not null;
        if (subscriptionWasProvided && serviceGroupWasProvided)
        {
            validationResult.Errors.Add("Specify either --subscription or --service-group, not both.");
        }
        else if (string.IsNullOrWhiteSpace(options.Subscription) &&
            string.IsNullOrWhiteSpace(options.ServiceGroup))
        {
            validationResult.Errors.Add("Missing Required options: --subscription or --service-group.");
        }

        if (serviceGroupWasProvided && !IsValidServiceGroupId(options.ServiceGroup!))
        {
            validationResult.Errors.Add(
                "The service group ID must be 1 to 250 characters and contain only letters, numbers, hyphens, underscores, periods, parentheses, or tildes.");
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

    private static bool IsValidServiceGroupIdCharacter(char character) =>
        char.IsLetterOrDigit(character) || character is '-' or '_' or '.' or '(' or ')' or '~';
}
